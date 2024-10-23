using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_API.Utils;
using System.Text.Json;
using System.Text;

namespace StudentCertificatePortal_API.Controllers
{
    public class CheckoutController : ApiControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ICheckoutService _service;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWalletService _walletService;
        private readonly ITransactionService _transactionService;

        public CheckoutController(ICheckoutService service, IHttpClientFactory httpClientFactory
            , IConfiguration configuration, IWalletService walletService, ITransactionService transactionService)
        {
            _service = service;
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _httpClient = _httpClientFactory.CreateClient();
            _configuration = configuration;
            _walletService = walletService;
            _transactionService = transactionService;
        }

        [HttpPost("{transactionId}")]
        public async Task<IActionResult> CreatePaymentLink(int transactionId)
        {

            var data = await _service.CreatePaymentLinkAsync(transactionId, new CancellationToken());
            var checksumKey = _configuration["PayOS:ChecksumKey"];
            var signature = PayOSHelper.CreateSignature(
                data.Amount.ToString(),
                data.CancelUrl,
                data.Description,
                data.OrderCode.ToString(),
                data.ReturnUrl,
                checksumKey
            );

            var url = "https://api-merchant.payos.vn/v2/payment-requests";

            var payload = new
            {
                orderCode = data.OrderCode,
                amount = data.Amount,
                description = data.Description,
                buyerName = data.BuyerName,
                buyerEmail = data.BuyerEmail,
                buyerPhone = data.BuyerPhone,
                buyerAddress = data.BuyerAddress,
                items = data.Items,
                cancelUrl = data.CancelUrl,
                returnUrl = data.ReturnUrl,
                expiredAt = data.ExpiredAt,
                signature = signature,
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            content.Headers.Add("x-client-id", _configuration["PayOS:ClientId"]);
            content.Headers.Add("x-api-key", _configuration["PayOS:APIKey"]);

            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                /*var transaction =await _transactionService.GetTransactionByIdAsync(data.OrderCode,new CancellationToken());
                await _transactionService.UpdateStatusTransactionAsync(transaction.TransactionId, Enums.EnumTransaction.Success, new CancellationToken());
                var wallet = await _walletService.GetWalletByWalletIdAsync(transaction.WalletId, new CancellationToken());
                await _walletService.UpdateWalletAsync(wallet.UserId ?? 0, data.Amount/1000, Enums.EnumWallet.IsUsed, new CancellationToken());*/
                return Ok(responseContent);
            }
            else
            {
                var errorMessage = $"Error: {response.StatusCode}, Reason: {response.ReasonPhrase}";
                return BadRequest(errorMessage);
            }
        }
    }
}

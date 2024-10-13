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

        public CheckoutController(ICheckoutService service, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _service = service;
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _httpClient = _httpClientFactory.CreateClient();
            _configuration = configuration;
        }

        [HttpPost("{orderId}")]
        public async Task<IActionResult> CreatePaymentLink(int orderId)
        {

            var data = await _service.CreatePaymentLinkAsync(orderId, new CancellationToken());
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

            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return Ok(responseContent); // Return the content of the response
            }
            else
            {
                // If request failed, return the error message
                var errorMessage = $"Error: {response.StatusCode}, Reason: {response.ReasonPhrase}";
                return BadRequest(errorMessage);
            }
        }
    }
}

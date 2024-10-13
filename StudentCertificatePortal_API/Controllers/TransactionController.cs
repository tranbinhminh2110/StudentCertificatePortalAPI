/*using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Utils;
using System.Text.Json;
using System.Text;

namespace StudentCertificatePortal_API.Controllers
{
    public class TransactionController: ApiControllerBase
    {

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IPaymentService _service;
        private readonly IHttpClientFactory _httpClientFactory;

        public PaymentController(IHttpClientFactory httpClientFactory, IConfiguration configuration, IPaymentService service)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _httpClient = _httpClientFactory.CreateClient();
            _configuration = configuration;
            _service = service;
        }

        [HttpPost("{orderId}")]
        public async Task<IActionResult> CreatePaymentLink(int orderId)
        {

            var data = await _service.CreatePaymentLinkAsync(orderId, new CancellationToken());
            var checksumKey = _configuration["PayOS:ChecksumKey"];
            var signature = PayOSHelper.CreateSignature(
                data.amount.ToString(),
                data.cancelUrl,
                data.description,
                data.orderCode.ToString(),
                data.returnUrl,
                checksumKey
            );

            var url = "https://api-merchant.payos.vn/v2/payment-requests";

            var payload = new
            {
                orderCode = data.orderCode,
                amount = data.amount,
                description = data.description,
                buyerName = data.buyerName,
                buyerEmail = data.buyerEmail,
                buyerPhone = data.buyerPhone,
                buyerAddress = data.buyerAddress,
                items = data.items,
                cancelUrl = data.cancelUrl,
                returnUrl = data.returnUrl,
                expiredAt = data.expiredAt,
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

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetInfoLinkPayment(int orderId)
        {
            var getUrl = $"https://api-merchant.payos.vn/v2/payment-requests/{orderId}";

            try
            {

                var request = new HttpRequestMessage(HttpMethod.Get, getUrl);


                request.Headers.Add("x-client-id", _configuration["PayOS:ClientId"]);
                request.Headers.Add("x-api-key", _configuration["PayOS:APIKey"]);

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {

                    var responseContent = await response.Content.ReadAsStringAsync();

                    var responseObject = JObject.Parse(responseContent);
                    var status = responseObject["data"]?["status"];

                    if (status != null)
                    {
                        return Ok(status.ToString());
                    }
                    else
                    {
                        return BadRequest("Status not found in response content");
                    }
                }
                else
                {
                    return BadRequest("Failed to retrieve payment information");
                }
            }
            catch (Exception ex)
            {

                return BadRequest($"Error: {ex.Message}");
            }

        }
    }
}
*/
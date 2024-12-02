using Microsoft.AspNetCore.Mvc;

namespace StudentCertificatePortal_API.Controllers
{
    public class BanksController:ApiControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public BanksController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> GetBanks()
        {
            var client = _httpClientFactory.CreateClient();
            try
            {
                var response = await client.GetAsync("https://api.httzip.com/api/bank/list");

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Failed to fetch banks data.");
                }

                var responseData = await response.Content.ReadAsStringAsync();

                return Content(responseData, "application/json");

            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error fetching data: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetAccountLookUp(string bankCode, string accountNumber)
        {
            if (string.IsNullOrEmpty(bankCode) || string.IsNullOrEmpty(accountNumber))
            {
                return BadRequest("Bank code and account number are required.");
            }

            var apiKey = _config["LookUp:ApiKey"];
            var apiSecret = _config["LookUp:ApiSecret"];

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                return StatusCode(500, "API credentials are not configured properly.");
            }

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://api.httzip.com/api/bank/id-lookup-prod");
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            client.DefaultRequestHeaders.Add("x-api-secret", apiSecret);

            try
            {
                var payload = new
                {
                    bank = bankCode,
                    account = accountNumber
                };

                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(payload),
                    System.Text.Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync("", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"API Error: {error}");
                }

                var responseData = await response.Content.ReadAsStringAsync();

                return Content(responseData, "application/json");
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error fetching data: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

    }
}

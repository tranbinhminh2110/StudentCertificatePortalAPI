using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.Services.Interface;

public class TextSimilarityService : ITextSimilarityService
{
    private readonly string _apiKey = "TJkpkTzuSQyfyZawVDbxxwgk6yu8EdNIv2YUcuZu"; 
    private readonly string _apiUrl = "https://api.api-ninjas.com/v1/textsimilarity";

    public async Task<double> GetSimilarityScoreAsync(CompareAnswersRequest request)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);

        var requestBody = new
        {
            text_1 = request.UserAnswer, 
            text_2 = request.SampleAnswer 
        };

        var jsonRequest = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync(_apiUrl, content);

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(responseString);

            return (double)result.similarity;
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"API Request failed: {ex.Message}");
        }
    }
}

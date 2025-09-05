using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AIGenSeeSharpSuite.Backend.Services
{
    public class BaiduAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _bearerToken;
        private readonly ILogger<BaiduAiService> _logger;
        private const string ChatUrl = "https://qianfan.baidubce.com/v2/chat/completions";

        public BaiduAiService(IConfiguration configuration, ILogger<BaiduAiService> logger)
        {
            _httpClient = new HttpClient();
            _logger = logger;
            
            // Try to get token from environment variable first, then from configuration
            _bearerToken = Environment.GetEnvironmentVariable("BAIDU_AI_BEARER_TOKEN") 
                ?? configuration["BaiduAiBearerToken"] 
                ?? throw new InvalidOperationException("BaiduAiBearerToken is not configured. Set BAIDU_AI_BEARER_TOKEN environment variable or add to appsettings.json");
            
            _logger.LogInformation("BaiduAI service initialized successfully");
        }

        public async Task<string> GenerateCodeAsync(string prompt, string model)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _bearerToken);

            var requestBody = new
            {
                model = model, // Using the user-specified model
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(ChatUrl, content);

            var responseString = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                // Throw an exception with the detailed error from Baidu
                throw new HttpRequestException($"Baidu API request failed with status code {response.StatusCode}: {responseString}");
            }

            var responseObject = JObject.Parse(responseString);
            
            // According to the documentation, the content is in choices[0].message.content
            // However, the previous code used "result". We will use the documented path.
            return responseObject["choices"]![0]!["message"]!["content"]!.ToString();
        }
    }
}

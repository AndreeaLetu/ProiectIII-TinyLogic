using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace TinyLogic_ok.Services
{
    public class AiService : IAiService
    {
        private readonly HttpClient _client;
        private readonly string _apiKey = "******************************************************";

        public AiService()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("https://openrouter.ai/api/");
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<string> GenerateAsync(string prompt)
        {
            var requestBody = new
            {
                model = "nvidia/nemotron-3-nano-30b-a3b:free",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var response = await _client.PostAsJsonAsync("v1/chat/completions", requestBody);
            var json = await response.Content.ReadAsStringAsync();

            var obj = JObject.Parse(json);

            return obj["choices"]?[0]?["message"]?["content"]?.ToString()
                   ?? "❌ No response from AI.";
        }
    }
}

using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace TinyLogic_ok.Services
{
    public class AiService : IAiService
    {
        private readonly HttpClient _client;
        private readonly string _apiKey = "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";

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
                model = "amazon/nova-2-lite-v1:free",
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

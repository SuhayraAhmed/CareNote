using System.Text;
using System.Text.Json;

// Implementerar IAIModelStrategy med Mixtral-modellen via Groq API
// Samma funktionalitet som LlamaStrategy men med annan AI-modell
// Utbytbar AI-strategi beroende på användarval

namespace CareNote.Services.AIModels
{
    public class MixtralStrategy : IAIModelStrategy
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public MixtralStrategy(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
        }

        public async Task<string> ImproveJournalTextAsync(string userMessage)
        {
            return await SendAIRequest(userMessage, "mixtral-8x7b-32768", true);
        }

        public async Task<string> GetGeneralResponseAsync(string userMessage)
        {
            return await SendAIRequest(userMessage, "mixtral-8x7b-32768", false);
        }

        private async Task<string> SendAIRequest(string userMessage, string model, bool improve)
        {
            var url = "https://api.groq.com/openai/v1/chat/completions";
            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = improve ?
                            @"Du är expert på omvårdnadsdokumentation..." :
                            @"Du är AI-Dok, en expert på svensk vårddokumentation..."
                    },
                    new
                    {
                        role = "user",
                        content = userMessage
                    }
                },
                max_tokens = improve ? 300 : 500,
                temperature = improve ? 0.3 : 0.7
            };

            var json = JsonSerializer.Serialize(requestBody);
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            request.Headers.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.SendAsync(request);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(jsonResponse);
                var content = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return content?.Trim() ?? "Kunde inte generera svar.";
            }

            return "🔧 AI-tjänsten är tillfälligt otillgänglig. Försök igen senare.";
        }
    }
}


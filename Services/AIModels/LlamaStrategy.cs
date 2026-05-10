using System.Text;
using System.Text.Json;

// Implementerar IAIModelStrategy med LLaMA-modellen via Groq API

namespace CareNote.Services.AIModels
{
    public class LlamaStrategy : IAIModelStrategy
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public LlamaStrategy(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
        }

        public async Task<string> ImproveJournalTextAsync(string userMessage) 
        {
            return await SendAIRequest(userMessage, "llama-3.3-70b-versatile", true);
        }

        public async Task<string> GetGeneralResponseAsync(string userMessage) 
        {
            return await SendAIRequest(userMessage, "llama-3.3-70b-versatile", false);
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
                            @"Du är expert på omvårdnadsdokumentation. Förbättra journaltexter från vårdarens perspektiv..." :
                            @"Du är AI-Dok, en expert på svensk vårddokumentation. Ge råd enligt VIPS och SBAR."
                    },
                    new
                    {
                        role = "user",
                        content = improve ?
                            $"Förbättra denna text till professionell journalanteckning: '{userMessage}'" :
                            userMessage
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


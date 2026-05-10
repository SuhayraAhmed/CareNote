using CareNote.Services.AIModels;
using CareNote.Services.Factories;



namespace CareNote.Services
{
    public class GroqAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private IAIModelStrategy _modelStrategy;

        public GroqAIService(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
            // Standardmodell (LLaMA)
            _modelStrategy = AIModelFactory.Create("llama", _httpClient, _apiKey);
        }

        public void SetModel(string modelName) 
        {
            _modelStrategy = AIModelFactory.Create(modelName, _httpClient, _apiKey);
        }

        public async Task<string> GenerateResponseAsync(string userMessage)
        {
            if (ShouldImproveJournalText(userMessage))
                return await _modelStrategy.ImproveJournalTextAsync(userMessage);

            return await _modelStrategy.GetGeneralResponseAsync(userMessage);
        }

        private bool ShouldImproveJournalText(string message) 
        {
            var improveKeywords = new[]
            {
                "förbättra:", "förbättra ", "skriv om:", "gör professionell:",
                "improve:", "rewrite:", "make professional:"
            };

            var lowerMessage = message.ToLower();
            return improveKeywords.Any(keyword => lowerMessage.Contains(keyword));
        }
    }
    // Används i AIChatController.
}
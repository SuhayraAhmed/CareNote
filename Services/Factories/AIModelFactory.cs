using CareNote.Services.AIModels;


namespace CareNote.Services.Factories
{
    public static class AIModelFactory
    {
        public static IAIModelStrategy Create(string modelName, HttpClient client, string apiKey) // Tar modelName som input och returnerar korrekt IAIModelStrategy
        {
            return modelName.ToLower() switch
            {
                "mixtral" => new MixtralStrategy(client, apiKey),
                _ => new LlamaStrategy(client, apiKey)
            };
        }
    }
}
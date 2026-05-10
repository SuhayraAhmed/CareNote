
namespace CareNote.Services.AIModels
{
    public interface IAIModelStrategy 
    { 
        Task<string> ImproveJournalTextAsync(string userMessage);
        Task<string> GetGeneralResponseAsync(string userMessage);
    }
}


using CareNote.Models;

// Interface för KnowledgeService
// Denna fil hanterar användarfiler, indexering, sökning, borttagning och externa resurser

namespace CareNote.Services
{
    public interface IKnowledgeService
    {
        Task<List<KnowledgeFile>> GetUserFilesAsync(string userId);
        Task ProcessFileAsync(Stream fileStream, string fileName, string userId);
        Task<List<SearchResult>> SearchAsync(string query, string userId);
        Task DeleteFileAsync(int fileId, string userId);
        Task<List<ExternalResource>> SearchExternalSourcesAsync(string query);
    }
}
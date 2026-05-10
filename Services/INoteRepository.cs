using CareNote.Models;

// Interface för enkel Note-lagring
// Hämtar alla anteckningar och lägger till nya

namespace CareNote.Services
{
    public interface INoteRepository
    {
        Task<IEnumerable<NoteModel>> GetAllAsync();
        Task AddAsync(NoteModel note);
    }
    
}
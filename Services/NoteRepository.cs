using CareNote.Models;
 
// Implementerar INoteRepository
// Enkel in-memory lagring av NoteModel

namespace CareNote.Services
{
    public class NoteRepository : INoteRepository
    {
        private readonly List<NoteModel> _notes = new();

        public Task<IEnumerable<NoteModel>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<NoteModel>>(_notes);
        }

        public Task AddAsync(NoteModel note)
        {
            _notes.Add(note);
            return Task.CompletedTask;
        }
        
    }
}
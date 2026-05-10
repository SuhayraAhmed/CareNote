namespace CareNote.Models
{
    public class NoteModel  // Liknande Note, men kan användas för vybindning (ViewModel)
    { 
        public int Id { get; set; }
        public string Patient { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        
    }
}
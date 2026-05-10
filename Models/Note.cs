using System.ComponentModel.DataAnnotations;

namespace CareNote.Models
{
    public class Note
    {
        public int Id { get; set; }
        public string Patient { get; set; }
        public string Text { get; set; }
        
        public Priority Priority { get; set; } = Priority.Normal;

        public DateTime CreatedAt { get; set; }
    }
}
namespace CareNote.Models
{
    public class KnowledgeFile  // KnowledgeFile: representerar en användares uppladdade fil
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.Now;
        public bool IsProcessed { get; set; }
    }

    public class SearchResult // SearchResult: resultat från sökning i filer, med relevanspoäng
    {
        public string FileName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public double Relevance { get; set; }
    }

    public class ExternalResource // ExternalResource: externa resurser (titel, beskrivning, URL, kategori, relevans)
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public int RelevanceScore { get; set; } = 50;
    }
} 
// Används i KnowledgeController för hantering och sökning av filer

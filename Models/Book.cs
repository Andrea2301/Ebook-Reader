namespace Ebook_Reader.Models
{
    public class Book
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string CoverImagePath { get; set; } = string.Empty; // Path to cached cover image
        public double LastReadPosition { get; set; } // Percentage or character index
        public string Description { get; set; } = "No description available.";
        public int TotalPages { get; set; } = 0;
        public int CurrentPage { get; set; } = 0;
    }
}

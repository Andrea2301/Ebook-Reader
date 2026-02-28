using System.Threading.Tasks;

namespace Ebook_Reader.Services.Metadata
{
    public class BookMetadata
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public byte[]? CoverImage { get; set; }
    }

    public interface IMetadataProvider
    {
        bool Supports(string extension);
        Task<BookMetadata> GetMetadataAsync(string filePath);
    }
}

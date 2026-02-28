using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace Ebook_Reader.Services.Metadata
{
    public class DefaultMetadataProvider : IMetadataProvider
    {
        private readonly string[] _supportedExtensions = { ".pdf", ".mobi", ".txt", ".cbz", ".fb2" };

        public bool Supports(string extension)
        {
            return _supportedExtensions.Contains(extension.ToLower());
        }

        public Task<BookMetadata> GetMetadataAsync(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            
            // Clean common patterns like "Book_Title-Author" -> "Book Title"
            string cleanedTitle = fileName.Replace("_", " ").Replace("-", " ");

            return Task.FromResult(new BookMetadata
            {
                Title = cleanedTitle,
                Author = "Unknown Author",
                CoverImage = null // Default covers will be handled by the UI or Service
            });
        }
    }
}

using System.IO;
using System.Threading.Tasks;
using VersOne.Epub;

namespace Ebook_Reader.Services.Metadata
{
    public class EpubMetadataProvider : IMetadataProvider
    {
        public bool Supports(string extension)
        {
            return extension.Equals(".epub", System.StringComparison.OrdinalIgnoreCase);
        }

        public async Task<BookMetadata> GetMetadataAsync(string filePath)
        {
            var epubBook = await EpubReader.ReadBookAsync(filePath);
            return new BookMetadata
            {
                Title = epubBook.Title ?? Path.GetFileNameWithoutExtension(filePath),
                Author = epubBook.Author ?? "Unknown Author",
                CoverImage = epubBook.CoverImage
            };
        }
    }
}

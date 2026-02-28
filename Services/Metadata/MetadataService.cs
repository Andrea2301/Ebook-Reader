using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ebook_Reader.Services.Metadata
{
    public class MetadataService
    {
        private readonly List<IMetadataProvider> _providers;

        public MetadataService()
        {
            _providers = new List<IMetadataProvider>
            {
                new EpubMetadataProvider(),
                new DefaultMetadataProvider()
            };
        }

        public async Task<BookMetadata> GetMetadataAsync(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            var provider = _providers.FirstOrDefault(p => p.Supports(extension));

            if (provider != null)
            {
                return await provider.GetMetadataAsync(filePath);
            }

            // Fallback if no provider supports the extension
            return new BookMetadata
            {
                Title = Path.GetFileNameWithoutExtension(filePath),
                Author = "Unknown Author"
            };
        }
    }
}

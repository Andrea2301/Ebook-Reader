using System.IO;
using System.Threading.Tasks;
using VersOne.Epub;

namespace Ebook_Reader.Services
{
    public class EpubService
    {
        public async Task<EpubBook> OpenBookAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Book file not found.", filePath);
            }
            return await EpubReader.ReadBookAsync(filePath);
        }

        public async Task<string> GetChapterContentAsync(EpubBook book, int chapterIndex)
        {
            if (book.ReadingOrder == null || chapterIndex < 0 || chapterIndex >= book.ReadingOrder.Count)
            {
                return string.Empty;
            }

            var chapter = book.ReadingOrder[chapterIndex];
            return chapter.Content; // This returns HTML content
        }
    }
}

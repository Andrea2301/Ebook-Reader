using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ebook_Reader.Models;
using Ebook_Reader.Services;
using VersOne.Epub;

namespace Ebook_Reader.ViewModels
{
    public enum ReadingTheme
    {
        Default,
        Paper,
        Sepia,
        Night
    }

    public class ReaderViewModel : ViewModelBase
    {
        private readonly EpubService _epubService;
        private readonly MainViewModel _mainViewModel;
        private readonly LibraryService _libraryService;
        private Book? _currentBook;
        private string _content = string.Empty;
        private string _contentPreview = string.Empty;
        private bool _isPdf;
        private int _currentPosition;
        private int _totalPositions;
        private int _textLength;
        private ReadingTheme _currentTheme = ReadingTheme.Default;
        private EpubBook? _currentEpub;

        public Book? CurrentBook
        {
            get => _currentBook;
            private set => SetProperty(ref _currentBook, value);
        }

        public string Content
        {
            get => _content;
            set 
            {
                if (SetProperty(ref _content, value))
                {
                    TextLength = value?.Length ?? 0;
                    UpdateContentPreview(value);
                }
            }
        }

        public string ContentPreview
        {
            get => _contentPreview;
            private set => SetProperty(ref _contentPreview, value);
        }

        public bool IsPdf
        {
            get => _isPdf;
            private set => SetProperty(ref _isPdf, value);
        }

        public int CurrentPosition
        {
            get => _currentPosition;
            set => SetProperty(ref _currentPosition, value);
        }

        public int TotalPositions
        {
            get => _totalPositions;
            set => SetProperty(ref _totalPositions, value);
        }

        public int TextLength
        {
            get => _textLength;
            private set => SetProperty(ref _textLength, value);
        }

        public ReadingTheme CurrentTheme
        {
            get => _currentTheme;
            set => SetProperty(ref _currentTheme, value);
        }

        public ICommand GoBackCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand PreviousCommand { get; }
        public ICommand ChangeThemeCommand { get; }

        public ReaderViewModel(EpubService epubService, MainViewModel mainViewModel, LibraryService libraryService)
        {
            _epubService = epubService;
            _mainViewModel = mainViewModel;
            _libraryService = libraryService;

            GoBackCommand = new RelayCommand(_ => _mainViewModel.NavigateToLibrary());
            NextCommand = new RelayCommand(_ => Navigate(1), _ => !IsPdf && CurrentPosition < TotalPositions - 1);
            PreviousCommand = new RelayCommand(_ => Navigate(-1), _ => !IsPdf && CurrentPosition > 0);
            ChangeThemeCommand = new RelayCommand(theme => 
            {
                if (theme is ReadingTheme t) CurrentTheme = t;
                else if (theme is string s && Enum.TryParse<ReadingTheme>(s, out var parsed)) CurrentTheme = parsed;
            });
        }

        public async void LoadBook(Book book)
        {
            CurrentBook = book;
            Content = string.Empty;
            IsPdf = System.IO.Path.GetExtension(book.FilePath).ToLower() == ".pdf";
            _currentEpub = null;

            if (IsPdf)
            {
                Content = book.FilePath;
                CurrentPosition = book.CurrentPage;
                TotalPositions = book.TotalPages;
            }
            else if (System.IO.Path.GetExtension(book.FilePath).ToLower() == ".epub")
            {
                try
                {
                    _currentEpub = await _epubService.OpenBookAsync(book.FilePath);
                    TotalPositions = _currentEpub.ReadingOrder.Count;
                    
                    // Use saved position or start at chapter 0
                    CurrentPosition = book.CurrentPage;
                    if (CurrentPosition < 0 || CurrentPosition >= TotalPositions) 
                        CurrentPosition = 0;

                    LoadChapter(CurrentPosition);
                    await SkipEmptyChaptersAsync();
                }
                catch (System.Exception ex)
                {
                    Content = $"<h1>Error</h1><p>{ex.Message}</p>";
                }
            }
            else
            {
                Content = "<h1>Format not supported yet</h1>";
            }
        }

        private void LoadChapter(int index)
        {
            if (_currentEpub == null || index < 0 || index >= TotalPositions) return;
            
            var chapter = _currentEpub.ReadingOrder[index];
            var rawContent = chapter.Content ?? string.Empty;
            
            
            System.Diagnostics.Debug.WriteLine($"[EPUB] Loading Chapter {index}. File: {chapter.FilePath}. Length: {rawContent.Length}");
            
            Content = rawContent;
        }

        private void Navigate(int delta)
        {
            if (_currentEpub == null) return;

            int newPos = CurrentPosition + delta;
            if (newPos >= 0 && newPos < TotalPositions)
            {
                CurrentPosition = newPos;
                LoadChapter(CurrentPosition);
                
               
                if (CurrentBook != null)
                {
                    CurrentBook.CurrentPage = CurrentPosition;
                    _libraryService.SaveLibrary();
                }
            }
        }

        private async Task SkipEmptyChaptersAsync()
        {
            if (_currentEpub == null) return;

            // Search for the first chapter with significant text (more than 100 actual letters)
            for (int i = CurrentPosition; i < Math.Min(CurrentPosition + 30, TotalPositions); i++)
            {
                var content = _currentEpub.ReadingOrder[i].Content;
                var textOnly = StripTags(content);
                
                if (!string.IsNullOrWhiteSpace(textOnly) && textOnly.Length > 100)
                {
                    if (i != CurrentPosition)
                    {
                        CurrentPosition = i;
                        LoadChapter(CurrentPosition);
                        
                        if (CurrentBook != null)
                        {
                            CurrentBook.CurrentPage = CurrentPosition;
                            _libraryService.SaveLibrary();
                        }
                    }
                    return;
                }
            }
        }

        private string StripTags(string html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;
            
            var clean = html;
            int limit = 0;
            while (clean.Contains("<") && limit++ < 500) // Safety limit
            {
                int start = clean.IndexOf("<");
                int end = clean.IndexOf(">", start);
                if (end == -1) break;
                clean = clean.Remove(start, end - start + 1);
            }
            return clean.Replace("&nbsp;", " ").Trim();
        }

        private void UpdateContentPreview(string html)
        {
            var clean = StripTags(html);
            if (string.IsNullOrWhiteSpace(clean))
            {
                ContentPreview = "No readable text found in this section.";
                return;
            }

            ContentPreview = clean.Length > 80 ? clean.Substring(0, 80) + "..." : clean;
        }
    }
}

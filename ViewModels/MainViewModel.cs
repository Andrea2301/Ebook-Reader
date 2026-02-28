using System.Windows.Input;
using Ebook_Reader.Models;
using Ebook_Reader.Services;

namespace Ebook_Reader.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase _currentView;
        private readonly LibraryService _libraryService;
        private readonly EpubService _epubService;

        public ViewModelBase CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public LibraryViewModel LibraryVM { get; }
        public BooksViewModel BooksVM { get; }
        public ReaderViewModel ReaderVM { get; }

        public ICommand NavigateToLibraryCommand { get; }
        public ICommand NavigateToBooksCommand { get; }

        public MainViewModel()
        {
            _libraryService = new LibraryService();
            _epubService = new EpubService();

            LibraryVM = new LibraryViewModel(_libraryService, this);
            BooksVM = new BooksViewModel(_libraryService, this);
            ReaderVM = new ReaderViewModel(_epubService, this, _libraryService);

            NavigateToLibraryCommand = new RelayCommand(_ => NavigateToLibrary());
            NavigateToBooksCommand = new RelayCommand(_ => NavigateToBooks());

            CurrentView = LibraryVM;
        }

        public void NavigateToLibrary()
        {
            CurrentView = LibraryVM;
        }

        public void NavigateToBooks()
        {
            CurrentView = BooksVM;
        }

        public void OpenBook(Book book)
        {
            ReaderVM.LoadBook(book);
            CurrentView = ReaderVM;
        }
    }
}

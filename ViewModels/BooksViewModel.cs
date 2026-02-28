using System.Collections.ObjectModel;
using System.Windows.Input;
using Ebook_Reader.Models;
using Ebook_Reader.Services;

namespace Ebook_Reader.ViewModels
{
    public class BooksViewModel : ViewModelBase
    {
        private readonly LibraryService _libraryService;
        private readonly MainViewModel _mainViewModel;

        public ObservableCollection<Book> Books => _libraryService.GetBooks();

        public ICommand OpenBookCommand { get; }

        public BooksViewModel(LibraryService libraryService, MainViewModel mainViewModel)
        {
            _libraryService = libraryService;
            _mainViewModel = mainViewModel;

            OpenBookCommand = new RelayCommand(OpenBook);
        }

        private void OpenBook(object? parameter)
        {
            if (parameter is Book book)
            {
                _mainViewModel.OpenBook(book);
            }
        }
    }
}

using System.Collections.ObjectModel;
using System.Windows.Input;
using Ebook_Reader.Models;
using Ebook_Reader.Services;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;

namespace Ebook_Reader.ViewModels
{
    public class LibraryViewModel : ViewModelBase
    {
        private readonly LibraryService _libraryService;
        private readonly MainViewModel _mainViewModel;
        private Book? _selectedBook;
        public Book? SelectedBook
        {
            get => _selectedBook;
            set => SetProperty(ref _selectedBook, value);
        }

        public ObservableCollection<Book> Books => _libraryService.GetBooks();
        public IEnumerable<Book> RecentBooks => Books.Reverse().Take(5);

        public ICommand AddBookCommand { get; }
        public ICommand OpenBookCommand { get; }

        public LibraryViewModel(LibraryService libraryService, MainViewModel mainViewModel)
        {
            _libraryService = libraryService;
            _mainViewModel = mainViewModel;

            AddBookCommand = new RelayCommand(AddBook);
            OpenBookCommand = new RelayCommand(OpenBook);

            Books.CollectionChanged += (s, e) => OnPropertyChanged(nameof(RecentBooks));
            
            // Initial selection
            SelectedBook = RecentBooks.FirstOrDefault();
        }

        private async void AddBook(object? parameter)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Ebook Files|*.epub;*.pdf;*.mobi;*.txt;*.cbz;*.fb2|EPUB Files (*.epub)|*.epub|PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                await _libraryService.AddBookAsync(dialog.FileName);
            }
        }

        private void OpenBook(object? parameter)
        {
            if (parameter is Book book)
            {
                SelectedBook = book;
                _mainViewModel.OpenBook(book);
            }
        }
    }
}

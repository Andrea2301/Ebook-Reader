using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text.Json;
using Ebook_Reader.Models;

namespace Ebook_Reader.Services
{
    public class LibraryService
    {
        private ObservableCollection<Book> _books;
        private readonly Metadata.MetadataService _metadataService;
        private readonly string _libraryFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "library.json");

        public LibraryService()
        {
            _metadataService = new Metadata.MetadataService();
            _books = new ObservableCollection<Book>();
            LoadLibrary();
        }

        private void LoadLibrary()
        {
            try
            {
                if (File.Exists(_libraryFilePath))
                {
                    string json = File.ReadAllText(_libraryFilePath);
                    var books = JsonSerializer.Deserialize<List<Book>>(json);
                    if (books != null)
                    {
                        foreach (var book in books)
                        {
                            _books.Add(book);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading library: {ex.Message}");
            }
        }

        public void SaveLibrary()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_books, options);
                File.WriteAllText(_libraryFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving library: {ex.Message}");
            }
        }

        public ObservableCollection<Book> GetBooks()
        {
            return _books;
        }

        public async Task AddBookAsync(string filePath)
        {
            try
            {
                var metadata = await _metadataService.GetMetadataAsync(filePath);
                
                string coverPath = string.Empty;
                if (metadata.CoverImage != null)
                {
                    string cacheDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Covers");
                    if (!Directory.Exists(cacheDir)) Directory.CreateDirectory(cacheDir);
                    
                    coverPath = Path.Combine(cacheDir, Path.GetFileNameWithoutExtension(filePath) + ".png");
                    File.WriteAllBytes(coverPath, metadata.CoverImage);
                }

                var book = new Book
                {
                    Title = metadata.Title,
                    Author = metadata.Author,
                    FilePath = filePath,
                    CoverImagePath = coverPath
                };

                _books.Add(book);
                SaveLibrary();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding book: {ex.Message}");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Bokhandel.Models;
using Bokhandel.Services;
using Microsoft.EntityFrameworkCore;
namespace Bokhandel.Services
{
    public class BookService
    {
        private readonly BokhandelContext _db;
        public BookService(BokhandelContext db)
        {
            _db = db;
        }
        public async Task ListAllBooks()
        {
            var books = await _db.Books
                     .Include(b => b.Author)
                     .Include(b => b.Publisher)
                     .OrderBy(b => b.Title)
                     .ToListAsync();

            if (!books.Any())
            {
                Console.WriteLine("No books found.");
                return;
            }

            Console.WriteLine("\n=== Book List ===");
            foreach (var b in books)
            {
                Console.WriteLine($"{b.Isbn13} | {b.Title} | {b.Author.FirstName} {b.Author.LastName} | Price: {b.Price} | Pages: {b.Pages}");
            }
        }
        public async Task AddNewBook(AuthorService authorService,string preFilledIsbn=null)
        {
            Console.WriteLine("\n=== Add New Book ===");
            string? isbn;
            if(!string.IsNullOrWhiteSpace(preFilledIsbn))
            {
                isbn = preFilledIsbn;
                Console.WriteLine($"Using pre-filled ISBN: {isbn}");
            }
            else
            {
                while (true)
                {
                    Console.Write("Enter ISBN(13 chars): ");
                    isbn= Console.ReadLine()?.Trim();
                    if (!string.IsNullOrWhiteSpace(isbn) && isbn.Length == 13)
                    {
                        break;
                    }
                    Console.WriteLine("Invalid ISBN. Please enter a 13-character ISBN.");
                }
                   
                var existingBook = await _db.Books.FindAsync(isbn);
                if (existingBook != null)
                {
                    Console.WriteLine("A book with this ISBN already exists.");
                    return;
                }
            }
            Console.Write("Enter Title: ");
            string? title= Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                Console.WriteLine("Title cannot be empty.");
                return;
            }
            Console.Write("Enter Language: ");
            string? language= Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(language))
            {
                Console.WriteLine("Language cannot be empty.");
                return;
            }
            Console.Write("Entre price: ");
            if(!decimal.TryParse(Console.ReadLine(), out decimal price) || price < 0)
            {
                Console.WriteLine("Invalid price.");
                return;
            }
            Console.Write("Enter release data(yyyy-MM-DD): ");
            if(!DateTime.TryParse(Console.ReadLine(), out DateTime releaseDate))
            {
                Console.WriteLine("Invalid date format.");
                return;
            }
            Console.Write("Enter number of pages: ");
            if(!int.TryParse(Console.ReadLine(), out int pages) || pages <= 0)
            {
                Console.WriteLine("Invalid number of pages.");
                return;
            }
            Console.WriteLine("\nSelect an author:");
            await authorService.ListAllAuthors();
            Console.WriteLine("------------------------");
            Console.Write("Enter Author ID(or 0 to create a New author): ");
            string? authorInput= Console.ReadLine()?.Trim();
            int authorId;
            if(authorInput == "0")
            {
                var newAuthorId = await authorService.AddAuthor();
                if(newAuthorId == null)
                {
                    Console.WriteLine("Author creation cancelled. Cannot continue adding book.");
                    return;
                }
                authorId = newAuthorId.Value;
            }
            else
            {
                if(!int.TryParse(authorInput, out authorId) || !await _db.Authors.AnyAsync(a => a.AuthorId == authorId))
                {
                    Console.WriteLine("Invalid Author ID.");
                    return;
                }
            }
          
            var book = new Book
            {
                Isbn13 = isbn,
                Title = title,
                Language = language,
                Price = price,
                ReleaseDate = releaseDate,
                Pages = pages,
                AuthorId = authorId
            };
            _db.Books.Add(book);
            await _db.SaveChangesAsync();
            Console.WriteLine($"Book '{book.Title}' added successfully.");

        }
        public async Task EditBook(AuthorService authorService)
        {
            await ListAllBooks();
            Console.Write("\nEnter ISBN of the book to edit: ");
            string? isbn= Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(isbn))
            {
                Console.WriteLine("ISBN cannot be empty.");
                return;
            }
            var bookToEdit = await _db.Books.FindAsync(isbn);
            if(bookToEdit == null)
            {
                Console.WriteLine("Book not found.");
                return;
            }
            Console.Write($"New title(Enter to keep'{bookToEdit.Title}'): ");
            string? title= Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(title))
            {
                bookToEdit.Title = title;
            }
            Console.Write($"New language(Enter to keep'{bookToEdit.Language}'): ");
            string? language= Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(language))
            {
                bookToEdit.Language = language;
            }
            Console.Write($"New price(Enter to keep'{bookToEdit.Price}'): ");
            string? priceInput= Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(priceInput))
            {
                if(decimal.TryParse(priceInput, out decimal price) && price >= 0)
                {
                    bookToEdit.Price = price;
                }
                else
                {
                    Console.WriteLine("Invalid price. Keeping existing value.");
                }
            }
            Console.Write($"New release date(yyyy-MM-DD)(Enter to keep'{bookToEdit.ReleaseDate:yyyy-MM-dd}'): ");
            string? dateInput= Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(dateInput))
            {
                if(DateTime.TryParse(dateInput, out DateTime releaseDate))
                {
                    bookToEdit.ReleaseDate = releaseDate;
                }
                else
                {
                    Console.WriteLine("Invalid date format. Keeping existing value.");
                }
            }
            Console.Write($"New number of pages(Enter to keep'{bookToEdit.Pages}'): ");
            string? pagesInput= Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(pagesInput))
            {
                if(int.TryParse(pagesInput, out int pages) && pages > 0)
                {
                    bookToEdit.Pages = pages;
                }
                else
                {
                    Console.WriteLine("Invalid number of pages. Keeping existing value.");
                }
            }
            Console.WriteLine("\nSelect a new author(Enter to keep existing):");
            await authorService.ListAllAuthors();
            Console.Write("Enter Author ID: ");
            string? authorInput= Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(authorInput))
            {
                if(int.TryParse(authorInput, out int authorId) && await _db.Authors.AnyAsync(a => a.AuthorId == authorId))
                {
                    bookToEdit.AuthorId = authorId;
                }
                else
                {
                    Console.WriteLine("Invalid Author ID. Keeping existing value.");
                }
            }
            await _db.SaveChangesAsync();
            Console.WriteLine("Book updated successfully.");

        }
        public async Task DeleteBook()
        {
            await ListAllBooks();
            Console.Write("\nEnter ISBN of the book to delete: ");
            string? isbn= Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(isbn))
            {
                Console.WriteLine("ISBN cannot be empty.");
                return;
            }
            var bookToDelete = await _db.Books.FindAsync(isbn);
            if(bookToDelete == null)
            {
                Console.WriteLine("Book not found.");
                return;
            }
            _db.Books.Remove(bookToDelete);
            await _db.SaveChangesAsync();
            Console.WriteLine("Book deleted successfully.");

        }
    }
}

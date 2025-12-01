using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bokhandel;
using Bokhandel.Models;
using Microsoft.EntityFrameworkCore;

namespace Bokhandel.Services
{
    public class AuthorService
    {
        private readonly BokhandelContext _db;
        public AuthorService(BokhandelContext db)
        {
            _db = db;
        }
        public async Task ListAllAuthors()
        {
            var authors = await _db.Authors.ToListAsync();
            if(!authors.Any())
            {
                Console.WriteLine("No authors found.");
                return;
            }
            Console.WriteLine("\n=== Authors ===");
            foreach (var a in authors)
            {
                Console.WriteLine($"{a.AuthorId}: {a.FirstName} {a.LastName} (Born: {a.Birthday:yyyy-MM-dd})");
            }
        }
        public async Task<int?> AddAuthor()
        {
            Console.Write("Enter first name: ");
            string? firstName = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(firstName))
            {
                Console.WriteLine("First name cannot be empty.");
                return null;
            }
            Console.Write("Enter last name: ");
            string? lastName = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(lastName))
            {
                Console.WriteLine("Last name cannot be empty.");
                return null;
            }
            Console.Write("Enter birthday (yyyy-MM-dd): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime birthday))
            {
                Console.WriteLine("Invalid date format.");
                return null;
            }
            var exists = await _db.Authors
                            .AnyAsync(a =>
                            a.FirstName.ToLower() == firstName.ToLower() &&
                            a.LastName.ToLower() == lastName.ToLower() &&
                            a.Birthday == birthday);

            if (exists)
            {
                Console.WriteLine("This author already exists in the database.");
                return null;
            }
            var author = new Author
            {
                FirstName = firstName,
                LastName = lastName,
                Birthday = birthday
            };

            _db.Authors.Add(author);
            await _db.SaveChangesAsync();
            Console.WriteLine($"Author {firstName} {lastName} added successfully.");
            return author.AuthorId;
        }

        public async Task EditAuthor()
        {
            await ListAllAuthors();
            Console.Write("Enter author ID to edit: ");
            if (!int.TryParse(Console.ReadLine(), out int authorId))
            {
                Console.WriteLine("Invalid author ID.");
                return;
            }
            var author = await _db.Authors.FindAsync(authorId);
            if (author == null)
            {
                Console.WriteLine("Author not found.");
                return;
            }
            Console.WriteLine($"Editing: {author.FirstName} {author.LastName}");

            Console.Write($"Enter new first name (or press Enter to keep current): ");
            string? firstName = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(firstName))
            {
                author.FirstName = firstName;
            }
            Console.Write($"Enter new last name (or press Enter to keep current): ");
            string? lastName = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(lastName))
            {
                author.LastName = lastName;
            }
            Console.Write("New birthday (yyyy-MM-dd) or press Enter to keep current: ");
            string? birthdayInput = Console.ReadLine();
            if (!string.IsNullOrEmpty(birthdayInput) && DateTime.TryParse(birthdayInput, out DateTime birthday))
            {
                author.Birthday = birthday;
            }
            await _db.SaveChangesAsync();
            Console.WriteLine($"Author {author.FirstName} {author.LastName} updated successfully.");
        }
        public async Task DeleteAuthor()
        {
            await ListAllAuthors();
            Console.Write("Enter author ID to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int authorId))
            {
                Console.WriteLine("Invalid author ID.");
                return;
            }
            var author = await _db.Authors
                          .Include(a => a.Books)
                          .FirstOrDefaultAsync(a => a.AuthorId == authorId);

            if (author == null)
            {
                Console.WriteLine("Author not found.");
                return;
            }
            if (author.Books.Any())
            {
                Console.WriteLine("Cannot delete author with associated books.");
                return;
            }
            _db.Authors.Remove(author);
            await _db.SaveChangesAsync();
            Console.WriteLine($"Author {author.FirstName} {author.LastName} deleted successfully.");
        }
        public async Task<Author?> SelectAuthor()
        {
            var authors=await _db.Authors.ToListAsync();
            if(!authors.Any())
            {
                Console.WriteLine("No authors available.");
                return null;
            }
            Console.WriteLine("\n=== Select an Author ===");
            foreach (var a in authors)
            {
                Console.WriteLine($"{a.AuthorId} {a.FirstName} {a.LastName}");
            }
            while (true)
            {
                Console.Write("Enter author ID: ");
                if(!int.TryParse(Console.ReadLine(),out int authorId))
                {
                    Console.WriteLine("Invalid ID. Try again.");
                    continue;
                }
                var author = await _db.Authors.FindAsync(authorId);
                if(author == null)
                {
                    Console.WriteLine("Author not found. Try again.");
                    continue;
                }
                return author;
            }
        }

    }
}

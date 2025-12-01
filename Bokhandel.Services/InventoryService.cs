using Bokhandel.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bokhandel.Services
{
    public class InventoryService
    {
        private readonly BokhandelContext _db;
        public InventoryService(BokhandelContext db)
        {
            _db = db;
        }
        private async Task<string?> SelecteBookISBN(int? storeId = null)
        {
            List<Book> books;

            if (storeId.HasValue)
            {
                // 1. Get valid ISBNs for this store first
                var validIsbns = await _db.Inventories
                    .Where(i => i.StoreId == storeId.Value && i.Quantity > 0)
                    .Select(i => i.Isbn13)
                    .Distinct()
                    .ToListAsync();

                // 2. Fetch full book details including Inventory data for display
                books = await _db.Books
                    .Where(b => validIsbns.Contains(b.Isbn13))
                    .Include(b => b.Inventories)
                    .ToListAsync();
            }
            else
            {
                // Fetch all books from the library
                books = await _db.Books
                    .Include(b => b.Inventories)
                    .ToListAsync();
            }

            if (!books.Any())
            {
                Console.WriteLine(storeId.HasValue
                    ? "No books in stock for this store."
                    : "No books available in the library.");
                return null;
            }

            while (true)
            {
                Console.WriteLine(storeId.HasValue
                    ? $"\n=== Available Stock in Store {storeId} ==="
                    : "\n=== All Available Titles ===");

                foreach (var book in books)
                {
                    string stockInfo = "";
                    if (storeId.HasValue)
                    {
                        // Show specific store stock
                        int qty = book.Inventories
                            .FirstOrDefault(i => i.StoreId == storeId.Value)?.Quantity ?? 0;
                        stockInfo = $" [Stock: {qty}]";
                    }
                    else
                    {
                        // Show total stock across all stores
                        int totalQty = book.Inventories.Sum(i => i.Quantity);
                        stockInfo = $" [Total System Stock: {totalQty}]";
                    }

                    Console.WriteLine($"{book.Isbn13} - {book.Title}{stockInfo}");
                }

                Console.Write("Enter ISBN: ");
                var isbn = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(isbn))
                {
                    Console.WriteLine("ISBN cannot be empty. Please try again.");
                    continue;
                }

                if (!books.Any(b => b.Isbn13 == isbn))
                {
                    Console.WriteLine("Book not found in the list. Please try again.");
                    continue;
                }

                return isbn;
            }
        }

        //private async Task<string?> SelecteBookISBN(int? storeId = null)
        //{
        //    List<Book> books; 

        //    if (storeId.HasValue)
        //    {
        //        books = await _db.Inventories
        //                        //.Include(i=>i.Isbn13Navigation)
        //                        .Where(i=>i.StoreId==storeId.Value && i.Quantity>0)
        //                        .Select(i=>i.Isbn13Navigation)
        //                        .Distinct()
        //                        .ToListAsync();
        //    }

        //    else
        //    {
        //        books = await _db.Books
        //                        .Include(b => b.Inventories)
        //                        .ToListAsync();

        //    }

        //    if(!books.Any())
        //    {
        //        Console.WriteLine("No books available.");
        //        return null;
        //    }
        //    while (true)
        //    {
        //        Console.WriteLine($"\n=== All Available Titles (Current Stock in Store {storeId}) ===");
        //        foreach (var book in books)
        //        {
        //            int currentQty=book.Inventories.FirstOrDefault(i=>i.StoreId==storeId)?.Quantity ?? 0;

        //            Console.WriteLine($"{book.Isbn13} - {book.Title} [Current:{currentQty}]");
        //        }

        //        Console.Write("Enter ISBN: ");
        //        var isbn = Console.ReadLine()?.Trim();


        //        if (string.IsNullOrWhiteSpace(isbn))
        //        {
        //            Console.WriteLine("ISBN cannot be empty. Please try again.");
        //            continue;
        //        }
        //        if (!books.Any(b => b.Isbn13 == isbn))
        //        {
        //            Console.WriteLine("Book not found. Please try again.");
        //            continue;
        //        }
        //        return isbn;
        //    }

        //}
        public async Task<List<Store>> ListAllStores()
        {
            var stores = await _db.Stores.ToListAsync();
            foreach (var store in stores)
            {
                Console.WriteLine($"{store.StoreId} - {store.StoreName} - {store.Adress}, {store.City}, {store.PostalCode}");
            }
            return stores;
        }

        public async Task<int>GetCurrentQuantity(int storeId,string isbn)
        {
            var inventory = await _db.Inventories
                .FirstOrDefaultAsync(i => i.StoreId == storeId && i.Isbn13 == isbn);
            return inventory?.Quantity ?? 0;
        }
        public async Task ListInventory(int storeId)
        {
           var store= await _db.Stores.FindAsync(storeId);
            if(store==null)
            {
                Console.WriteLine("Store not found.");
                return;
            }

            Console.WriteLine($"=== Inventory for Store: {store.StoreName} (ID:{store.StoreId})===");

            var inventories = await _db.Inventories
                .Where(i => i.StoreId == storeId && i.Quantity > 0)
                .Include(i => i.Isbn13Navigation)
                .OrderBy(i => i.Isbn13Navigation.Title)
                .ToListAsync();
            if (!inventories.Any())
            {
                Console.WriteLine("No books in this store.");
                return;
            }
            foreach (var inventory in inventories)
            {
                Console.WriteLine($"ISBN: {inventory.Isbn13} | Title: {inventory.Isbn13Navigation.Title} |  Quantity: {inventory.Quantity}");
            }
        }

        public async Task AddBookToStore(int storeId,BookService bookService, AuthorService authorService)
        {
            Console.WriteLine("=== Add Book to Store ===");
            Console.Write("Enter ISBN to add: ");
            string isbn = Console.ReadLine()?.Trim()??"";
            var bookExists = await _db.Books.AnyAsync(b => b.Isbn13 == isbn);
            if (!bookExists)
            {
                Console.WriteLine($"Book with ISBN {isbn} does not found in store.");
                Console.Write("Do you want to create it now? (y/n): ");
                if (Console.ReadLine()?.Trim().ToLower() == "y")
                {
                    await bookService.AddNewBook(authorService, isbn);
                    if(!await _db.Books.AnyAsync(b => b.Isbn13 == isbn))
                    {
                        Console.WriteLine("Book creation cancelled. Cannot add to store.");
                        return;
                    }

                }
                else
                {
                    return;
                }
            }
            if (isbn==null)return;

            Console.Write("Enter quantity to add: ");

            if(int.TryParse(Console.ReadLine(), out int amount) && amount > 0)
            {
                
                await UpdateBookQuantity(storeId,isbn,amount);
                int newQuantity = await GetCurrentQuantity(storeId, isbn);
                Console.WriteLine($"Added {amount} copies of book {isbn}. New stock: {newQuantity}");
            }
            else
            {
                Console.WriteLine("Invalid amount");
                return;
            }

              await  _db.SaveChangesAsync();
            Console.WriteLine("Book added to store!");
        }
        public async Task RemoveBookFromStore(int storeId)
        {
            var isbn =await SelecteBookISBN(storeId);
            if (isbn == null) return;
            int currentQty = await GetCurrentQuantity(storeId, isbn);
            if(currentQty==0)
            {
                Console.WriteLine("No such book in inventory to remove.");
                return;
            }

            int amount;
            try
            {
                Console.Write("Enter quantity to remove: ");
                string? input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                    throw new Exception("Input cannot be empty.");

                amount = int.Parse(input);

                if (amount <= 0)
                    throw new Exception("Quantity must be a positive number.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Invalid input: {ex.Message}");
                return;
            }

            if (amount > currentQty)
            {
                Console.WriteLine($"Cannot remove {amount} copies. Only {currentQty} in stock.");
                return;
            }

            int newQty = await UpdateBookQuantity(storeId, isbn, -amount);

            if (newQty == 0)
                Console.WriteLine($"Book {isbn} has been completely removed. Stock = 0.");
            else
                Console.WriteLine($"Removed {amount} copies of {isbn}. Remaining stock: {newQty}.");

        }
        public async Task TransferBookBetweenStores()
        {
            Console.Write("Enter source store ID: ");
            if (!int.TryParse(Console.ReadLine(), out int fromStoreId))
            {
                Console.WriteLine("Invalid store ID.");
                return;
            }

            // Ask for destination store ID
            Console.Write("Enter destination store ID: ");
            if (!int.TryParse(Console.ReadLine(), out int toStoreId))
            {
                Console.WriteLine("Invalid store ID.");
                return;
            }

            var isbn = await SelecteBookISBN(fromStoreId);
            if (isbn == null) return;
            int sourceQty = await GetCurrentQuantity(fromStoreId, isbn);
            if(sourceQty==0)
            {
                Console.WriteLine("No such book in source store to transfer.");
                return;
            }

            Console.Write("Enter quantity to transfer: ");
            if(!int.TryParse(Console.ReadLine(), out int amount) && amount > 0)
            {
                
                if(sourceQty < amount)
                {
                    Console.WriteLine("Insufficient stock in source store.");
                    return;
                }
                int newSourceQty = await UpdateBookQuantity(fromStoreId, isbn, -amount);
                int newDestQty= await UpdateBookQuantity(toStoreId, isbn, amount);
                
                Console.WriteLine($"Transferred {amount} copies of book {isbn} from store {fromStoreId} to store {toStoreId}.");
                Console.WriteLine($"Remining stock in source store: {newSourceQty}");
                Console.WriteLine($"Total stock in destination store: {newDestQty}");
            }
            else
            {
                Console.WriteLine("Invalid amount. Please enter a positive number.");
            }
        }
        public async Task<int> UpdateBookQuantity(int storeId,string isbn,int amountChange)
        {
            var inventory = await _db.Inventories.FirstOrDefaultAsync(i => i.StoreId == storeId && i.Isbn13 == isbn);
            if(inventory==null)
            {
                if (amountChange <= 0) return 0;
                
                inventory = new Inventory
                {
                    StoreId = storeId,
                    Isbn13 = isbn,
                    Quantity = amountChange
                };
                _db.Inventories.Add(inventory);
                await _db.SaveChangesAsync();
                
                return inventory.Quantity;
            }
            inventory.Quantity += amountChange;

            if(inventory.Quantity<=0)
            {
                _db.Inventories.Remove(inventory);
                await _db.SaveChangesAsync();
                Console.WriteLine("Inventory entre removed as quantity reached zero.");
                return 0;
            }
           
            await _db.SaveChangesAsync();
            return inventory.Quantity;
        }
    }
}

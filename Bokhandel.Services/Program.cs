using Bokhandel.Models;
using Bokhandel.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace Bokhandel.Services
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<Program>()
                .Build();

            //ServiceCollection.AddSinggleton<IConfiguration>(configuration);
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDbContext<BokhandelContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("BokhandelDatabase")));

            serviceCollection.AddTransient<InventoryService>();
            serviceCollection.AddTransient<BookService>();
            serviceCollection.AddTransient<AuthorService>();

            using var serviceProvider = serviceCollection.BuildServiceProvider();

            using(var scope = serviceProvider.CreateScope())
            {
                var provider = scope.ServiceProvider;

                var inventoryService = provider.GetRequiredService<InventoryService>();
                var bookService = provider.GetRequiredService<BookService>();
                var authorService = provider.GetRequiredService<AuthorService>();
                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("===BOOKSTORE MAIN MENU===");
                    Console.WriteLine("1.Inventory Management");
                    Console.WriteLine("2.Book Management");
                    Console.WriteLine("3.Author Management");
                    Console.WriteLine("0.Exit");
                    Console.Write("Choose option: ");

                    var choice = Console.ReadLine();
                    switch (choice)
                    {
                        case "1":
                            await InventoryMenu(inventoryService, bookService, authorService);
                            break;
                        case "2":
                            await BookMenu(bookService, authorService);
                            break;
                        case "3":
                            await AuthorMenu(authorService);
                            break;
                        case "0":

                            return;
                        default:
                            Console.WriteLine("Invalid option!");
                            Console.ReadKey();
                            break;
                    }
                }
            }
        }
        public static async Task InventoryMenu(InventoryService inventoryService,BookService bookService, AuthorService authorService)
        {
            int? selectedStoreId = null;
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== INVENTOR Management===");
                Console.WriteLine("1.Select Store");
                Console.WriteLine("0.Back to Main Menu");
                Console.Write("Choose option: ");

                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        selectedStoreId = await SelectedStoreId(inventoryService);
                        if(selectedStoreId != null)
                        {
                            await InventoryStoreMenu(inventoryService,bookService,authorService, selectedStoreId.Value);
                        }
                        
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid option!");
                        Console.ReadKey();
                        break;
                     
                }
            }
        }
        public static async Task InventoryStoreMenu(InventoryService inventoryService, BookService bookService,AuthorService authorService, int storeId)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"=== INVENTORY MANAGEMENT FOR STORE {storeId} ===");
                Console.WriteLine("1.List Inventory");
                Console.WriteLine("2.Add Book to Store");
                Console.WriteLine("3.Remove Book from Store");
                Console.WriteLine("4.Move Book to Another Store");
                Console.WriteLine("0.Back to Previous Menu");
                Console.Write("Choose option: ");
                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        await inventoryService.ListInventory(storeId);
                        Console.ReadKey();
                        break;
                    case "2":
                       
                        await inventoryService.AddBookToStore(storeId,bookService,authorService);
                        Console.ReadKey();
                        break;
                    case "3":
                        
                        await inventoryService.RemoveBookFromStore(storeId);
                        Console.ReadKey();
                        break;
                    case "4":
                        await inventoryService.TransferBookBetweenStores();
                        Console.ReadKey();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid option!");
                        Console.ReadKey();
                        break;
                }
            }
        }
        public static async Task<int?> SelectedStoreId(InventoryService inventoryService)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== SELECT STORE ===");
                var stores = await inventoryService.ListAllStores();
                foreach (var store in stores)
                {
                    Console.WriteLine($"{store.StoreId}. {store.StoreName} - {store.City}");
                }
                Console.Write("Enter Store ID to select (or 0 to cancel): ");
                var input = Console.ReadLine();

                if (int.TryParse(input, out int storeId))
                {
                    if (storeId == 0)
                    {
                        return null; // Cancel selection
                    }
                    var selectedStore = stores.FirstOrDefault(s => s.StoreId == storeId);
                    if (selectedStore != null)
                    {
                        Console.WriteLine($"Selected Store: {selectedStore.StoreName}");
                        Console.ReadKey();
                        return selectedStore.StoreId;
                    }
                }
                Console.WriteLine("Invalid Store ID! Press any key to try again.");
                Console.ReadKey();
            }
        }

        public static async Task BookMenu(BookService bookService,AuthorService authorService)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== BOOK MENU===");
                Console.WriteLine("1.List All Books");
                Console.WriteLine("2.Add New Book");
                Console.WriteLine("3.Edit Book");
                Console.WriteLine("4.Delete Book");
                Console.WriteLine("0.Back to Main Menu");
                Console.Write("Choose option: ");

                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        await bookService.ListAllBooks();
                        Console.ReadKey();
                        break;
                    case "2":

                       await bookService.AddNewBook(authorService);
                        Console.ReadKey();
                        break;
                    case "3":
                       await bookService.EditBook(authorService);
                        Console.ReadKey();
                        break;
                    case "4":

                       await bookService.DeleteBook();
                        Console.ReadKey();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid option!");
                        Console.ReadKey();
                        break;
                }
            }
        }

        public static async Task AuthorMenu(AuthorService authorService)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== AUTHOR MENU===");
                Console.WriteLine("1.List All Authors");
                Console.WriteLine("2.Add Author");
                Console.WriteLine("3.Edit Author");
                Console.WriteLine("4.Delete Author");
                Console.WriteLine("0.Back to Main Menu");
                Console.Write("Choose option: ");

                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        await authorService.ListAllAuthors();
                        Console.ReadKey();
                        break;
                    case "2":

                        await authorService.AddAuthor();
                        Console.ReadKey();
                        break;
                    case "3":

                        await authorService.EditAuthor();
                        Console.ReadKey();
                        break;
                    case "4":
                       
                        await authorService.DeleteAuthor();
                        Console.ReadKey();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid option!");
                        Console.ReadKey();
                        break;
                }
            }
        }

        
    }
}

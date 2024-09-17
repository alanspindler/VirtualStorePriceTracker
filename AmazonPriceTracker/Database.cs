using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Database
{
    public class Product
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string Url { get; set; }
        public int Store_Id { get; set; }
        public double? Current_Price { get; set; }
        public bool Unavailable { get; set; }
        public DateTime? Last_Checked_Date { get; set; }
        public bool Active { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Active { get; set; }
        public bool WelcomeEmailSent { get; set; }
        public string? Phone { get; set; }
        public bool WelcomeWhatsappSent { get; set; }
    }

    public class User_Product
    {
        public int Id { get; set; }
        public double Price { get; set; }
        public bool Unique_notification { get; set; }
        public DateTime? Last_notification { get; set; }
        public bool Active { get; set; }

        [ForeignKey("User")]
        public int User_id { get; set; }

        [ForeignKey("Product")]
        public int Product_id { get; set; }
        public User User { get; set; }
        public Product Product { get; set; }
    }

    public class LogType
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }

    public class Config
    {
        [Key]
        public int Id { get; set; }

        [Column("Key")]
        public string Key { get; set; }

        [Column("Value")]
        public string Value { get; set; }
    }

    public class Log
    {
        public int Id { get; set; }

        [ForeignKey("LogTypeId")]
        public int LogTypeId { get; set; }
        public string Description { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class ExecutionLog
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class PriceHistory
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public double CurrentPrice { get; set; }
        public DateTime Date { get; set; }
    }


    public class LoggingService
    {
        private readonly AppDbContext _context;

        public LoggingService(AppDbContext context)
        {
            _context = context;
        }
    }
    public class AppDbContext : DbContext
    {
        public DbSet<Product> Product { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<User_Product> User_Product { get; set; }
        public DbSet<LogType> LogType { get; set; }

        public DbSet<Log> Log { get; set; }
        public DbSet<ExecutionLog> ExecutionLog { get; set; }

        public DbSet<PriceHistory> PriceHistory { get; set; }

        public DbSet<Config> Config { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=VirtualStoresPriceTracker;User Id=sa;Password=hadouken00;TrustServerCertificate=True;");
        }
    }

    public class ProductRepository
    {
        private readonly AppDbContext _context;

        public List<Product> Products;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Product> LerProducts()
        {
            return Products;
        }

        public void InsertProduct(Product newProduct)
        {
            _context.Product.Add(newProduct);
            _context.SaveChanges();
        }

        public void AlterProduct(int id, string newName, string NewUrl, int NewStore_Id, double? NewCurrent_Price, bool NewUnavailable, DateTime NewLast_Checked_Date)
        {
            var product = _context.Product.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                product.Name = newName;
                product.Url = NewUrl;
                product.Store_Id = NewStore_Id;
                product.Current_Price = NewCurrent_Price;
                product.Unavailable = NewUnavailable;
                product.Last_Checked_Date = NewLast_Checked_Date;
                _context.SaveChanges();

                _context.Database.ExecuteSqlRaw("EXEC CheckAndInsertPriceHistory @ProductId = {0}", id);
            }
        }

        public Dictionary<int, List<Product>> GetProductsGroupedByStoreIdWithNullNames()
        {
            var productsWithNullNames = _context.Product
                .Where(p => p.Name == null &&
                            (p.Last_Checked_Date == null ||
                             p.Last_Checked_Date < DateTime.Now.AddHours(-24)) && p.Active == true)
                .GroupBy(p => p.Store_Id)
                .ToDictionary(g => g.Key, g => g.ToList());

            return productsWithNullNames;
        }

        public Dictionary<string, List<Product>> GetProductsGroupedByStoreIdPendingPriceUpdate()
        {
            var productsPendingPriceUpdate = _context.Product
                .Where(p => p.Name != null &&
                    (p.Last_Checked_Date == null || p.Last_Checked_Date < DateTime.Now.AddMinutes(-30) || (p.Current_Price == null && p.Unavailable == false)) &&
                    p.Active == true)
                .OrderBy(p => p.Last_Checked_Date)
                .Take(180)
                .GroupBy(p => p.Name)
                .ToDictionary(g => g.Key, g => g.ToList());

            return productsPendingPriceUpdate;
        }

        public List<string> GetShortenedAmazonUrls()
        {
            var shortenedUrls = _context.Product
                .Where(p => p.Url.Contains("a.co/"))
                .Select(p => p.Url)
                .ToList();

            return shortenedUrls;
        }

        public string ReturnEmailRecipient(int userId)
        {
            using (var context = new AppDbContext())
            {
                var email = context.User
                    .Where(u => u.Id == userId)
                    .Select(u => u.Email)
                    .FirstOrDefault();

                return email ?? "Email not found";
            }
        }

        public void DeleteProduct(int id)
        {
            var Product = Products.FirstOrDefault(p => p.Id == id);
            if (Product != null)
            {
                Products.Remove(Product);
            }
        }
    }

    public class PriceHistoryRepository
    {
        private readonly Func<AppDbContext> _contextFactory;

        public PriceHistoryRepository(Func<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void AddPriceHistory(int productId, double currentPrice)
        {
            using (var context = _contextFactory())
            {
                var newPriceHistory = new PriceHistory
                {
                    ProductId = productId,
                    CurrentPrice = currentPrice,
                    Date = DateTime.Now
                };

                context.PriceHistory.Add(newPriceHistory);
                context.SaveChanges();
            }
        }

        public PriceHistory GetMostRecentPriceHistory(int productId)
        {
            using (var context = _contextFactory())
            {
                return context.PriceHistory
                    .Where(ph => ph.ProductId == productId)
                    .OrderByDescending(ph => ph.Date)
                    .FirstOrDefault();
            }
        }

        public List<ExecutionLog> GetLast10ExecutionLogs()
        {
            using (var context = _contextFactory())
            {
                return context.ExecutionLog
                .OrderByDescending(e => e.Id)
                .Take(10)
                .ToList();
            }
        }
    }
}
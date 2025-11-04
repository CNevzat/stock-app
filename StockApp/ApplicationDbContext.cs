using Microsoft.EntityFrameworkCore;
using StockApp.Entities;

namespace StockApp
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();
        public DbSet<StockMovement> StockMovements => Set<StockMovement>();
        public DbSet<TodoItem> TodoItems => Set<TodoItem>();
        public DbSet<Location> Locations => Set<Location>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Category → Product
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Location → Product (optional)
            modelBuilder.Entity<Location>()
                .HasMany(l => l.Products)
                .WithOne(p => p.Location)
                .HasForeignKey(p => p.LocationId)
                .OnDelete(DeleteBehavior.SetNull);

            // Product → ProductAttribute
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Attributes)
                .WithOne(a => a.Product)
                .HasForeignKey(a => a.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product StockCode unique constraint
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.StockCode)
                .IsUnique();

            // StockMovement → Product
            modelBuilder.Entity<StockMovement>()
                .HasOne(sm => sm.Product)
                .WithMany()
                .HasForeignKey(sm => sm.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // StockMovement → Category
            modelBuilder.Entity<StockMovement>()
                .HasOne(sm => sm.Category)
                .WithMany()
                .HasForeignKey(sm => sm.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
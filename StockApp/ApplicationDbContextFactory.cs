using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StockApp
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            
            // Use SQLite for design-time (migrations)
            optionsBuilder.UseSqlite("Data Source=stockapp.db");
            
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}


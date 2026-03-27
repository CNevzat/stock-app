using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StockApp
{
    // Migration’lar: depo kökünde `dotnet tool restore`, backend’de `dotnet ef migrations add ... --output-dir Migrations`.
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var cs = Environment.GetEnvironmentVariable("STOCKAPP_CONNECTION_STRING")
                ?? "Host=localhost;Port=5432;Database=stockapp;Username=stockapp;Password=stockapp";
            optionsBuilder.UseNpgsql(cs);
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}


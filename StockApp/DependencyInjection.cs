using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockApp.Services;

namespace StockApp;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register DbContext with SQLite
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlite(connectionString);
        });

        // Register MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Register application services
        services.AddScoped<IMarkdownService, MarkdownService>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IExcelService, ExcelService>();
        services.AddScoped<IImageService, ImageService>();

        return services;
    }
}
using StockApp;
using StockApp.ApiEndpoints;
using StockApp.Hub;
using StockApp.Middleware;
using StockApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Stock App API",
        Version = "v1",
        Description = "Stock Management API with CQRS and Pagination"
    });
    
    // Use fully qualified names to avoid schema ID conflicts for types with the same name
    options.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name);

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter your token in the text input below (without 'Bearer' prefix - it will be added automatically).",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Development'ta tüm origin'lere izin ver (mobil cihazlar için)
            // Not: AllowAnyOrigin() ve AllowCredentials() birlikte kullanılamaz
            policy.SetIsOriginAllowed(_ => true) // Tüm origin'lere izin ver
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials(); // SignalR için gerekli
        }
        else
        {
            // Production'da sadece belirli origin'lere izin ver
            policy.WithOrigins(
                    "http://localhost:5173", 
                    "http://localhost:5174", 
                    "http://localhost:3000"
                  )
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials(); // SignalR için gerekli
        }
    });
});

// Register application services (DbContext, MediatR, etc.)
builder.Services.AddApplicationServices(builder.Configuration);
// SignalR ekleme
builder.Services.AddSignalR();

// Mobil cihazlardan erişim için tüm IP adreslerinde dinle
if (builder.Environment.IsDevelopment())
{
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Stock App API V1");
        options.RoutePrefix = string.Empty; // Swagger UI root'ta açılır (http://localhost:5134/)
    });
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Use exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// SignalR Hub mapping
app.MapHub<StockHub>("/hubs/stock");

// Map all API endpoints
app.MapApiEndpoints();

// Static files serving (images ve frontend için)
// Production'da frontend dosyalarını serve et (SPA fallback)
if (!app.Environment.IsDevelopment())
{
    // Default files (index.html) ve static files için
    app.UseDefaultFiles();
    app.UseStaticFiles();
    
    // React Router için fallback - tüm non-API istekleri index.html'e yönlendir
    app.MapFallbackToFile("/index.html");
}
else
{
    // Development'ta sadece images için static files
    app.UseStaticFiles();
}

// Seed veritabanı (sadece development'ta ve veri yoksa)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var seeder = new DatabaseSeeder(
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(),
            scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<StockApp.Entities.ApplicationUser>>(),
            scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>()
        );
        await seeder.SeedAsync();

        // Elasticsearch index'lerini oluştur
        var elasticsearchService = scope.ServiceProvider.GetService<IElasticsearchService>();
        if (elasticsearchService != null)
        {
            try
            {
                await elasticsearchService.EnsureIndicesExistAsync();
                Console.WriteLine("Elasticsearch indices created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Elasticsearch index creation failed: {ex.Message}");
            }
        }
    }
}

app.Run();

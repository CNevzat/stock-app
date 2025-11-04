using StockApp;
using StockApp.ApiEndpoints;
using StockApp.Hub;
using StockApp.Middleware;

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
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // SignalR için gerekli
    });
});

// Register application services (DbContext, MediatR, etc.)
builder.Services.AddApplicationServices(builder.Configuration);
// SignalR ekleme
builder.Services.AddSignalR();

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

// Static files serving (images için)
app.UseStaticFiles();

// Use exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// SignalR Hub mapping
app.MapHub<StockHub>("/hubs/stock");

// Map all API endpoints
app.MapApiEndpoints();

app.Run();

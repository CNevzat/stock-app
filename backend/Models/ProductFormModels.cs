using Microsoft.AspNetCore.Http;

namespace StockApp.Models;

// multipart/form-data ile gelen ürün oluşturma alanları (model binding).
public sealed class CreateProductForm
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; }
    public int CategoryId { get; set; }
    public string? LocationId { get; set; }
    public string PurchasePrice { get; set; } = "";
    public string SalePrice { get; set; } = "";
    public IFormFile? Image { get; set; }
}

// multipart/form-data ile gelen ürün güncelleme alanları (kısmi güncelleme; gönderilmeyen alan null kalır).
public sealed class UpdateProductForm
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? StockQuantity { get; set; }
    public int? LowStockThreshold { get; set; }
    public int? CategoryId { get; set; }
    // Boş string = lokasyon kaldır; alan yoksa null.
    public string? LocationId { get; set; }
    public string? PurchasePrice { get; set; }
    public string? SalePrice { get; set; }
    public IFormFile? Image { get; set; }
}

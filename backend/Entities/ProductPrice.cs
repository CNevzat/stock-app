namespace StockApp.Entities;

public class ProductPrice
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}



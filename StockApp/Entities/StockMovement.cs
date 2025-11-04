namespace StockApp.Entities;

public class StockMovement
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public StockMovementType Type { get; set; }
    public int Quantity { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum StockMovementType
{
    In = 1,   // Giriş
    Out = 2   // Çıkış
}



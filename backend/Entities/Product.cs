namespace StockApp.Entities
{
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public string StockCode { get; set; } = null!; // Unique stock code like ABC433
        public string Description { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public int LowStockThreshold { get; set; } = 5; // Düşük stok eşiği, kullanıcı belirler
        public string? ImagePath { get; set; } // Ürün resmi yolu
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public decimal CurrentPurchasePrice { get; set; }
        public decimal CurrentSalePrice { get; set; }

        // Foreign keys
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        
        public int? LocationId { get; set; } // Opsiyonel lokasyon
        public Location? Location { get; set; }

        // Dinamik özellikler
        public List<ProductAttribute> Attributes { get; set; } = new();
        public List<ProductPrice> PriceHistory { get; set; } = new();
    }
}
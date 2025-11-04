namespace StockApp.Entities
{
    public class Location
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public string? Description { get; set; } // Opsiyonel açıklama (örn: "Üst raf, sol taraf")
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public List<Product> Products { get; set; } = new();
    }
}




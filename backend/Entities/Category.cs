namespace StockApp.Entities
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public List<Product> Products { get; set; } = new();
    }
}
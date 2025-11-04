namespace StockApp.Entities;

public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TodoStatus Status { get; set; } = TodoStatus.Todo;
    public TodoPriority Priority { get; set; } = TodoPriority.Medium;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public enum TodoStatus
{
    Todo = 1,          // Yapılacak
    InProgress = 2,    // Devam Ediyor
    Completed = 3      // Tamamlandı
}

public enum TodoPriority
{
    Low = 1,      // Düşük
    Medium = 2,   // Orta
    High = 3      // Yüksek
}


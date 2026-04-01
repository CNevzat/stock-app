namespace StockApp.Entities;

public class SupportRequest
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? DetectedIntent { get; set; }
    public SupportRequestStatus Status { get; set; } = SupportRequestStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public enum SupportRequestStatus
{
    Pending = 0,
    Replied = 1
}

namespace StockApp.Common.Constants;

/// <summary>
/// Cache key'leri için sabitler
/// </summary>
public static class CacheKeys
{
    public const string DashboardStats = "dashboard:stats";
    
    // Product cache keys
    public static string ProductsList(int page, int pageSize, int? categoryId, int? locationId, string? searchTerm) 
        => $"products:list:page:{page}:size:{pageSize}:cat:{categoryId ?? 0}:loc:{locationId ?? 0}:search:{searchTerm ?? ""}";
    
    // StockMovement cache keys
    public static string StockMovementsList(int page, int pageSize, string? searchTerm, DateTime? startDate = null, DateTime? endDate = null) 
        => $"stockmovements:list:page:{page}:size:{pageSize}:search:{searchTerm ?? ""}:start:{startDate?.ToString("yyyy-MM-dd") ?? ""}:end:{endDate?.ToString("yyyy-MM-dd") ?? ""}";
    
    // ProductAttribute cache keys
    public static string ProductAttributesList(int page, int pageSize, int? productId, string? searchKey) 
        => $"productattributes:list:page:{page}:size:{pageSize}:product:{productId ?? 0}:search:{searchKey ?? ""}";
}


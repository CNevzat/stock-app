using StockApp.Entities;

namespace StockApp.Common.Constants;

// Cache key'leri için sabitler
public static class CacheKeys
{
    public const string DashboardStats = "dashboard:stats";

    /// <summary>Redis’te saklanan nesil; ürün CUD sonrası artırılır.</summary>
    public const string ProductsListGenerationKey = "products:list:generation";
    
    // Product cache keys (nesil değişince tüm liste önbellekleri geçersiz sayılır)
    public static string ProductsList(int page, int pageSize, int? categoryId, int? locationId, string? searchTerm, long generation) 
        => $"products:list:gen:{generation}:page:{page}:size:{pageSize}:cat:{categoryId ?? 0}:loc:{locationId ?? 0}:search:{searchTerm ?? ""}";
    
    /// <summary>Redis’te saklanan nesil; stok hareketi ekleme/silme sonrası artırılır.</summary>
    public const string StockMovementsListGenerationKey = "stockmovements:list:generation";

    // StockMovement cache keys — productId/categoryId/type OLMADAN önceki anahtar tüm istekleri birleştiriyordu (ürün detayı boş/yanlış veri).
    public static string StockMovementsList(
        int page,
        int pageSize,
        string? searchTerm,
        DateTime? startDate,
        DateTime? endDate,
        int? productId,
        int? categoryId,
        StockMovementType? type,
        long generation)
        => $"stockmovements:list:gen:{generation}:page:{page}:size:{pageSize}:search:{searchTerm ?? ""}:start:{startDate?.ToString("yyyy-MM-dd") ?? ""}:end:{endDate?.ToString("yyyy-MM-dd") ?? ""}:product:{productId ?? 0}:cat:{categoryId ?? 0}:type:{(type.HasValue ? (int)type.Value : -1)}";
    
    // ProductAttribute cache keys
    public static string ProductAttributesList(int page, int pageSize, int? productId, string? searchKey) 
        => $"productattributes:list:page:{page}:size:{pageSize}:product:{productId ?? 0}:search:{searchKey ?? ""}";
}

using StockApp.App.Product.Query;
using StockApp.App.StockMovement.Query;
using StockApp.Entities;
using ProductAttributeDto = StockApp.App.ProductAttribute.Query.ProductAttributeDto;

namespace StockApp.Services;

public interface IElasticsearchService
{
    // Product operations
    Task IndexProductAsync(ProductDto product, CancellationToken cancellationToken = default);
    Task UpdateProductAsync(ProductDto product, CancellationToken cancellationToken = default);
    Task DeleteProductAsync(int productId, CancellationToken cancellationToken = default);
    Task<SearchResult<ProductDto>> SearchProductsAsync(string query, int page = 1, int pageSize = 10, int? categoryId = null, int? locationId = null, CancellationToken cancellationToken = default);

    // StockMovement operations
    Task IndexStockMovementAsync(StockMovementDto stockMovement, CancellationToken cancellationToken = default);
    Task UpdateStockMovementAsync(StockMovementDto stockMovement, CancellationToken cancellationToken = default);
    Task DeleteStockMovementAsync(int stockMovementId, CancellationToken cancellationToken = default);
    Task<SearchResult<StockMovementDto>> SearchStockMovementsAsync(string query, int page = 1, int pageSize = 10, int? productId = null, int? categoryId = null, StockMovementType? type = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);

    // ProductAttribute operations
    Task IndexProductAttributeAsync(ProductAttributeDto productAttribute, CancellationToken cancellationToken = default);
    Task UpdateProductAttributeAsync(ProductAttributeDto productAttribute, CancellationToken cancellationToken = default);
    Task DeleteProductAttributeAsync(int productAttributeId, CancellationToken cancellationToken = default);
    Task<SearchResult<ProductAttributeDto>> SearchProductAttributesAsync(string query, int page = 1, int pageSize = 10, int? productId = null, CancellationToken cancellationToken = default);

    // Index management
    Task<bool> EnsureIndicesExistAsync(CancellationToken cancellationToken = default);
    Task<bool> DeleteIndicesAsync(CancellationToken cancellationToken = default);
    Task<bool> DeleteProductsIndexAsync(CancellationToken cancellationToken = default);
    Task<bool> DeleteStockMovementsIndexAsync(CancellationToken cancellationToken = default);
    Task<bool> DeleteProductAttributesIndexAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<string, long>> GetIndexDocumentCountsAsync(CancellationToken cancellationToken = default);
}

public class SearchResult<T>
{
    public List<T> Items { get; set; } = new();
    public long TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}



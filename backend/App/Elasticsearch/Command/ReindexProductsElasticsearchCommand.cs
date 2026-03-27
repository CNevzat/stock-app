using MediatR;
using StockApp.App.Elasticsearch;
using StockApp.App.Product.Query;
using StockApp.Services;

namespace StockApp.App.Elasticsearch.Command;

public sealed record ReindexProductsElasticsearchCommand : IRequest<ElasticsearchOperationResult>;

internal sealed class ReindexProductsElasticsearchCommandHandler
    : IRequestHandler<ReindexProductsElasticsearchCommand, ElasticsearchOperationResult>
{
    private readonly IMediator _mediator;
    private readonly IElasticsearchService? _elasticsearchService;
    private readonly ICacheService _cacheService;

    public ReindexProductsElasticsearchCommandHandler(
        IMediator mediator,
        IElasticsearchService? elasticsearchService,
        ICacheService cacheService)
    {
        _mediator = mediator;
        _elasticsearchService = elasticsearchService;
        _cacheService = cacheService;
    }

    public async Task<ElasticsearchOperationResult> Handle(
        ReindexProductsElasticsearchCommand request,
        CancellationToken cancellationToken)
    {
        if (_elasticsearchService == null)
        {
            return ElasticsearchOperationResult.BadRequest("Elasticsearch service is not available");
        }

        try
        {
            await _elasticsearchService.DeleteProductsIndexAsync();

            var indicesCreated = await _elasticsearchService.EnsureIndicesExistAsync();
            if (!indicesCreated)
            {
                return ElasticsearchOperationResult.Problem(
                    400,
                    "Index Creation Failed",
                    "Failed to create Elasticsearch indices. Check backend logs for details. Common issues: Turkish analyzer plugins not installed or incorrect analyzer configuration.");
            }

            var products = await _mediator.Send(new GetAllProductsQuery(), cancellationToken);

            if (products == null || products.Count == 0)
            {
                return ElasticsearchOperationResult.Ok(new
                {
                    message = "No products found in database to index",
                    indexedCount = 0,
                    totalProducts = 0
                });
            }

            var indexedCount = 0;
            var failedCount = 0;
            var errors = new List<string>();

            foreach (var product in products)
            {
                try
                {
                    await _elasticsearchService.IndexProductAsync(product);
                    indexedCount++;
                }
                catch (Exception ex)
                {
                    failedCount++;
                    errors.Add($"Product {product.Id} ({product.Name}): {ex.Message}");
                }
            }

            try
            {
                await _cacheService.InvalidateProductsListCacheAsync(cancellationToken);
            }
            catch
            {
                // cache temizliği kritik değil
            }

            if (failedCount > 0)
            {
                return ElasticsearchOperationResult.Ok(new
                {
                    message = $"Reindexing completed with {failedCount} failures",
                    indexedCount,
                    totalProducts = products.Count,
                    failedCount,
                    errors = errors.Take(10).ToList()
                });
            }

            return ElasticsearchOperationResult.Ok(new
            {
                message = "Products reindexed successfully",
                indexedCount,
                totalProducts = products.Count
            });
        }
        catch (Exception ex)
        {
            return ElasticsearchOperationResult.Problem(500, "Reindexing failed", ex.Message);
        }
    }
}

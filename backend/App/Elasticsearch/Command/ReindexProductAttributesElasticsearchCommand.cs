using MediatR;
using StockApp.App.Elasticsearch;
using StockApp.App.Product.Query;
using StockApp.App.ProductAttribute.Query;
using StockApp.Common.Constants;
using StockApp.Services;

namespace StockApp.App.Elasticsearch.Command;

public sealed record ReindexProductAttributesElasticsearchCommand : IRequest<ElasticsearchOperationResult>;

internal sealed class ReindexProductAttributesElasticsearchCommandHandler
    : IRequestHandler<ReindexProductAttributesElasticsearchCommand, ElasticsearchOperationResult>
{
    private readonly IMediator _mediator;
    private readonly IElasticsearchService? _elasticsearchService;
    private readonly ICacheService _cacheService;

    public ReindexProductAttributesElasticsearchCommandHandler(
        IMediator mediator,
        IElasticsearchService? elasticsearchService,
        ICacheService cacheService)
    {
        _mediator = mediator;
        _elasticsearchService = elasticsearchService;
        _cacheService = cacheService;
    }

    public async Task<ElasticsearchOperationResult> Handle(
        ReindexProductAttributesElasticsearchCommand request,
        CancellationToken cancellationToken)
    {
        if (_elasticsearchService == null)
        {
            return ElasticsearchOperationResult.BadRequest("Elasticsearch service is not available");
        }

        try
        {
            await _elasticsearchService.DeleteProductAttributesIndexAsync();

            var indicesCreated = await _elasticsearchService.EnsureIndicesExistAsync();
            if (!indicesCreated)
            {
                return ElasticsearchOperationResult.Problem(
                    400,
                    "Index Creation Failed",
                    "Failed to create Elasticsearch indices. Check backend logs for details.");
            }

            var attributes = await _mediator.Send(new GetAllProductAttributesQuery(), cancellationToken);

            var indexedCount = 0;
            foreach (var attribute in attributes)
            {
                await _elasticsearchService.IndexProductAttributeAsync(attribute);
                indexedCount++;
            }

            try
            {
                var products = await _mediator.Send(new GetAllProductsQuery(), cancellationToken);

                for (var page = 1; page <= 10; page++)
                {
                    for (var size = 10; size <= 100; size += 10)
                    {
                        await _cacheService.RemoveAsync(CacheKeys.ProductAttributesList(page, size, null, null));
                        await _cacheService.RemoveAsync(CacheKeys.ProductAttributesList(page, size, null, ""));

                        foreach (var product in products)
                        {
                            await _cacheService.RemoveAsync(CacheKeys.ProductAttributesList(page, size, product.Id, null));
                            await _cacheService.RemoveAsync(CacheKeys.ProductAttributesList(page, size, product.Id, ""));
                        }
                    }
                }
            }
            catch
            {
                // ignore
            }

            return ElasticsearchOperationResult.Ok(new
            {
                message = "Product attributes reindexed successfully",
                indexedCount,
                totalAttributes = attributes.Count
            });
        }
        catch (Exception ex)
        {
            return ElasticsearchOperationResult.Problem(500, "Reindexing failed", ex.Message);
        }
    }
}

using MediatR;
using StockApp.App.Elasticsearch;
using StockApp.App.StockMovement.Query;
using StockApp.Services;

namespace StockApp.App.Elasticsearch.Command;

public sealed record ReindexStockMovementsElasticsearchCommand : IRequest<ElasticsearchOperationResult>;

internal sealed class ReindexStockMovementsElasticsearchCommandHandler
    : IRequestHandler<ReindexStockMovementsElasticsearchCommand, ElasticsearchOperationResult>
{
    private readonly IMediator _mediator;
    private readonly IElasticsearchService? _elasticsearchService;
    private readonly ICacheService _cacheService;

    public ReindexStockMovementsElasticsearchCommandHandler(
        IMediator mediator,
        IElasticsearchService? elasticsearchService,
        ICacheService cacheService)
    {
        _mediator = mediator;
        _elasticsearchService = elasticsearchService;
        _cacheService = cacheService;
    }

    public async Task<ElasticsearchOperationResult> Handle(
        ReindexStockMovementsElasticsearchCommand request,
        CancellationToken cancellationToken)
    {
        if (_elasticsearchService == null)
        {
            return ElasticsearchOperationResult.BadRequest("Elasticsearch service is not available");
        }

        try
        {
            await _elasticsearchService.DeleteStockMovementsIndexAsync();

            var indicesCreated = await _elasticsearchService.EnsureIndicesExistAsync();
            if (!indicesCreated)
            {
                return ElasticsearchOperationResult.Problem(
                    400,
                    "Index Creation Failed",
                    "Failed to create Elasticsearch indices. Check backend logs for details.");
            }

            var stockMovements = await _mediator.Send(new GetAllStockMovementsQuery(), cancellationToken);

            var indexedCount = 0;
            foreach (var stockMovement in stockMovements)
            {
                await _elasticsearchService.IndexStockMovementAsync(stockMovement);
                indexedCount++;
            }

            try
            {
                await _cacheService.InvalidateStockMovementsListCacheAsync(cancellationToken);
            }
            catch
            {
                // ignore
            }

            return ElasticsearchOperationResult.Ok(new
            {
                message = "Stock movements reindexed successfully",
                indexedCount,
                totalStockMovements = stockMovements.Count
            });
        }
        catch (Exception ex)
        {
            return ElasticsearchOperationResult.Problem(500, "Reindexing failed", ex.Message);
        }
    }
}

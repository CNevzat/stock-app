using MediatR;
using StockApp.App.Elasticsearch;
using StockApp.Services;

namespace StockApp.App.Elasticsearch.Query;

public sealed record GetElasticsearchProductsStatusQuery : IRequest<ElasticsearchOperationResult>;

internal sealed class GetElasticsearchProductsStatusQueryHandler
    : IRequestHandler<GetElasticsearchProductsStatusQuery, ElasticsearchOperationResult>
{
    private readonly IElasticsearchService? _elasticsearchService;

    public GetElasticsearchProductsStatusQueryHandler(IElasticsearchService? elasticsearchService)
    {
        _elasticsearchService = elasticsearchService;
    }

    public async Task<ElasticsearchOperationResult> Handle(
        GetElasticsearchProductsStatusQuery request,
        CancellationToken cancellationToken)
    {
        if (_elasticsearchService == null)
        {
            return ElasticsearchOperationResult.BadRequest("Elasticsearch service is not available");
        }

        try
        {
            var counts = await _elasticsearchService.GetIndexDocumentCountsAsync(cancellationToken);
            var testSearch = await _elasticsearchService.SearchProductsAsync(string.Empty, 1, 10, null, null, cancellationToken);

            return ElasticsearchOperationResult.Ok(new
            {
                message = "Elasticsearch index status",
                indices = counts,
                totalDocuments = counts.Values.Sum(),
                testSearch = new
                {
                    totalCount = testSearch.TotalCount,
                    itemsCount = testSearch.Items.Count,
                    page = testSearch.Page,
                    pageSize = testSearch.PageSize
                }
            });
        }
        catch (Exception ex)
        {
            return ElasticsearchOperationResult.Problem(
                500,
                "Failed to get index status",
                ex.Message + "\n" + ex.StackTrace);
        }
    }
}

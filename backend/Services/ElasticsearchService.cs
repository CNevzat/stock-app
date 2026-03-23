using Elasticsearch.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using StockApp.App.Product.Query;
using StockApp.App.StockMovement.Query;
using StockApp.Entities;
using ProductAttributeDto = StockApp.App.ProductAttribute.Query.ProductAttributeDto;
using StockApp.Options;

namespace StockApp.Services;

public class ElasticsearchService : IElasticsearchService
{
    private readonly IElasticClient _client;
    private readonly ILogger<ElasticsearchService> _logger;
    private const string ProductsIndex = "products";
    private const string StockMovementsIndex = "stockmovements";
    private const string ProductAttributesIndex = "productattributes";

    public ElasticsearchService(IOptions<ElasticsearchOptions> options, ILogger<ElasticsearchService> logger)
    {
        _logger = logger;
        var settings = new ConnectionSettings(new Uri(options.Value.ConnectionString))
            .DefaultIndex(ProductsIndex)
            .EnableApiVersioningHeader()
            .DisableDirectStreaming();

        _client = new ElasticClient(settings);
    }

    #region Product Operations

    public async Task IndexProductAsync(ProductDto product, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.IndexAsync(product, idx => idx
                .Index(ProductsIndex)
                .Id(product.Id)
                .Refresh(Refresh.True), cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("Elasticsearch product index failed for ProductId {ProductId}: {Error}", product.Id, response.DebugInformation);
                throw new InvalidOperationException($"Failed to index product {product.Id}: {response.DebugInformation}");
            }
            else
            {
                _logger.LogInformation("Product indexed successfully: {ProductId} - {ProductName}", product.Id, product.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing product {ProductId} - {ProductName}", product.Id, product.Name);
            throw; // Re-throw to ensure reindex endpoint knows about failures
        }
    }

    public async Task UpdateProductAsync(ProductDto product, CancellationToken cancellationToken = default)
    {
        await IndexProductAsync(product, cancellationToken); // Update is same as index in ES
    }

    public async Task DeleteProductAsync(int productId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.DeleteAsync<ProductDto>(productId, idx => idx
                .Index(ProductsIndex)
                .Refresh(Refresh.True), cancellationToken);

            if (!response.IsValid && response.Result != Result.NotFound)
            {
                _logger.LogWarning("Elasticsearch product delete failed: {Error}", response.DebugInformation);
            }
            else
            {
                _logger.LogInformation("Product deleted from index: {ProductId}", productId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId} from index", productId);
        }
    }

    public async Task<SearchResult<ProductDto>> SearchProductsAsync(string query, int page = 1, int pageSize = 10, int? categoryId = null, int? locationId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var from = (page - 1) * pageSize;

            var searchDescriptor = new SearchDescriptor<ProductDto>()
                .Index(ProductsIndex)
                .From(from)
                .Size(pageSize)
                .Query(q => q
                    .Bool(b =>
                    {
                        var mustQueries = new List<Func<QueryContainerDescriptor<ProductDto>, QueryContainer>>();

                        // Text search
                        if (!string.IsNullOrWhiteSpace(query))
                        {
                            // Tüm aramalar için MultiMatch kullan, kısa aramalar için özel parametreler
                            var isShortQuery = query.Length <= 3;
                            
                            mustQueries.Add(must => must
                                .MultiMatch(mm => mm
                                    .Query(query)
                                    .Fields(f => f
                                        .Field(p => p.Name, boost: 3.0)
                                        .Field(p => p.Description, boost: 2.0)
                                        .Field(p => p.StockCode, boost: 2.0)
                                        .Field(p => p.CategoryName)
                                    )
                                    // Kısa aramalar için fuzziness kapalı, uzun aramalar için açık
                                    .Fuzziness(isShortQuery ? Fuzziness.EditDistance(0) : Fuzziness.Auto)
                                    .Type(TextQueryType.BestFields)
                                    // Kısa aramalar için OR operator (daha esnek), uzun aramalar için AND
                                    .Operator(isShortQuery ? Operator.Or : Operator.And)
                                    // Kısa aramalar için minimum match düşük (en az 1 field), uzun aramalar için yüksek
                                    .MinimumShouldMatch(isShortQuery ? "1" : "75%")
                                ));
                        }
                        else
                        {
                            mustQueries.Add(must => must.MatchAll());
                        }

                        // Category filter
                        if (categoryId.HasValue)
                        {
                            mustQueries.Add(must => must.Term(t => t.Field(p => p.CategoryId).Value(categoryId.Value)));
                        }

                        // Location filter
                        if (locationId.HasValue)
                        {
                            mustQueries.Add(must => must.Term(t => t.Field(p => p.LocationId).Value(locationId.Value)));
                        }

                        return b.Must(mustQueries);
                    })
                )
                .Highlight(h => h
                    .Fields(f => f
                        .Field(p => p.Name)
                        .Field(p => p.Description)
                    )
                )
                .Sort(s => s
                    .Script(script => script
                        .Type("number")
                        .Script(ss => ss
                            .Source("if (doc['updatedAt'].size() > 0) { return doc['updatedAt'].value.toInstant().toEpochMilli(); } else { return doc['createdAt'].value.toInstant().toEpochMilli(); }")
                            .Lang("painless")
                        )
                        .Order(SortOrder.Descending)
                    )
                );

            _logger.LogInformation("Executing Elasticsearch search. Query: {Query}", string.IsNullOrWhiteSpace(query) ? "MatchAll" : query);
            
            var response = await _client.SearchAsync<ProductDto>(searchDescriptor, cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("Elasticsearch product search failed: {Error}", response.DebugInformation);
                _logger.LogError("Server error: {ServerError}", response.ServerError?.Error);
                _logger.LogError("Original exception: {OriginalException}", response.OriginalException?.Message);
                return new SearchResult<ProductDto> { Page = page, PageSize = pageSize };
            }

            _logger.LogInformation("Elasticsearch search completed. Total: {Total}, Documents: {Count}, IsValid: {IsValid}", 
                response.Total, response.Documents.Count, response.IsValid);
            
            if (response.Documents.Count == 0 && response.Total > 0)
            {
                _logger.LogWarning("Elasticsearch returned {Total} total but 0 documents. This might be a pagination issue.", response.Total);
            }

            return new SearchResult<ProductDto>
            {
                Items = response.Documents.ToList(),
                TotalCount = response.Total,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products");
            return new SearchResult<ProductDto> { Page = page, PageSize = pageSize };
        }
    }

    #endregion

    #region StockMovement Operations

    public async Task IndexStockMovementAsync(StockMovementDto stockMovement, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.IndexAsync(stockMovement, idx => idx
                .Index(StockMovementsIndex)
                .Id(stockMovement.Id)
                .Refresh(Refresh.True), cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogWarning("Elasticsearch stock movement index failed: {Error}", response.DebugInformation);
            }
            else
            {
                _logger.LogInformation("Stock movement indexed: {StockMovementId}", stockMovement.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing stock movement {StockMovementId}", stockMovement.Id);
        }
    }

    public async Task UpdateStockMovementAsync(StockMovementDto stockMovement, CancellationToken cancellationToken = default)
    {
        await IndexStockMovementAsync(stockMovement, cancellationToken);
    }

    public async Task DeleteStockMovementAsync(int stockMovementId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.DeleteAsync<StockMovementDto>(stockMovementId, idx => idx
                .Index(StockMovementsIndex)
                .Refresh(Refresh.True), cancellationToken);

            if (!response.IsValid && response.Result != Result.NotFound)
            {
                _logger.LogWarning("Elasticsearch stock movement delete failed: {Error}", response.DebugInformation);
            }
            else
            {
                _logger.LogInformation("Stock movement deleted from index: {StockMovementId}", stockMovementId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting stock movement {StockMovementId} from index", stockMovementId);
        }
    }

    public async Task<SearchResult<StockMovementDto>> SearchStockMovementsAsync(string query, int page = 1, int pageSize = 10, int? productId = null, int? categoryId = null, StockMovementType? type = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var from = (page - 1) * pageSize;

            var searchDescriptor = new SearchDescriptor<StockMovementDto>()
                .Index(StockMovementsIndex)
                .From(from)
                .Size(pageSize)
                .Query(q => q
                    .Bool(b =>
                    {
                        var mustQueries = new List<Func<QueryContainerDescriptor<StockMovementDto>, QueryContainer>>();

                        // Text search
                        if (!string.IsNullOrWhiteSpace(query))
                        {
                            mustQueries.Add(must => must
                                .MultiMatch(mm => mm
                                    .Query(query)
                                    .Fields(f => f
                                        .Field(sm => sm.ProductName, boost: 3.0)
                                        .Field(sm => sm.CategoryName, boost: 2.0)
                                        .Field(sm => sm.Description)
                                    )
                                    .Fuzziness(Fuzziness.Auto)
                                    .Type(TextQueryType.BestFields)
                                ));
                        }
                        else
                        {
                            mustQueries.Add(must => must.MatchAll());
                        }

                        // Product filter
                        if (productId.HasValue)
                        {
                            mustQueries.Add(must => must.Term(t => t.Field(sm => sm.ProductId).Value(productId.Value)));
                        }

                        // Category filter
                        if (categoryId.HasValue)
                        {
                            mustQueries.Add(must => must.Term(t => t.Field(sm => sm.CategoryId).Value(categoryId.Value)));
                        }

                        // Type filter
                        if (type.HasValue)
                        {
                            mustQueries.Add(must => must.Term(t => t.Field(sm => sm.Type).Value(type.Value.ToString())));
                        }

                        // Date range filter
                        if (startDate.HasValue || endDate.HasValue)
                        {
                            mustQueries.Add(must => must
                                .DateRange(dr => dr
                                    .Field(sm => sm.CreatedAt)
                                    .GreaterThanOrEquals(startDate.HasValue ? startDate.Value : (DateTime?)null)
                                    .LessThanOrEquals(endDate.HasValue ? endDate.Value.Date.AddDays(1).AddTicks(-1) : (DateTime?)null) // End of day
                                ));
                        }

                        return b.Must(mustQueries);
                    })
                )
                .Highlight(h => h
                    .Fields(f => f
                        .Field(sm => sm.ProductName)
                        .Field(sm => sm.Description)
                    )
                )
                .Sort(s => s.Descending(sm => sm.CreatedAt));

            var response = await _client.SearchAsync<StockMovementDto>(searchDescriptor, cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogWarning("Elasticsearch stock movement search failed: {Error}", response.DebugInformation);
                return new SearchResult<StockMovementDto> { Page = page, PageSize = pageSize };
            }

            return new SearchResult<StockMovementDto>
            {
                Items = response.Documents.ToList(),
                TotalCount = response.Total,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching stock movements");
            return new SearchResult<StockMovementDto> { Page = page, PageSize = pageSize };
        }
    }

    #endregion

    #region ProductAttribute Operations

    public async Task IndexProductAttributeAsync(ProductAttributeDto productAttribute, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.IndexAsync(productAttribute, idx => idx
                .Index(ProductAttributesIndex)
                .Id(productAttribute.Id)
                .Refresh(Refresh.True), cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogWarning("Elasticsearch product attribute index failed: {Error}", response.DebugInformation);
            }
            else
            {
                _logger.LogInformation("Product attribute indexed: {ProductAttributeId}", productAttribute.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing product attribute {ProductAttributeId}", productAttribute.Id);
        }
    }

    public async Task UpdateProductAttributeAsync(ProductAttributeDto productAttribute, CancellationToken cancellationToken = default)
    {
        await IndexProductAttributeAsync(productAttribute, cancellationToken);
    }

    public async Task DeleteProductAttributeAsync(int productAttributeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.DeleteAsync<ProductAttributeDto>(productAttributeId, idx => idx
                .Index(ProductAttributesIndex)
                .Refresh(Refresh.True), cancellationToken);

            if (!response.IsValid && response.Result != Result.NotFound)
            {
                _logger.LogWarning("Elasticsearch product attribute delete failed: {Error}", response.DebugInformation);
            }
            else
            {
                _logger.LogInformation("Product attribute deleted from index: {ProductAttributeId}", productAttributeId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product attribute {ProductAttributeId} from index", productAttributeId);
        }
    }

    public async Task<SearchResult<ProductAttributeDto>> SearchProductAttributesAsync(string query, int page = 1, int pageSize = 10, int? productId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var from = (page - 1) * pageSize;

            var searchDescriptor = new SearchDescriptor<ProductAttributeDto>()
                .Index(ProductAttributesIndex)
                .From(from)
                .Size(pageSize)
                .Query(q => q
                    .Bool(b =>
                    {
                        var mustQueries = new List<Func<QueryContainerDescriptor<ProductAttributeDto>, QueryContainer>>();

                        // Text search
                        if (!string.IsNullOrWhiteSpace(query))
                        {
                            mustQueries.Add(must => must
                                .MultiMatch(mm => mm
                                    .Query(query)
                                    .Fields(f => f
                                        .Field(pa => pa.ProductName, boost: 3.0)
                                        .Field(pa => pa.Key, boost: 2.0)
                                        .Field(pa => pa.Value, boost: 2.0)
                                    )
                                    .Fuzziness(Fuzziness.Auto)
                                    .Type(TextQueryType.BestFields)
                                ));
                        }
                        else
                        {
                            mustQueries.Add(must => must.MatchAll());
                        }

                        // Product filter
                        if (productId.HasValue)
                        {
                            mustQueries.Add(must => must.Term(t => t.Field(pa => pa.ProductId).Value(productId.Value)));
                        }

                        return b.Must(mustQueries);
                    })
                )
                .Highlight(h => h
                    .Fields(f => f
                        .Field(pa => pa.Key)
                        .Field(pa => pa.Value)
                    )
                )
                .Sort(s => s
                    .Script(script => script
                        .Type("number")
                        .Script(ss => ss
                            .Source("if (doc['updatedAt'].size() > 0) { return doc['updatedAt'].value.toInstant().toEpochMilli(); } else { return doc['createdAt'].value.toInstant().toEpochMilli(); }")
                            .Lang("painless")
                        )
                        .Order(SortOrder.Descending)
                    )
                );

            var response = await _client.SearchAsync<ProductAttributeDto>(searchDescriptor, cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogWarning("Elasticsearch product attribute search failed: {Error}", response.DebugInformation);
                return new SearchResult<ProductAttributeDto> { Page = page, PageSize = pageSize };
            }

            return new SearchResult<ProductAttributeDto>
            {
                Items = response.Documents.ToList(),
                TotalCount = response.Total,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching product attributes");
            return new SearchResult<ProductAttributeDto> { Page = page, PageSize = pageSize };
        }
    }

    #endregion

    #region Index Management

    public async Task<bool> EnsureIndicesExistAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Products index
            var productsIndexExists = await _client.Indices.ExistsAsync(ProductsIndex, ct: cancellationToken);
            if (!productsIndexExists.Exists)
            {
                var createProductsIndexResponse = await _client.Indices.CreateAsync(ProductsIndex, c => c
                    .Map<ProductDto>(m => m
                        .Properties(p => p
                            .Text(t => t.Name(n => n.Name)
                                .Analyzer("autocomplete")
                                .SearchAnalyzer("autocomplete_search"))
                            .Text(t => t.Name(n => n.Description)
                                .Analyzer("autocomplete")
                                .SearchAnalyzer("autocomplete_search"))
                            .Text(t => t.Name(n => n.StockCode)
                                .Analyzer("autocomplete")
                                .SearchAnalyzer("autocomplete_search"))
                            .Keyword(k => k.Name(n => n.CategoryName))
                            .Keyword(k => k.Name(n => n.LocationName))
                            .Number(n => n.Name(n => n.Id).Type(NumberType.Integer))
                            .Number(n => n.Name(n => n.CategoryId).Type(NumberType.Integer))
                            .Number(n => n.Name(n => n.LocationId).Type(NumberType.Integer))
                            .Number(n => n.Name(n => n.StockQuantity).Type(NumberType.Integer))
                            .Number(n => n.Name(n => n.LowStockThreshold).Type(NumberType.Integer))
                            .Number(n => n.Name(n => n.CurrentPurchasePrice).Type(NumberType.Float))
                            .Number(n => n.Name(n => n.CurrentSalePrice).Type(NumberType.Float))
                            .Date(d => d.Name(n => n.CreatedAt))
                            .Date(d => d.Name(n => n.UpdatedAt))
                        )
                    )
                    .Settings(s => s
                        .Analysis(a => a
                            .TokenFilters(tf => tf
                                .EdgeNGram("autocomplete_filter", eng => eng
                                    .MinGram(1)
                                    .MaxGram(20)
                                )
                            )
                            .Analyzers(an => an
                                .Custom("autocomplete", c => c
                                    .Tokenizer("standard")
                                    .Filters("lowercase", "asciifolding", "autocomplete_filter")
                                )
                                .Custom("autocomplete_search", c => c
                                    .Tokenizer("standard")
                                    .Filters("lowercase", "asciifolding")
                                )
                                .Custom("turkish", c => c
                                    .Tokenizer("standard")
                                    .Filters("lowercase", "asciifolding")
                                )
                            )
                        )
                    ), cancellationToken);

                if (!createProductsIndexResponse.IsValid)
                {
                    _logger.LogError("Failed to create products index: {Error}", createProductsIndexResponse.DebugInformation);
                    return false;
                }
            }

            // StockMovements index
            var stockMovementsIndexExists = await _client.Indices.ExistsAsync(StockMovementsIndex, ct: cancellationToken);
            if (!stockMovementsIndexExists.Exists)
            {
                var createStockMovementsIndexResponse = await _client.Indices.CreateAsync(StockMovementsIndex, c => c
                    .Map<StockMovementDto>(m => m
                        .Properties(p => p
                            .Text(t => t.Name(n => n.ProductName)
                                .Analyzer("autocomplete")
                                .SearchAnalyzer("autocomplete_search"))
                            .Text(t => t.Name(n => n.CategoryName)
                                .Analyzer("autocomplete")
                                .SearchAnalyzer("autocomplete_search"))
                            .Text(t => t.Name(n => n.Description)
                                .Analyzer("autocomplete")
                                .SearchAnalyzer("autocomplete_search"))
                            .Number(n => n.Name(n => n.Id).Type(NumberType.Integer))
                            .Number(n => n.Name(n => n.ProductId).Type(NumberType.Integer))
                            .Number(n => n.Name(n => n.CategoryId).Type(NumberType.Integer))
                            .Number(n => n.Name(n => n.Quantity).Type(NumberType.Integer))
                            .Number(n => n.Name(n => n.UnitPrice).Type(NumberType.Float))
                            .Number(n => n.Name(n => n.TotalValue).Type(NumberType.Float))
                            .Number(n => n.Name(n => n.CurrentStockQuantity).Type(NumberType.Integer))
                            .Number(n => n.Name(n => n.LowStockThreshold).Type(NumberType.Integer))
                            .Keyword(k => k.Name(n => n.Type))
                            .Date(d => d.Name(n => n.CreatedAt))
                        )
                    )
                    .Settings(s => s
                        .Analysis(a => a
                            .TokenFilters(tf => tf
                                .EdgeNGram("autocomplete_filter", eng => eng
                                    .MinGram(1)
                                    .MaxGram(20)
                                )
                            )
                            .Analyzers(an => an
                                .Custom("autocomplete", c => c
                                    .Tokenizer("standard")
                                    .Filters("lowercase", "asciifolding", "autocomplete_filter")
                                )
                                .Custom("autocomplete_search", c => c
                                    .Tokenizer("standard")
                                    .Filters("lowercase", "asciifolding")
                                )
                                .Custom("turkish", c => c
                                    .Tokenizer("standard")
                                    .Filters("lowercase", "asciifolding")
                                )
                            )
                        )
                    ), cancellationToken);

                if (!createStockMovementsIndexResponse.IsValid)
                {
                    _logger.LogError("Failed to create stock movements index: {Error}", createStockMovementsIndexResponse.DebugInformation);
                    return false;
                }
            }

            // ProductAttributes index
            var productAttributesIndexExists = await _client.Indices.ExistsAsync(ProductAttributesIndex, ct: cancellationToken);
            if (!productAttributesIndexExists.Exists)
            {
                var createProductAttributesIndexResponse = await _client.Indices.CreateAsync(ProductAttributesIndex, c => c
                    .Map<ProductAttributeDto>(m => m
                        .Properties(p => p
                            .Text(t => t.Name(n => n.Key)
                                .Analyzer("autocomplete")
                                .SearchAnalyzer("autocomplete_search"))
                            .Text(t => t.Name(n => n.Value)
                                .Analyzer("autocomplete")
                                .SearchAnalyzer("autocomplete_search"))
                            .Text(t => t.Name(n => n.ProductName)
                                .Analyzer("autocomplete")
                                .SearchAnalyzer("autocomplete_search"))
                            .Number(n => n.Name(n => n.Id).Type(NumberType.Integer))
                            .Number(n => n.Name(n => n.ProductId).Type(NumberType.Integer))
                            .Date(d => d.Name(n => n.CreatedAt))
                            .Date(d => d.Name(n => n.UpdatedAt))
                        )
                    )
                    .Settings(s => s
                        .Analysis(a => a
                            .TokenFilters(tf => tf
                                .EdgeNGram("autocomplete_filter", eng => eng
                                    .MinGram(1)
                                    .MaxGram(20)
                                )
                            )
                            .Analyzers(an => an
                                .Custom("autocomplete", c => c
                                    .Tokenizer("standard")
                                    .Filters("lowercase", "asciifolding", "autocomplete_filter")
                                )
                                .Custom("autocomplete_search", c => c
                                    .Tokenizer("standard")
                                    .Filters("lowercase", "asciifolding")
                                )
                                .Custom("turkish", c => c
                                    .Tokenizer("standard")
                                    .Filters("lowercase", "asciifolding")
                                )
                            )
                        )
                    ), cancellationToken);

                if (!createProductAttributesIndexResponse.IsValid)
                {
                    _logger.LogError("Failed to create product attributes index: {Error}", createProductAttributesIndexResponse.DebugInformation);
                    return false;
                }
            }

            _logger.LogInformation("All Elasticsearch indices ensured");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring indices exist");
            return false;
        }
    }

    public async Task<bool> DeleteIndicesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Products index
            var productsIndexExists = await _client.Indices.ExistsAsync(ProductsIndex, ct: cancellationToken);
            if (productsIndexExists.Exists)
            {
                var deleteResponse = await _client.Indices.DeleteAsync(ProductsIndex, ct: cancellationToken);
                if (!deleteResponse.IsValid)
                {
                    _logger.LogWarning("Failed to delete products index: {Error}", deleteResponse.DebugInformation);
                }
                else
                {
                    _logger.LogInformation("Products index deleted");
                }
            }

            // StockMovements index
            var stockMovementsIndexExists = await _client.Indices.ExistsAsync(StockMovementsIndex, ct: cancellationToken);
            if (stockMovementsIndexExists.Exists)
            {
                var deleteResponse = await _client.Indices.DeleteAsync(StockMovementsIndex, ct: cancellationToken);
                if (!deleteResponse.IsValid)
                {
                    _logger.LogWarning("Failed to delete stock movements index: {Error}", deleteResponse.DebugInformation);
                }
                else
                {
                    _logger.LogInformation("Stock movements index deleted");
                }
            }

            // ProductAttributes index
            var productAttributesIndexExists = await _client.Indices.ExistsAsync(ProductAttributesIndex, ct: cancellationToken);
            if (productAttributesIndexExists.Exists)
            {
                var deleteResponse = await _client.Indices.DeleteAsync(ProductAttributesIndex, ct: cancellationToken);
                if (!deleteResponse.IsValid)
                {
                    _logger.LogWarning("Failed to delete product attributes index: {Error}", deleteResponse.DebugInformation);
                }
                else
                {
                    _logger.LogInformation("Product attributes index deleted");
                }
            }

            _logger.LogInformation("All Elasticsearch indices deleted");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting indices");
            return false;
        }
    }

    public async Task<bool> DeleteProductsIndexAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var productsIndexExists = await _client.Indices.ExistsAsync(ProductsIndex, ct: cancellationToken);
            if (productsIndexExists.Exists)
            {
                var deleteResponse = await _client.Indices.DeleteAsync(ProductsIndex, ct: cancellationToken);
                if (!deleteResponse.IsValid)
                {
                    _logger.LogWarning("Failed to delete products index: {Error}", deleteResponse.DebugInformation);
                    return false;
                }
                _logger.LogInformation("Products index deleted");
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting products index");
            return false;
        }
    }

    public async Task<bool> DeleteStockMovementsIndexAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var stockMovementsIndexExists = await _client.Indices.ExistsAsync(StockMovementsIndex, ct: cancellationToken);
            if (stockMovementsIndexExists.Exists)
            {
                var deleteResponse = await _client.Indices.DeleteAsync(StockMovementsIndex, ct: cancellationToken);
                if (!deleteResponse.IsValid)
                {
                    _logger.LogWarning("Failed to delete stock movements index: {Error}", deleteResponse.DebugInformation);
                    return false;
                }
                _logger.LogInformation("Stock movements index deleted");
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting stock movements index");
            return false;
        }
    }

    public async Task<bool> DeleteProductAttributesIndexAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var productAttributesIndexExists = await _client.Indices.ExistsAsync(ProductAttributesIndex, ct: cancellationToken);
            if (productAttributesIndexExists.Exists)
            {
                var deleteResponse = await _client.Indices.DeleteAsync(ProductAttributesIndex, ct: cancellationToken);
                if (!deleteResponse.IsValid)
                {
                    _logger.LogWarning("Failed to delete product attributes index: {Error}", deleteResponse.DebugInformation);
                    return false;
                }
                _logger.LogInformation("Product attributes index deleted");
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product attributes index");
            return false;
        }
    }

    public async Task<Dictionary<string, long>> GetIndexDocumentCountsAsync(CancellationToken cancellationToken = default)
    {
        var counts = new Dictionary<string, long>();
        try
        {
            // Products index
            var productsCount = await _client.CountAsync<ProductDto>(c => c.Index(ProductsIndex), cancellationToken);
            counts["products"] = productsCount.Count;

            // StockMovements index
            var stockMovementsCount = await _client.CountAsync<StockMovementDto>(c => c.Index(StockMovementsIndex), cancellationToken);
            counts["stockmovements"] = stockMovementsCount.Count;

            // ProductAttributes index
            var productAttributesCount = await _client.CountAsync<ProductAttributeDto>(c => c.Index(ProductAttributesIndex), cancellationToken);
            counts["productattributes"] = productAttributesCount.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting index document counts");
        }
        return counts;
    }

    #endregion
}



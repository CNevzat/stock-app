using MediatR;
using StockApp.App.ProductAttribute;
using StockApp.App.ProductAttribute.Command;
using StockApp.App.ProductAttribute.Query;
using StockApp.Common.Models;
using StockApp.Common.Constants;
using StockApp.Services;

namespace StockApp.ApiEndpoints;

public static class ProductAttributeEndpoints
{
    public static void MapProductAttributes(this WebApplication app)
    {
        var group = app.MapGroup("/api/product-attributes").WithTags("ProductAttribute");

        #region Get Product Attributes

        group.MapGet("/", async (
            IMediator mediator,
            int pageNumber = 1,
            int pageSize = 10,
            int? productId = null,
            string? searchKey = null) =>
        {
            var query = new GetProductAttributesQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                ProductId = productId,
                SearchKey = searchKey
            };
            var attributes = await mediator.Send(query);
            return Results.Ok(attributes);
        })
        .RequireAuthorization("CanViewProductAttributes")
        .Produces<PaginatedList<StockApp.App.ProductAttribute.Query.ProductAttributeDto>>(StatusCodes.Status200OK);

        #endregion

        #region Get Product Attribute By Id

        group.MapGet("/by-id", async (
            IMediator mediator,
            int id) =>
        {
            var query = new GetProductAttributeByIdQuery { Id = id };
            var attribute = await mediator.Send(query);

            if (attribute == null)
            {
                throw new KeyNotFoundException($"ProductAttribute with ID {id} not found.");
            }

            return Results.Ok(attribute);
        })
        .RequireAuthorization("CanViewProductAttributes")
        .Produces<ProductAttributeDetailDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        #endregion

        #region Create Product Attribute

        group.MapPost("/", async (
            IMediator mediator,
            CreateProductAttributeCommand command) =>
        {
            var response = await mediator.Send(command);
            return Results.Ok(response);
        })
        .RequireAuthorization("CanManageProductAttributes")
        .Produces<CreateProductAttributeCommandResponse>(StatusCodes.Status200OK);

        #endregion

        #region Update Product Attribute

        group.MapPut("/", async (
            IMediator mediator,
            UpdateProductAttributeCommand command) =>
        {
            var response = await mediator.Send(command);
            return Results.Ok(response);
        })
        .RequireAuthorization("CanManageProductAttributes")
        .Produces<UpdateProductAttributeCommandResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        #endregion

        #region Delete Product Attribute

        group.MapDelete("/", async (
            IMediator mediator,
            int id) =>
        {
            var command = new DeleteProductAttributeCommand { Id = id };
            var response = await mediator.Send(command);
            return Results.Ok(response);
        })
        .RequireAuthorization("CanManageProductAttributes")
        .Produces<DeleteProductAttributeCommandResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        #endregion

        #region Export Excel

        group.MapGet("/export/excel", async (
            IMediator mediator,
            IExcelService excelService) =>
        {
            var query = new GetAllProductAttributesQuery();
            var attributes = await mediator.Send(query);

            var content = excelService.GenerateProductAttributesExcel(attributes);

            var fileName = $"Urun_Oznitelikleri_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return Results.File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName,
                enableRangeProcessing: false);
        })
        .RequireAuthorization("CanViewProductAttributes")
        .Produces(StatusCodes.Status200OK);

        #endregion

        #region Reindex to Elasticsearch

        group.MapPost("/reindex-elasticsearch", async (
            IMediator mediator,
            IElasticsearchService? elasticsearchService,
            ICacheService cacheService) =>
        {
            if (elasticsearchService == null)
            {
                return Results.BadRequest("Elasticsearch service is not available");
            }

            try
            {
                // Önce mevcut index'leri sil (yeni mapping için)
                await elasticsearchService.DeleteProductAttributesIndexAsync();
                
                // Index'leri yeniden oluştur
                var indicesCreated = await elasticsearchService.EnsureIndicesExistAsync();
                if (!indicesCreated)
                {
                    return Results.Problem(
                        detail: "Failed to create Elasticsearch indices. Check backend logs for details.",
                        statusCode: 400,
                        title: "Index Creation Failed");
                }

                // Tüm product attributes'leri getir
                var getAllAttributesQuery = new GetAllProductAttributesQuery();
                var attributes = await mediator.Send(getAllAttributesQuery);

                // Her attribute'u Elasticsearch'e index et
                int indexedCount = 0;
                foreach (var attribute in attributes)
                {
                    await elasticsearchService.IndexProductAttributeAsync(attribute);
                    indexedCount++;
                }

                // Cache'i temizle (reindex sonrası eski cache verilerini sil)
                try
                {
                    var products = await mediator.Send(new StockApp.App.Product.Query.GetAllProductsQuery());
                    
                    int clearedCount = 0;
                    for (int page = 1; page <= 10; page++)
                    {
                        for (int size = 10; size <= 100; size += 10)
                        {
                            // Temel kombinasyonlar
                            await cacheService.RemoveAsync(CacheKeys.ProductAttributesList(page, size, null, null));
                            await cacheService.RemoveAsync(CacheKeys.ProductAttributesList(page, size, null, ""));
                            clearedCount += 2;
                            
                            // Her ürün için
                            foreach (var product in products)
                            {
                                await cacheService.RemoveAsync(CacheKeys.ProductAttributesList(page, size, product.Id, null));
                                await cacheService.RemoveAsync(CacheKeys.ProductAttributesList(page, size, product.Id, ""));
                                clearedCount += 2;
                            }
                        }
                    }
                }
                catch
                {
                    // Cache temizleme hatası kritik değil, devam et
                }

                return Results.Ok(new { 
                    message = "Product attributes reindexed successfully", 
                    indexedCount = indexedCount,
                    totalAttributes = attributes.Count 
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: 500,
                    title: "Reindexing failed");
            }
        })
        .RequireAuthorization("CanManageProductAttributes")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        #endregion
    }
}


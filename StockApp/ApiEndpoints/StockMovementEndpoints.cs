using MediatR;
using StockApp.App.StockMovement.Command;
using StockApp.App.StockMovement.Query;
using StockApp.Common.Models;
using StockApp.Common.Constants;
using StockApp.Entities;
using StockApp.Services;

namespace StockApp.ApiEndpoints;

public static class StockMovementEndpoints
{
    public static void MapStockMovements(this WebApplication app)
    {
        var group = app.MapGroup("/api/stock-movements").WithTags("StockMovement");

        #region Get Stock Movements

        group.MapGet("/", async (
            IMediator mediator,
            int pageNumber = 1,
            int pageSize = 10,
            int? productId = null,
            int? categoryId = null,
            StockMovementType? type = null,
            string? searchTerm = null,
            DateTime? startDate = null,
            DateTime? endDate = null) =>
        {
            var query = new GetStockMovementsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                ProductId = productId,
                CategoryId = categoryId,
                Type = type,
                SearchTerm = searchTerm,
                StartDate = startDate,
                EndDate = endDate
            };
            var movements = await mediator.Send(query);
            return Results.Ok(movements);
        })
        .RequireAuthorization("CanViewStockMovements")
        .Produces<PaginatedList<StockMovementDto>>(StatusCodes.Status200OK);

        #endregion

        #region Create Stock Movement

        group.MapPost("/", async (
            IMediator mediator,
            CreateStockMovementCommand command) =>
        {
            var response = await mediator.Send(command);
            return Results.Ok(response);
        })
        .RequireAuthorization("CanManageStockMovements")
        .Produces<CreateStockMovementCommandResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        #endregion

        #region Export Excel

        group.MapGet("/export/excel", async (
            IMediator mediator,
            IExcelService excelService) =>
        {
            var movements = await mediator.Send(new GetAllStockMovementsQuery());
            var content = excelService.GenerateStockMovementsExcel(movements);
            var fileName = $"Stok_Hareketleri_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return Results.File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName,
                enableRangeProcessing: false);
        })
        .RequireAuthorization("CanViewStockMovements")
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
                await elasticsearchService.DeleteStockMovementsIndexAsync();
                
                // Index'leri yeniden oluştur
                var indicesCreated = await elasticsearchService.EnsureIndicesExistAsync();
                if (!indicesCreated)
                {
                    return Results.Problem(
                        detail: "Failed to create Elasticsearch indices. Check backend logs for details.",
                        statusCode: 400,
                        title: "Index Creation Failed");
                }

                // Tüm stock movements'leri getir
                var getAllStockMovementsQuery = new GetAllStockMovementsQuery();
                var stockMovements = await mediator.Send(getAllStockMovementsQuery);

                // Her stock movement'ı Elasticsearch'e index et
                int indexedCount = 0;
                foreach (var stockMovement in stockMovements)
                {
                    await elasticsearchService.IndexStockMovementAsync(stockMovement);
                    indexedCount++;
                }

                // Cache'i temizle (reindex sonrası eski cache verilerini sil)
                try
                {
                    for (int page = 1; page <= 10; page++)
                    {
                        for (int size = 10; size <= 100; size += 10)
                        {
                            // Temel kombinasyonlar (tarih filtreleri olmadan)
                            await cacheService.RemoveAsync(CacheKeys.StockMovementsList(page, size, null));
                            await cacheService.RemoveAsync(CacheKeys.StockMovementsList(page, size, ""));
                        }
                    }
                }
                catch
                {
                    // Cache temizleme hatası kritik değil, devam et
                }

                return Results.Ok(new { 
                    message = "Stock movements reindexed successfully", 
                    indexedCount = indexedCount,
                    totalStockMovements = stockMovements.Count 
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
        .RequireAuthorization("CanManageStockMovements")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        #endregion
    }
}


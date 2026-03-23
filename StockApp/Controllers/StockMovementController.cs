using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockApp.App.StockMovement.Command;
using StockApp.App.StockMovement.Query;
using StockApp.Common.Constants;
using StockApp.Common.Models;
using StockApp.Entities;
using StockApp.Services;

namespace StockApp.Controllers;

[ApiController]
[Route("api/stock-movements")]
[Tags("StockMovement")]
public class StockMovementController : ControllerBase
{
    private readonly IMediator _mediator;

    public StockMovementController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = "CanViewStockMovements")]
    public async Task<IActionResult> GetStockMovements(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? productId = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] StockMovementType? type = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
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
        var movements = await _mediator.Send(query);
        return Ok(movements);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageStockMovements")]
    public async Task<IActionResult> Create([FromBody] CreateStockMovementCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpGet("export/excel")]
    [Authorize(Policy = "CanViewStockMovements")]
    public async Task<IActionResult> ExportExcel([FromServices] IExcelService excelService)
    {
        var movements = await _mediator.Send(new GetAllStockMovementsQuery());
        var content = excelService.GenerateStockMovementsExcel(movements);
        var fileName = $"Stok_Hareketleri_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(
            content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    [HttpPost("reindex-elasticsearch")]
    [Authorize(Policy = "CanManageStockMovements")]
    public async Task<IActionResult> ReindexElasticsearch([FromServices] IElasticsearchService? elasticsearchService,
        [FromServices] ICacheService cacheService)
    {
        if (elasticsearchService == null)
        {
            return BadRequest("Elasticsearch service is not available");
        }

        try
        {
            await elasticsearchService.DeleteStockMovementsIndexAsync();

            var indicesCreated = await elasticsearchService.EnsureIndicesExistAsync();
            if (!indicesCreated)
            {
                return Problem(
                    detail: "Failed to create Elasticsearch indices. Check backend logs for details.",
                    statusCode: 400,
                    title: "Index Creation Failed");
            }

            var getAllStockMovementsQuery = new GetAllStockMovementsQuery();
            var stockMovements = await _mediator.Send(getAllStockMovementsQuery);

            int indexedCount = 0;
            foreach (var stockMovement in stockMovements)
            {
                await elasticsearchService.IndexStockMovementAsync(stockMovement);
                indexedCount++;
            }

            try
            {
                for (int page = 1; page <= 10; page++)
                {
                    for (int size = 10; size <= 100; size += 10)
                    {
                        await cacheService.RemoveAsync(CacheKeys.StockMovementsList(page, size, null));
                        await cacheService.RemoveAsync(CacheKeys.StockMovementsList(page, size, ""));
                    }
                }
            }
            catch
            {
                // ignore
            }

            return Ok(new
            {
                message = "Stock movements reindexed successfully",
                indexedCount = indexedCount,
                totalStockMovements = stockMovements.Count
            });
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: 500,
                title: "Reindexing failed");
        }
    }
}

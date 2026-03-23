using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockApp.App.Elasticsearch.Command;
using StockApp.App.StockMovement.Command;
using StockApp.App.StockMovement.Query;
using StockApp.Common.Models;
using StockApp.Entities;
using StockApp.Extensions;
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
    public async Task<IActionResult> ReindexElasticsearch()
    {
        var result = await _mediator.Send(new ReindexStockMovementsElasticsearchCommand());
        return result.ToActionResult();
    }
}

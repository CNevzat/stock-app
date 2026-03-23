using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockApp.App.Elasticsearch.Command;
using StockApp.App.ProductAttribute;
using StockApp.App.ProductAttribute.Command;
using StockApp.App.ProductAttribute.Query;
using StockApp.Common.Models;
using StockApp.Extensions;
using StockApp.Services;

namespace StockApp.Controllers;

[ApiController]
[Route("api/product-attributes")]
[Tags("ProductAttribute")]
public class ProductAttributeController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductAttributeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = "CanViewProductAttributes")]
    public async Task<IActionResult> GetProductAttributes(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? productId = null,
        [FromQuery] string? searchKey = null)
    {
        var query = new GetProductAttributesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            ProductId = productId,
            SearchKey = searchKey
        };
        var attributes = await _mediator.Send(query);
        return Ok(attributes);
    }

    [HttpGet("by-id")]
    [Authorize(Policy = "CanViewProductAttributes")]
    public async Task<IActionResult> GetById([FromQuery] int id)
    {
        var query = new GetProductAttributeByIdQuery { Id = id };
        var attribute = await _mediator.Send(query);

        if (attribute == null)
        {
            throw new KeyNotFoundException($"ProductAttribute with ID {id} not found.");
        }

        return Ok(attribute);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageProductAttributes")]
    public async Task<IActionResult> Create([FromBody] CreateProductAttributeCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpPut]
    [Authorize(Policy = "CanManageProductAttributes")]
    public async Task<IActionResult> Update([FromBody] UpdateProductAttributeCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpDelete]
    [Authorize(Policy = "CanManageProductAttributes")]
    public async Task<IActionResult> Delete([FromQuery] int id)
    {
        var command = new DeleteProductAttributeCommand { Id = id };
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpGet("export/excel")]
    [Authorize(Policy = "CanViewProductAttributes")]
    public async Task<IActionResult> ExportExcel([FromServices] IExcelService excelService)
    {
        var query = new GetAllProductAttributesQuery();
        var attributes = await _mediator.Send(query);

        var content = excelService.GenerateProductAttributesExcel(attributes);

        var fileName = $"Urun_Oznitelikleri_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(
            content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    [HttpPost("reindex-elasticsearch")]
    [Authorize(Policy = "CanManageProductAttributes")]
    public async Task<IActionResult> ReindexElasticsearch()
    {
        var result = await _mediator.Send(new ReindexProductAttributesElasticsearchCommand());
        return result.ToActionResult();
    }
}

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockApp.App.Product.Query;
using StockApp.App.ProductAttribute;
using StockApp.App.ProductAttribute.Command;
using StockApp.App.ProductAttribute.Query;
using StockApp.Common.Constants;
using StockApp.Common.Models;
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
    public async Task<IActionResult> ReindexElasticsearch(
        [FromServices] IElasticsearchService? elasticsearchService,
        [FromServices] ICacheService cacheService)
    {
        if (elasticsearchService == null)
        {
            return BadRequest("Elasticsearch service is not available");
        }

        try
        {
            await elasticsearchService.DeleteProductAttributesIndexAsync();

            var indicesCreated = await elasticsearchService.EnsureIndicesExistAsync();
            if (!indicesCreated)
            {
                return Problem(
                    detail: "Failed to create Elasticsearch indices. Check backend logs for details.",
                    statusCode: 400,
                    title: "Index Creation Failed");
            }

            var getAllAttributesQuery = new GetAllProductAttributesQuery();
            var attributes = await _mediator.Send(getAllAttributesQuery);

            int indexedCount = 0;
            foreach (var attribute in attributes)
            {
                await elasticsearchService.IndexProductAttributeAsync(attribute);
                indexedCount++;
            }

            try
            {
                var products = await _mediator.Send(new GetAllProductsQuery());

                for (int page = 1; page <= 10; page++)
                {
                    for (int size = 10; size <= 100; size += 10)
                    {
                        await cacheService.RemoveAsync(CacheKeys.ProductAttributesList(page, size, null, null));
                        await cacheService.RemoveAsync(CacheKeys.ProductAttributesList(page, size, null, ""));

                        foreach (var product in products)
                        {
                            await cacheService.RemoveAsync(CacheKeys.ProductAttributesList(page, size, product.Id, null));
                            await cacheService.RemoveAsync(CacheKeys.ProductAttributesList(page, size, product.Id, ""));
                        }
                    }
                }
            }
            catch
            {
                // ignore
            }

            return Ok(new
            {
                message = "Product attributes reindexed successfully",
                indexedCount = indexedCount,
                totalAttributes = attributes.Count
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

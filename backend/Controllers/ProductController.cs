using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockApp.App.Elasticsearch.Command;
using StockApp.App.Elasticsearch.Query;
using StockApp.App.Product;
using StockApp.App.Product.Command;
using StockApp.App.Product.Query;
using StockApp.Common.Models;
using StockApp.Extensions;
using StockApp.Mapping;
using StockApp.Models;
using StockApp.Services;

namespace StockApp.Controllers;

[ApiController]
[Route("api/products")]
[Tags("Product")]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IImageService _imageService;

    public ProductController(IMediator mediator, IImageService imageService)
    {
        _mediator = mediator;
        _imageService = imageService;
    }

    [HttpGet]
    [Authorize(Policy = "CanViewProducts")]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? categoryId = null,
        [FromQuery] int? locationId = null,
        [FromQuery] string? searchTerm = null)
    {
        var query = new GetProductsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            CategoryId = categoryId,
            LocationId = locationId,
            SearchTerm = searchTerm
        };
        var products = await _mediator.Send(query);
        return Ok(products);
    }

    [HttpGet("by-id")]
    [Authorize(Policy = "CanViewProducts")]
    public async Task<IActionResult> GetById([FromQuery] int id)
    {
        var query = new GetProductByIdQuery { Id = id };
        var product = await _mediator.Send(query);

        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {id} not found.");
        }

        return Ok(product);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageProducts")]
    public async Task<IActionResult> Create([FromForm] CreateProductForm form)
    {
        if (!Request.HasFormContentType)
        {
            return BadRequest("Content-Type must be multipart/form-data");
        }

        var (command, error) = ProductFormMapper.MapCreate(form);
        if (error != null)
        {
            return BadRequest(error);
        }

        var response = await _mediator.Send(command!);

        if (form.Image is { Length: > 0 })
        {
            try
            {
                var imagePath = await _imageService.SaveImageAsync(form.Image, response.ProductId);

                await _mediator.Send(new UpdateProductCommand
                {
                    Id = response.ProductId,
                    ImagePath = imagePath
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Resim yükleme hatası: {ex.Message}");
            }
        }

        return Ok(response);
    }

    [HttpPut]
    [Authorize(Policy = "CanManageProducts")]
    public async Task<IActionResult> Update([FromForm] UpdateProductForm form)
    {
        if (!Request.HasFormContentType)
        {
            return BadRequest("Content-Type must be multipart/form-data");
        }

        var (command, error) = ProductFormMapper.MapUpdate(form);
        if (error != null)
        {
            return BadRequest(error);
        }

        var productId = command!.Id;

        var oldProduct = await _mediator.Send(new GetProductByIdQuery { Id = productId });
        var oldImagePath = oldProduct?.ImagePath;

        if (form.Image is { Length: > 0 })
        {
            try
            {
                var imagePath = await _imageService.SaveImageAsync(form.Image, productId);
                command = command with { ImagePath = imagePath };

                if (!string.IsNullOrEmpty(oldImagePath))
                {
                    _imageService.DeleteImage(oldImagePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Resim yükleme hatası: {ex.Message}");
            }
        }

        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpDelete]
    [Authorize(Policy = "CanManageProducts")]
    public async Task<IActionResult> Delete([FromQuery] int id)
    {
        var command = new DeleteProductCommand { Id = id };
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpGet("export/excel")]
    [Authorize(Policy = "CanViewProducts")]
    public async Task<IActionResult> ExportExcel([FromServices] IExcelService excelService)
    {
        var query = new GetAllProductsQuery();
        var products = await _mediator.Send(query);

        var content = excelService.GenerateProductsExcel(products);

        var fileName = $"Urunler_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(
            content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    [HttpPost("reindex-elasticsearch")]
    [Authorize(Policy = "CanManageProducts")]
    public async Task<IActionResult> ReindexElasticsearch()
    {
        var result = await _mediator.Send(new ReindexProductsElasticsearchCommand());
        return result.ToActionResult();
    }

    [HttpGet("elasticsearch-status")]
    [Authorize(Policy = "CanViewProducts")]
    public async Task<IActionResult> ElasticsearchStatus()
    {
        var result = await _mediator.Send(new GetElasticsearchProductsStatusQuery());
        return result.ToActionResult();
    }

}

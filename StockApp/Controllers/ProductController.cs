using System.Globalization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockApp.App.Category.Query;
using StockApp.App.Location.Query;
using StockApp.App.Product;
using StockApp.App.Product.Command;
using StockApp.App.Product.Query;
using StockApp.Common.Constants;
using StockApp.Common.Models;
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
    public async Task<IActionResult> Create()
    {
        var request = HttpContext.Request;
        if (!request.HasFormContentType)
        {
            return BadRequest("Content-Type must be multipart/form-data");
        }

        var form = await request.ReadFormAsync();

        if (!TryParseDecimal(form["purchasePrice"].ToString(), out var purchasePrice))
        {
            return BadRequest("Satın alma fiyatı geçerli bir sayı olmalıdır.");
        }

        if (!TryParseDecimal(form["salePrice"].ToString(), out var salePrice))
        {
            return BadRequest("Satış fiyatı geçerli bir sayı olmalıdır.");
        }

        var command = new CreateProductCommand
        {
            Name = form["name"].ToString(),
            Description = form["description"].ToString(),
            StockQuantity = int.Parse(form["stockQuantity"].ToString()),
            LowStockThreshold = int.Parse(form["lowStockThreshold"].ToString()),
            CategoryId = int.Parse(form["categoryId"].ToString()),
            LocationId = form.ContainsKey("locationId") && !string.IsNullOrEmpty(form["locationId"].ToString())
                ? int.Parse(form["locationId"].ToString())
                : null,
            PurchasePrice = purchasePrice,
            SalePrice = salePrice
        };

        var response = await _mediator.Send(command);

        if (form.Files.Count > 0)
        {
            var imageFile = form.Files["image"];
            if (imageFile != null && imageFile.Length > 0)
            {
                try
                {
                    var imagePath = await _imageService.SaveImageAsync(imageFile, response.ProductId);

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
        }

        return Ok(response);
    }

    [HttpPut]
    [Authorize(Policy = "CanManageProducts")]
    public async Task<IActionResult> Update()
    {
        var request = HttpContext.Request;
        if (!request.HasFormContentType)
        {
            return BadRequest("Content-Type must be multipart/form-data");
        }

        var form = await request.ReadFormAsync();

        if (!int.TryParse(form["id"].ToString(), out var productId))
        {
            return BadRequest("Product ID is required");
        }

        var command = new UpdateProductCommand
        {
            Id = productId
        };

        if (form.ContainsKey("name") && !string.IsNullOrEmpty(form["name"].ToString()))
        {
            command = command with { Name = form["name"].ToString() };
        }

        if (form.ContainsKey("description"))
        {
            command = command with { Description = form["description"].ToString() };
        }

        if (form.ContainsKey("stockQuantity") && int.TryParse(form["stockQuantity"].ToString(), out var stockQuantity))
        {
            command = command with { StockQuantity = stockQuantity };
        }

        if (form.ContainsKey("lowStockThreshold") && int.TryParse(form["lowStockThreshold"].ToString(), out var lowStockThreshold))
        {
            command = command with { LowStockThreshold = lowStockThreshold };
        }

        if (form.ContainsKey("purchasePrice") && TryParseDecimal(form["purchasePrice"].ToString(), out var purchasePrice))
        {
            command = command with { PurchasePrice = purchasePrice };
        }

        if (form.ContainsKey("salePrice") && TryParseDecimal(form["salePrice"].ToString(), out var salePrice))
        {
            command = command with { SalePrice = salePrice };
        }

        if (form.ContainsKey("categoryId") && int.TryParse(form["categoryId"].ToString(), out var categoryId))
        {
            command = command with { CategoryId = categoryId };
        }

        if (form.ContainsKey("locationId"))
        {
            var locationIdStr = form["locationId"].ToString();
            if (string.IsNullOrEmpty(locationIdStr))
            {
                command = command with { LocationId = -1 };
            }
            else if (int.TryParse(locationIdStr, out var locationId))
            {
                command = command with { LocationId = locationId };
            }
        }

        var oldProduct = await _mediator.Send(new GetProductByIdQuery { Id = productId });
        var oldImagePath = oldProduct?.ImagePath;

        if (form.Files.Count > 0)
        {
            var imageFile = form.Files["image"];
            if (imageFile != null && imageFile.Length > 0)
            {
                try
                {
                    var imagePath = await _imageService.SaveImageAsync(imageFile, productId);
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
            await elasticsearchService.DeleteProductsIndexAsync();

            var indicesCreated = await elasticsearchService.EnsureIndicesExistAsync();
            if (!indicesCreated)
            {
                return Problem(
                    detail: "Failed to create Elasticsearch indices. Check backend logs for details. Common issues: Turkish analyzer plugins not installed or incorrect analyzer configuration.",
                    statusCode: 400,
                    title: "Index Creation Failed");
            }

            var getAllProductsQuery = new GetAllProductsQuery();
            var products = await _mediator.Send(getAllProductsQuery);

            if (products == null || products.Count == 0)
            {
                return Ok(new
                {
                    message = "No products found in database to index",
                    indexedCount = 0,
                    totalProducts = 0
                });
            }

            int indexedCount = 0;
            int failedCount = 0;
            var errors = new List<string>();

            foreach (var product in products)
            {
                try
                {
                    await elasticsearchService.IndexProductAsync(product);
                    indexedCount++;
                }
                catch (Exception ex)
                {
                    failedCount++;
                    errors.Add($"Product {product.Id} ({product.Name}): {ex.Message}");
                }
            }

            try
            {
                var categories = await _mediator.Send(new GetCategoriesQuery { PageNumber = 1, PageSize = 100 });
                var locations = await _mediator.Send(new GetLocationsQuery { PageNumber = 1, PageSize = 100 });

                for (int page = 1; page <= 10; page++)
                {
                    for (int size = 10; size <= 100; size += 10)
                    {
                        await cacheService.RemoveAsync(CacheKeys.ProductsList(page, size, null, null, null));
                        await cacheService.RemoveAsync(CacheKeys.ProductsList(page, size, null, null, ""));

                        foreach (var cat in categories.Items)
                        {
                            await cacheService.RemoveAsync(CacheKeys.ProductsList(page, size, cat.Id, null, null));
                            await cacheService.RemoveAsync(CacheKeys.ProductsList(page, size, cat.Id, null, ""));
                        }

                        foreach (var loc in locations.Items)
                        {
                            await cacheService.RemoveAsync(CacheKeys.ProductsList(page, size, null, loc.Id, null));
                            await cacheService.RemoveAsync(CacheKeys.ProductsList(page, size, null, loc.Id, ""));
                        }
                    }
                }
            }
            catch
            {
                // ignore
            }

            if (failedCount > 0)
            {
                return Ok(new
                {
                    message = $"Reindexing completed with {failedCount} failures",
                    indexedCount = indexedCount,
                    totalProducts = products.Count,
                    failedCount = failedCount,
                    errors = errors.Take(10).ToList()
                });
            }

            return Ok(new
            {
                message = "Products reindexed successfully",
                indexedCount = indexedCount,
                totalProducts = products.Count
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

    [HttpGet("elasticsearch-status")]
    [Authorize(Policy = "CanViewProducts")]
    public async Task<IActionResult> ElasticsearchStatus([FromServices] IElasticsearchService? elasticsearchService)
    {
        if (elasticsearchService == null)
        {
            return BadRequest("Elasticsearch service is not available");
        }

        try
        {
            var counts = await elasticsearchService.GetIndexDocumentCountsAsync();

            var testSearch = await elasticsearchService.SearchProductsAsync(string.Empty, 1, 10, null, null);

            return Ok(new
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
            return Problem(
                detail: ex.Message + "\n" + ex.StackTrace,
                statusCode: 500,
                title: "Failed to get index status");
        }
    }

    private static bool TryParseDecimal(string? input, out decimal value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        if (decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        return decimal.TryParse(input, NumberStyles.Any, new CultureInfo("tr-TR"), out value);
    }
}

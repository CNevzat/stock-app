using System.Globalization;
using MediatR;
using StockApp.App.Product;
using StockApp.App.Product.Command;
using StockApp.App.Product.Query;
using StockApp.Common.Models;
using StockApp.Common.Constants;
using StockApp.Services;

namespace StockApp.ApiEndpoints;

public static class ProductEndpoints
{
    public static void MapProducts(this WebApplication app)
    {
        var group = app.MapGroup("/api/products").WithTags("Product");


        #region Get Products

        group.MapGet("/", async (
            IMediator mediator,
            int pageNumber = 1,
            int pageSize = 10,
            int? categoryId = null,
            int? locationId = null,
            string? searchTerm = null) =>
        {
            var query = new GetProductsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                CategoryId = categoryId,
                LocationId = locationId,
                SearchTerm = searchTerm
            };
            var products = await mediator.Send(query);
            return Results.Ok(products);
        })
        .RequireAuthorization("CanViewProducts")
        .Produces<PaginatedList<ProductDto>>(StatusCodes.Status200OK);

        #endregion

        #region Get Product By Id
        
        group.MapGet("/by-id", async (
                IMediator mediator,
                int id) =>
            {
                var query = new GetProductByIdQuery { Id = id };
                var product = await mediator.Send(query);

                if (product == null)
                {
                    throw new KeyNotFoundException($"Product with ID {id} not found.");
                }

                return Results.Ok(product);
            })
            .RequireAuthorization("CanViewProducts")
            .Produces<ProductDetailDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        #endregion

        #region Create Product

        group.MapPost("/", async (
                IMediator mediator,
                IImageService imageService,
                HttpRequest request) =>
            {
                // Multipart form data kontrolü
                if (!request.HasFormContentType)
                {
                    return Results.BadRequest("Content-Type must be multipart/form-data");
                }

                var form = await request.ReadFormAsync();

                if (!TryParseDecimal(form["purchasePrice"].ToString(), out var purchasePrice))
                {
                    return Results.BadRequest("Satın alma fiyatı geçerli bir sayı olmalıdır.");
                }

                if (!TryParseDecimal(form["salePrice"].ToString(), out var salePrice))
                {
                    return Results.BadRequest("Satış fiyatı geçerli bir sayı olmalıdır.");
                }

                // Ürün bilgilerini form'dan al
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

                // Ürünü oluştur
                var response = await mediator.Send(command);

                // Resim varsa kaydet ve ürünü güncelle
                if (form.Files.Count > 0)
                {
                    var imageFile = form.Files["image"];
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        try
                        {
                            var imagePath = await imageService.SaveImageAsync(imageFile, response.ProductId);
                            
                            // Ürünü güncelle (ImagePath ekle)
                            await mediator.Send(new UpdateProductCommand
                            {
                                Id = response.ProductId,
                                ImagePath = imagePath
                            });
                        }
                        catch (Exception ex)
                        {
                            // Resim yükleme hatası, ürün zaten oluşturuldu
                            Console.WriteLine($"Resim yükleme hatası: {ex.Message}");
                        }
                    }
                }

                return Results.Ok(response);
            })
            .RequireAuthorization("CanManageProducts")
            .DisableAntiforgery() // Minimal API'de multipart için gerekli
            .Produces<CreateProductCommandResponse>(StatusCodes.Status200OK);

        #endregion

        #region Update Product

        group.MapPut("/", async (
            IMediator mediator,
            IImageService imageService,
            HttpRequest request) =>
        {
            // Multipart form data kontrolü
            if (!request.HasFormContentType)
            {
                return Results.BadRequest("Content-Type must be multipart/form-data");
            }

            var form = await request.ReadFormAsync();
            
            // Ürün ID'si zorunlu
            if (!int.TryParse(form["id"].ToString(), out var productId))
            {
                return Results.BadRequest("Product ID is required");
            }

            // Ürün bilgilerini form'dan al (sadece gönderilenler)
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
                    // Boş string gönderilirse location'ı kaldır
                    command = command with { LocationId = -1 }; // Özel değer, handler'da null'a çevrilecek
                }
                else if (int.TryParse(locationIdStr, out var locationId))
                {
                    command = command with { LocationId = locationId };
                }
            }

            // Eski resmi al (silme için)
            var oldProduct = await mediator.Send(new GetProductByIdQuery { Id = productId });
            var oldImagePath = oldProduct?.ImagePath;

            // Yeni resim varsa kaydet
            if (form.Files.Count > 0)
            {
                var imageFile = form.Files["image"];
                if (imageFile != null && imageFile.Length > 0)
                {
                    try
                    {
                        var imagePath = await imageService.SaveImageAsync(imageFile, productId);
                        command = command with { ImagePath = imagePath };
                        
                        // Eski resmi sil
                        if (!string.IsNullOrEmpty(oldImagePath))
                        {
                            imageService.DeleteImage(oldImagePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Resim yükleme hatası: {ex.Message}");
                        // Resim yükleme hatasında devam et
                    }
                }
            }

            var response = await mediator.Send(command);
            return Results.Ok(response);
        })
        .RequireAuthorization("CanManageProducts")
        .DisableAntiforgery() // Minimal API'de multipart için gerekli
        .Produces<UpdateProductCommandResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        #endregion

        #region Delete Product

        group.MapDelete("/", async (
            IMediator mediator,
            int id) =>
        {
            var command = new DeleteProductCommand { Id = id };
            var response = await mediator.Send(command);
            return Results.Ok(response);
        })
        .RequireAuthorization("CanManageProducts")
        .Produces<DeleteProductCommandResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        #endregion

        #region Export Excel

        group.MapGet("/export/excel", async (
            IMediator mediator,
            IExcelService excelService) =>
        {
            var query = new GetAllProductsQuery();
            var products = await mediator.Send(query);

            var content = excelService.GenerateProductsExcel(products);

            var fileName = $"Urunler_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return Results.File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName,
                enableRangeProcessing: false);
        })
        .RequireAuthorization("CanViewProducts")
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
                // Önce mevcut products index'ini sil (yeni mapping için)
                await elasticsearchService.DeleteProductsIndexAsync();
                
                // Index'leri yeniden oluştur
                var indicesCreated = await elasticsearchService.EnsureIndicesExistAsync();
                if (!indicesCreated)
                {
                    // Daha detaylı hata mesajı için log'ları kontrol edin
                    return Results.Problem(
                        detail: "Failed to create Elasticsearch indices. Check backend logs for details. Common issues: Turkish analyzer plugins not installed or incorrect analyzer configuration.",
                        statusCode: 400,
                        title: "Index Creation Failed");
                }

                // Tüm ürünleri getir
                var getAllProductsQuery = new GetAllProductsQuery();
                var products = await mediator.Send(getAllProductsQuery);

                if (products == null || products.Count == 0)
                {
                    return Results.Ok(new { 
                        message = "No products found in database to index", 
                        indexedCount = 0,
                        totalProducts = 0 
                    });
                }

                // Her ürünü Elasticsearch'e index et
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
                        // Continue with next product
                    }
                }

                // Cache'i temizle (reindex sonrası eski cache verilerini sil)
                // Tüm olası cache key kombinasyonlarını temizle
                try
                {
                    var categories = await mediator.Send(new StockApp.App.Category.Query.GetCategoriesQuery { PageNumber = 1, PageSize = 100 });
                    var locations = await mediator.Send(new StockApp.App.Location.Query.GetLocationsQuery { PageNumber = 1, PageSize = 100 });
                    
                    int clearedCount = 0;
                    for (int page = 1; page <= 10; page++)
                    {
                        for (int size = 10; size <= 100; size += 10)
                        {
                            // Temel kombinasyonlar
                            await cacheService.RemoveAsync(CacheKeys.ProductsList(page, size, null, null, null));
                            await cacheService.RemoveAsync(CacheKeys.ProductsList(page, size, null, null, ""));
                            clearedCount += 2;
                            
                            // Her kategori için
                            foreach (var cat in categories.Items)
                            {
                                await cacheService.RemoveAsync(CacheKeys.ProductsList(page, size, cat.Id, null, null));
                                await cacheService.RemoveAsync(CacheKeys.ProductsList(page, size, cat.Id, null, ""));
                                clearedCount += 2;
                            }
                            
                            // Her lokasyon için
                            foreach (var loc in locations.Items)
                            {
                                await cacheService.RemoveAsync(CacheKeys.ProductsList(page, size, null, loc.Id, null));
                                await cacheService.RemoveAsync(CacheKeys.ProductsList(page, size, null, loc.Id, ""));
                                clearedCount += 2;
                            }
                        }
                    }
                }
                catch
                {
                    // Cache temizleme hatası kritik değil, devam et
                }

                if (failedCount > 0)
                {
                    return Results.Ok(new { 
                        message = $"Reindexing completed with {failedCount} failures", 
                        indexedCount = indexedCount,
                        totalProducts = products.Count,
                        failedCount = failedCount,
                        errors = errors.Take(10).ToList() // Limit to first 10 errors
                    });
                }

                return Results.Ok(new { 
                    message = "Products reindexed successfully", 
                    indexedCount = indexedCount,
                    totalProducts = products.Count 
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
        .RequireAuthorization("CanManageProducts")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        #endregion

        #region Check Elasticsearch Index Status

        group.MapGet("/elasticsearch-status", async (
            IElasticsearchService? elasticsearchService) =>
        {
            if (elasticsearchService == null)
            {
                return Results.BadRequest("Elasticsearch service is not available");
            }

            try
            {
                var counts = await elasticsearchService.GetIndexDocumentCountsAsync();
                
                // Test search query
                var testSearch = await elasticsearchService.SearchProductsAsync(string.Empty, 1, 10, null, null);
                
                return Results.Ok(new
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
                return Results.Problem(
                    detail: ex.Message + "\n" + ex.StackTrace,
                    statusCode: 500,
                    title: "Failed to get index status");
            }
        })
        .RequireAuthorization("CanViewProducts")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        #endregion
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


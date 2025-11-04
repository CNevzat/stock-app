using MediatR;
using StockApp.App.Product;
using StockApp.App.Product.Command;
using StockApp.App.Product.Query;
using StockApp.Common.Models;
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
        }).Produces<PaginatedList<ProductDto>>(StatusCodes.Status200OK);

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
                        : null
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
        .Produces(StatusCodes.Status200OK);

        #endregion
    }
}


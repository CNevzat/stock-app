using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StockApp.App.Dashboard.Query;
using StockApp.App.Product.Query;
using StockApp.Hub;
using StockApp.Services;
using StockApp.Entities;
using StockApp.Common.Constants;

namespace StockApp.App.Product.Command;

public record UpdateProductCommand : IRequest<UpdateProductCommandResponse>
{
    public int Id { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public int? StockQuantity { get; init; }
    public int? LowStockThreshold { get; init; }
    public int? LocationId { get; init; }
    public int? CategoryId { get; init; }
    public decimal? PurchasePrice { get; init; }
    public decimal? SalePrice { get; init; }
    public string? ImagePath { get; init; }
}

public record UpdateProductCommandResponse 
{
    public int ProductId { get; set; }
}

internal class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, UpdateProductCommandResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<StockHub> _hubContext;
    private readonly IMediator _mediator;
    private readonly IImageService _imageService;
    private readonly ICacheService _cacheService;
    private readonly IElasticsearchService? _elasticsearchService;

    public UpdateProductCommandHandler(
        ApplicationDbContext context, 
        IHubContext<StockHub> hubContext, 
        IMediator mediator, 
        IImageService imageService, 
        ICacheService cacheService,
        IElasticsearchService? elasticsearchService = null)
    {
        _context = context;
        _hubContext = hubContext;
        _mediator = mediator;
        _imageService = imageService;
        _cacheService = cacheService;
        _elasticsearchService = elasticsearchService;
    }

    public async Task<UpdateProductCommandResponse> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {request.Id} not found.");
        }

        // Sadece gönderilen (null olmayan) alanları güncelle
        if (request.Name != null)
        {
            product.Name = request.Name;
        }

        if (request.Description != null)
        {
            product.Description = request.Description;
        }

        if (request.StockQuantity.HasValue)
        {
            product.StockQuantity = request.StockQuantity.Value;
        }

        if (request.LowStockThreshold.HasValue)
        {
            product.LowStockThreshold = request.LowStockThreshold.Value;
        }

        if (request.LocationId.HasValue)
        {
            if (request.LocationId.Value == -1)
            {
                // -1 özel değeri location'ı kaldırmak için
                product.LocationId = null;
            }
            else
            {
                product.LocationId = request.LocationId.Value;
            }
        }

        if (request.CategoryId.HasValue)
        {
            product.CategoryId = request.CategoryId.Value;
        }

        var priceChanged = false;

        if (request.PurchasePrice.HasValue)
        {
            if (request.PurchasePrice.Value <= 0)
            {
                throw new ArgumentException("Satın alma fiyatı 0'dan büyük olmalıdır.", nameof(request.PurchasePrice));
            }

            if (product.CurrentPurchasePrice != request.PurchasePrice.Value)
            {
                product.CurrentPurchasePrice = request.PurchasePrice.Value;
                priceChanged = true;
            }
        }

        if (request.SalePrice.HasValue)
        {
            if (request.SalePrice.Value <= 0)
            {
                throw new ArgumentException("Satış fiyatı 0'dan büyük olmalıdır.", nameof(request.SalePrice));
            }

            if (product.CurrentSalePrice != request.SalePrice.Value)
            {
                product.CurrentSalePrice = request.SalePrice.Value;
                priceChanged = true;
            }
        }

        if (request.ImagePath != null)
        {
            // Eski resmi sil (yeni resim eklendiyse)
            if (!string.IsNullOrEmpty(product.ImagePath) && product.ImagePath != request.ImagePath)
            {
                _imageService.DeleteImage(product.ImagePath);
            }
            product.ImagePath = request.ImagePath;
        }

        // Güncellenme tarihini ayarla
        product.UpdatedAt = DateTime.UtcNow;

        _context.Products.Update(product);

        if (priceChanged)
        {
            var priceHistory = new ProductPrice
            {
                ProductId = product.Id,
                PurchasePrice = product.CurrentPurchasePrice,
                SalePrice = product.CurrentSalePrice,
                EffectiveDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            _context.ProductPrices.Add(priceHistory);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Cache'i invalidate et (dashboard stats değişti)
        await _cacheService.RemoveAsync(CacheKeys.DashboardStats, cancellationToken);

        // SignalR ile dashboard stats gönder
        try
        {
            var dashboardStats = await _mediator.Send(new GetDashboardStatsQuery(), cancellationToken);
            await _hubContext.Clients.All.SendAsync("DashboardStatsUpdated", dashboardStats, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR gönderim hatası: {ex.Message}");
        }

        // SignalR ile güncellenmiş ürünü tüm client'lara gönder
        try
        {
            var productDetail = await _mediator.Send(new GetProductByIdQuery { Id = product.Id }, cancellationToken);
            if (productDetail != null)
            {
                // Elasticsearch'i güncelle
                if (_elasticsearchService != null)
                {
                    var productDto = new ProductDto
                    {
                        Id = productDetail.Id,
                        Name = productDetail.Name,
                        StockCode = productDetail.StockCode,
                        Description = productDetail.Description,
                        StockQuantity = productDetail.StockQuantity,
                        LowStockThreshold = productDetail.LowStockThreshold,
                        CategoryId = productDetail.CategoryId,
                        CategoryName = productDetail.CategoryName,
                        LocationId = productDetail.LocationId,
                        LocationName = productDetail.LocationName,
                        ImagePath = productDetail.ImagePath,
                        CreatedAt = productDetail.CreatedAt,
                        UpdatedAt = productDetail.UpdatedAt,
                        CurrentPurchasePrice = productDetail.CurrentPurchasePrice,
                        CurrentSalePrice = productDetail.CurrentSalePrice
                    };
                    await _elasticsearchService.UpdateProductAsync(productDto, cancellationToken);
                }

                await _hubContext.Clients.All.SendAsync("ProductUpdated", productDetail, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR product updated gönderim hatası: {ex.Message}");
        }

        return new UpdateProductCommandResponse()
        {
            ProductId = product.Id
        };
    }
}


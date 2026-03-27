using System.Globalization;
using StockApp.App.Product.Command;
using StockApp.Models;

namespace StockApp.Mapping;

// Multipart ürün formlarını MediatR komutlarına çevirir (tek yerde doğrulama ve eşleme).
public static class ProductFormMapper
{
    public static (CreateProductCommand? Command, string? Error) MapCreate(CreateProductForm form)
    {
        if (string.IsNullOrWhiteSpace(form.Name))
        {
            return (null, "Ürün adı zorunludur.");
        }

        if (!TryParseDecimal(form.PurchasePrice, out var purchasePrice))
        {
            return (null, "Satın alma fiyatı geçerli bir sayı olmalıdır.");
        }

        if (!TryParseDecimal(form.SalePrice, out var salePrice))
        {
            return (null, "Satış fiyatı geçerli bir sayı olmalıdır.");
        }

        int? locationId = null;
        if (!string.IsNullOrWhiteSpace(form.LocationId) && int.TryParse(form.LocationId, out var loc))
        {
            locationId = loc;
        }

        var command = new CreateProductCommand
        {
            Name = form.Name.Trim(),
            Description = form.Description ?? "",
            StockQuantity = form.StockQuantity,
            LowStockThreshold = form.LowStockThreshold,
            CategoryId = form.CategoryId,
            LocationId = locationId,
            PurchasePrice = purchasePrice,
            SalePrice = salePrice
        };

        return (command, null);
    }

    public static (UpdateProductCommand? Command, string? Error) MapUpdate(UpdateProductForm form)
    {
        if (form.Id <= 0)
        {
            return (null, "Product ID is required");
        }

        UpdateProductCommand command = new() { Id = form.Id };

        if (!string.IsNullOrEmpty(form.Name))
        {
            command = command with { Name = form.Name };
        }

        if (form.Description != null)
        {
            command = command with { Description = form.Description };
        }

        if (form.StockQuantity.HasValue)
        {
            command = command with { StockQuantity = form.StockQuantity };
        }

        if (form.LowStockThreshold.HasValue)
        {
            command = command with { LowStockThreshold = form.LowStockThreshold };
        }

        if (form.CategoryId.HasValue)
        {
            command = command with { CategoryId = form.CategoryId };
        }

        if (form.PurchasePrice != null && TryParseDecimal(form.PurchasePrice, out var purchasePrice))
        {
            command = command with { PurchasePrice = purchasePrice };
        }

        if (form.SalePrice != null && TryParseDecimal(form.SalePrice, out var salePrice))
        {
            command = command with { SalePrice = salePrice };
        }

        if (form.LocationId != null)
        {
            if (string.IsNullOrEmpty(form.LocationId))
            {
                command = command with { LocationId = -1 };
            }
            else if (int.TryParse(form.LocationId, out var locationId))
            {
                command = command with { LocationId = locationId };
            }
        }

        return (command, null);
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

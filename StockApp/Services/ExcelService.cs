using ClosedXML.Excel;
using StockApp.App.Product.Query;
using ProductAttributeDto = StockApp.App.ProductAttribute.Query.ProductAttributeDto;

namespace StockApp.Services;

public interface IExcelService
{
    byte[] GenerateProductsExcel(List<ProductDto> products);
    byte[] GenerateProductAttributesExcel(List<ProductAttributeDto> attributes);
}

public class ExcelService : IExcelService
{
    public byte[] GenerateProductsExcel(List<ProductDto> products)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Ürünler");

        // Başlık satırı
        worksheet.Cell(1, 1).Value = "Ürün Adı";
        worksheet.Cell(1, 2).Value = "Stok Kodu";
        worksheet.Cell(1, 3).Value = "Açıklama";
        worksheet.Cell(1, 4).Value = "Stok Miktarı";
        worksheet.Cell(1, 5).Value = "Düşük Stok Eşiği";
        worksheet.Cell(1, 6).Value = "Kategori";
        worksheet.Cell(1, 7).Value = "Oluşturulma Tarihi";
        worksheet.Cell(1, 8).Value = "Güncellenme Tarihi";

        // Başlık satırını formatla
        var headerRange = worksheet.Range(1, 1, 1, 8);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Veri satırlarını ekle
        for (int i = 0; i < products.Count; i++)
        {
            var product = products[i];
            var row = i + 2;
            worksheet.Cell(row, 1).Value = product.Name;
            worksheet.Cell(row, 2).Value = product.StockCode;
            worksheet.Cell(row, 3).Value = product.Description;
            worksheet.Cell(row, 4).Value = product.StockQuantity;
            worksheet.Cell(row, 5).Value = product.LowStockThreshold;
            worksheet.Cell(row, 6).Value = product.CategoryName;
            worksheet.Cell(row, 7).Value = product.CreatedAt.ToString("dd.MM.yyyy HH:mm");
            worksheet.Cell(row, 8).Value = product.UpdatedAt?.ToString("dd.MM.yyyy HH:mm") ?? "-";
        }

        // Sütun genişliklerini ayarla
        worksheet.Column(1).Width = 25;
        worksheet.Column(2).Width = 15;
        worksheet.Column(3).Width = 40;
        worksheet.Column(4).Width = 15;
        worksheet.Column(5).Width = 20;
        worksheet.Column(6).Width = 20;
        worksheet.Column(7).Width = 20;
        worksheet.Column(8).Width = 20;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GenerateProductAttributesExcel(List<ProductAttributeDto> attributes)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Ürün Öznitelikleri");

        // Başlık satırı
        worksheet.Cell(1, 1).Value = "Ürün Adı";
        worksheet.Cell(1, 2).Value = "Öznitelik Anahtarı";
        worksheet.Cell(1, 3).Value = "Öznitelik Değeri";

        // Başlık satırını formatla
        var headerRange = worksheet.Range(1, 1, 1, 3);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Veri satırlarını ekle
        for (int i = 0; i < attributes.Count; i++)
        {
            var attribute = attributes[i];
            var row = i + 2;
            worksheet.Cell(row, 1).Value = attribute.ProductName;
            worksheet.Cell(row, 2).Value = attribute.Key;
            worksheet.Cell(row, 3).Value = attribute.Value;
        }

        // Sütun genişliklerini ayarla
        worksheet.Column(1).Width = 30;
        worksheet.Column(2).Width = 25;
        worksheet.Column(3).Width = 30;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}

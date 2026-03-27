using Markdig;
using PuppeteerSharp;
using StockApp.App.Product.Query;

namespace StockApp.Services;

public interface IPdfService
{
    Task<byte[]> GenerateCriticalStockPdf(List<ProductDto> products);
}

public class PdfService : IPdfService
{
    public async Task<byte[]> GenerateCriticalStockPdf(List<ProductDto> products)
    {
        var markdown = GenerateCriticalStockReport(products);

        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var html = Markdown.ToHtml(markdown, pipeline);

        var styledHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            font-size: 12pt;
            line-height: 1.6;
            color: #333;
            margin: 20px;
            padding: 20px;
        }}
        h1 {{
            color: #2c3e50;
            border-bottom: 3px solid #3498db;
            padding-bottom: 10px;
            margin-bottom: 20px;
        }}
        h2 {{
            color: #34495e;
            margin-top: 30px;
            margin-bottom: 15px;
        }}
        table {{
            width: 100%;
            border-collapse: collapse;
            margin: 20px 0;
            font-size: 11pt;
        }}
        th {{
            background-color: #3498db;
            color: white;
            padding: 12px;
            text-align: left;
            font-weight: bold;
        }}
        td {{
            padding: 10px;
            border-bottom: 1px solid #ddd;
        }}
        tr:nth-child(even) {{
            background-color: #f9f9f9;
        }}
        strong {{
            color: #e74c3c;
            font-weight: bold;
        }}
        hr {{
            border: none;
            border-top: 2px solid #ecf0f1;
            margin: 20px 0;
        }}
        blockquote {{
            border-left: 4px solid #3498db;
            padding-left: 20px;
            margin: 20px 0;
            color: #7f8c8d;
            font-style: italic;
        }}
        ul {{
            margin: 10px 0;
            padding-left: 25px;
        }}
        li {{
            margin: 5px 0;
        }}
        @page {{
            margin: 2cm;
        }}
        @media print {{
            body {{
                margin: 0;
                padding: 15px;
            }}
        }}
    </style>
</head>
<body>
    {html}
</body>
</html>";

        await new BrowserFetcher().DownloadAsync();

        using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
        });

        using var page = await browser.NewPageAsync();

        await page.SetContentAsync(styledHtml, new NavigationOptions
        {
            WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
        });

        var pdfBytes = await page.PdfDataAsync(new PdfOptions
        {
            Format = PuppeteerSharp.Media.PaperFormat.A4,
            MarginOptions = new PuppeteerSharp.Media.MarginOptions
            {
                Top = "20mm",
                Bottom = "20mm",
                Left = "15mm",
                Right = "15mm"
            },
            PrintBackground = true
        });

        return pdfBytes ?? Array.Empty<byte>();
    }

    private static string GenerateCriticalStockReport(List<ProductDto> products)
    {
        var markdown = new System.Text.StringBuilder();

        markdown.AppendLine("# Kritik Stok Uyarıları Raporu");
        markdown.AppendLine();
        markdown.AppendLine($"**Rapor Tarihi:** {DateTime.Now:dd.MM.yyyy HH:mm}");
        markdown.AppendLine($"**Toplam Kritik Ürün Sayısı:** {products.Count}");
        markdown.AppendLine();
        markdown.AppendLine("---");
        markdown.AppendLine();

        if (!products.Any())
        {
            markdown.AppendLine("**Kritik stokta ürün bulunmamaktadır.**");
            return markdown.ToString();
        }

        markdown.AppendLine("**Aşağıdaki ürünlerin stok seviyeleri kritik düzeydedir.**");
        markdown.AppendLine();

        markdown.AppendLine("| # | Ürün Adı | Stok Kodu | Kategori | Mevcut Stok | Kritik Eşik | Eksik Miktar | Satın Alma | Satış | Envanter Maliyeti | Potansiyel Gelir | Potansiyel Kar | Durum |");
        markdown.AppendLine("|---|---|---|---|---|---|---|---|---|---|---|---|---|");

        var index = 1;
        foreach (var product in products)
        {
            var eksikMiktar = product.LowStockThreshold - product.StockQuantity;
            var durum = eksikMiktar > 10 ? "Çok Kritik" : eksikMiktar > 5 ? "Kritik" : "Dikkat";
            var purchasePrice = product.CurrentPurchasePrice;
            var salePrice = product.CurrentSalePrice;
            var inventoryCost = product.StockQuantity * purchasePrice;
            var potentialRevenue = product.StockQuantity * salePrice;
            var potentialProfit = potentialRevenue - inventoryCost;

            markdown.AppendLine($"| {index} | {EscapeMarkdown(product.Name)} | {EscapeMarkdown(product.StockCode)} | {EscapeMarkdown(product.CategoryName)} | **{product.StockQuantity}** | {product.LowStockThreshold} | **{eksikMiktar}** | ₺{purchasePrice:N2} | ₺{salePrice:N2} | ₺{inventoryCost:N2} | ₺{potentialRevenue:N2} | ₺{potentialProfit:N2} | {durum} |");
            index++;
        }

        markdown.AppendLine();
        markdown.AppendLine("---");
        markdown.AppendLine();

        var toplamEksik = products.Sum(p => p.LowStockThreshold - p.StockQuantity);
        var toplamMaliyet = products.Sum(p => p.StockQuantity * p.CurrentPurchasePrice);
        var toplamGelir = products.Sum(p => p.StockQuantity * p.CurrentSalePrice);
        var toplamKar = toplamGelir - toplamMaliyet;
        var ortalamaMarj = products.Any() && toplamMaliyet > 0
            ? (toplamKar / toplamMaliyet) * 100m
            : 0m;

        markdown.AppendLine("## Özet İstatistikler");
        markdown.AppendLine();
        markdown.AppendLine($"- **Toplam Eksik Stok:** {toplamEksik} adet");
        markdown.AppendLine($"- **Toplam Envanter Maliyeti:** ₺{toplamMaliyet:N2}");
        markdown.AppendLine($"- **Potansiyel Gelir:** ₺{toplamGelir:N2}");
        markdown.AppendLine($"- **Potansiyel Kar:** ₺{toplamKar:N2}");
        markdown.AppendLine($"- **Ortalama Marj:** {ortalamaMarj:N2} %");
        markdown.AppendLine($"- **En Kritik Ürün:** {products.First().Name} ({products.First().LowStockThreshold - products.First().StockQuantity} adet eksik)");
        markdown.AppendLine();

        var kategoriGruplu = products.GroupBy(p => p.CategoryName)
            .OrderByDescending(g => g.Count())
            .ToList();

        if (kategoriGruplu.Any())
        {
            markdown.AppendLine("## 📁 Kategorilere Göre Dağılım");
            markdown.AppendLine();
            foreach (var grup in kategoriGruplu)
            {
                markdown.AppendLine($"- **{EscapeMarkdown(grup.Key)}:** {grup.Count()} ürün");
            }
            markdown.AppendLine();
        }

        markdown.AppendLine("Not: Bu rapor otomatik olarak oluşturulmuştur.");

        return markdown.ToString();
    }

    private static string EscapeMarkdown(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return text
            .Replace("|", "\\|")
            .Replace("\n", " ")
            .Replace("\r", " ");
    }
}

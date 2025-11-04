using StockApp.App.Product.Query;

namespace StockApp.Services;

public interface IMarkdownService
{
    string GenerateCriticalStockReport(List<ProductDto> products);
}

public class MarkdownService : IMarkdownService
{
    public string GenerateCriticalStockReport(List<ProductDto> products)
    {
        var markdown = new System.Text.StringBuilder();
        
        // Başlık
        markdown.AppendLine("# Kritik Stok Uyarıları Raporu");
        markdown.AppendLine();
        markdown.AppendLine($"**Rapor Tarihi:** {DateTime.Now:dd.MM.yyyy HH:mm}");
        markdown.AppendLine($"**Toplam Kritik Ürün Sayısı:** {products.Count}");
        markdown.AppendLine();
        markdown.AppendLine("---");
        markdown.AppendLine();

        if (!products.Any())
        {
            markdown.AppendLine("✅ **Kritik stokta ürün bulunmamaktadır.**");
            return markdown.ToString();
        }

        // Uyarı mesajı
        markdown.AppendLine("⚠️ **Aşağıdaki ürünlerin stok seviyeleri kritik düzeydedir.**");
        markdown.AppendLine();

        // Tablo başlığı
        markdown.AppendLine("| # | Ürün Adı | Stok Kodu | Kategori | Mevcut Stok | Kritik Eşik | Eksik Miktar | Durum |");
        markdown.AppendLine("|---|---|---|---|---|---|---|---|");

        // Ürünleri ekle
        int index = 1;
        foreach (var product in products)
        {
            var eksikMiktar = product.LowStockThreshold - product.StockQuantity;
            var durum = eksikMiktar > 10 ? "🔴 Çok Kritik" : eksikMiktar > 5 ? "🟡 Kritik" : "🟠 Dikkat";

            markdown.AppendLine($"| {index} | {EscapeMarkdown(product.Name)} | {EscapeMarkdown(product.StockCode)} | {EscapeMarkdown(product.CategoryName)} | **{product.StockQuantity}** | {product.LowStockThreshold} | **{eksikMiktar}** | {durum} |");
            index++;
        }

        markdown.AppendLine();
        markdown.AppendLine("---");
        markdown.AppendLine();

        // Özet istatistikler
        var toplamEksik = products.Sum(p => p.LowStockThreshold - p.StockQuantity);
        var ortalamaEksik = products.Any() ? toplamEksik / products.Count : 0;
        
        markdown.AppendLine("## 📊 Özet İstatistikler");
        markdown.AppendLine();
        markdown.AppendLine($"- **Toplam Eksik Stok:** {toplamEksik} adet");
        markdown.AppendLine($"- **En Kritik Ürün:** {products.First().Name} ({products.First().LowStockThreshold - products.First().StockQuantity} adet eksik)");
        markdown.AppendLine();

        // Kategorilere göre dağılım
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

        // Not
        markdown.AppendLine("> **Not:** Bu rapor otomatik olarak oluşturulmuştur.");

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


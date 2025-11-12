using System.Globalization;
using System.Linq;
using System.Text;
using MediatR;
using Microsoft.Extensions.Logging;
using StockApp.App.Dashboard.Query;
using StockApp.Services;

namespace StockApp.App.Reports.Query;

public record GetNaturalLanguageReportQuery(string Question) : IRequest<NaturalLanguageReportResponse>;

public record NaturalLanguageReportResponse(bool Success, string Message, string? Model = null, bool IsConfigured = true);

internal class GetNaturalLanguageReportQueryHandler : IRequestHandler<GetNaturalLanguageReportQuery, NaturalLanguageReportResponse>
{
    private readonly IMediator _mediator;
    private readonly IGeminiService _geminiService;
    private readonly ILogger<GetNaturalLanguageReportQueryHandler> _logger;

    public GetNaturalLanguageReportQueryHandler(
        IMediator mediator,
        IGeminiService geminiService,
        ILogger<GetNaturalLanguageReportQueryHandler> logger)
    {
        _mediator = mediator;
        _geminiService = geminiService;
        _logger = logger;
    }

    public async Task<NaturalLanguageReportResponse> Handle(GetNaturalLanguageReportQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return new NaturalLanguageReportResponse(false, "Soru metni boş olamaz.");
        }

        var stats = await _mediator.Send(new GetDashboardStatsQuery(), cancellationToken);
        var prompt = BuildPrompt(request.Question, stats);

        var result = await _geminiService.GenerateTextAsync(prompt, cancellationToken);
        if (!result.Success)
        {
            _logger.LogWarning("Gemini raporu üretilemedi. Success: {Success}, Message: {Message}", result.Success, result.Message);
            return new NaturalLanguageReportResponse(false, result.Message, result.Model, result.IsConfigured);
        }

        return new NaturalLanguageReportResponse(true, result.Message, result.Model, result.IsConfigured);
    }

    private static string BuildPrompt(string question, DashboardStatsDto stats)
    {
        var culture = new CultureInfo("tr-TR");
        var sb = new StringBuilder();

        sb.AppendLine("Sen bir stok yönetim asistanısın. Kullanıcı sorularını sadece verilen verileri kullanarak cevapla.");
        sb.AppendLine("Finansal değerleri Türk Lirası formatında (₺) ve metrikleri Türkçe olarak ifade et.");
        sb.AppendLine("Aşağıda sistemden alınan özet veriler bulunuyor:");
        sb.AppendLine();

        sb.AppendLine("Genel İstatistikler:");
        sb.AppendLine($"- Toplam Ürün: {stats.TotalProducts}");
        sb.AppendLine($"- Toplam Stok Miktarı: {stats.TotalStockQuantity}");
        sb.AppendLine($"- Toplam Envanter Maliyeti: {FormatCurrency(stats.TotalInventoryCost, culture)}");
        sb.AppendLine($"- Beklenen Toplam Satış: {FormatCurrency(stats.TotalExpectedSalesRevenue, culture)}");
        sb.AppendLine($"- Potansiyel Kar: {FormatCurrency(stats.TotalPotentialProfit, culture)}");
        sb.AppendLine($"- Ortalama Marj: {FormatPercentage(stats.AverageMarginPercentage)}");
        sb.AppendLine();

        var topValuable = stats.TopValuableProducts.Take(5).ToList();
        if (topValuable.Any())
        {
            sb.AppendLine("En Değerli Ürünler (İlk 5):");
            foreach (var product in topValuable)
            {
                sb.AppendLine(
                    $"- {product.ProductName}: Envanter Maliyeti {FormatCurrency(product.InventoryCost, culture)}, Beklenen Satış {FormatCurrency(product.InventoryPotentialRevenue, culture)}, Potansiyel Kar {FormatCurrency(product.PotentialProfit, culture)}");
            }
            sb.AppendLine();
        }

        var topCategories = stats.CategoryValueDistribution
            .OrderByDescending(c => c.TotalPotentialRevenue)
            .Take(5)
            .ToList();
        if (topCategories.Any())
        {
            sb.AppendLine("Kategori Bazlı Özet (İlk 5):");
            foreach (var category in topCategories)
            {
                sb.AppendLine(
                    $"- {category.CategoryName}: Toplam Maliyet {FormatCurrency(category.TotalCost, culture)}, Beklenen Satış {FormatCurrency(category.TotalPotentialRevenue, culture)}, Beklenen Kar {FormatCurrency(category.TotalPotentialProfit, culture)}");
            }
            sb.AppendLine();
        }

        var recentMovements = stats.RecentStockMovements
            .OrderByDescending(m => m.CreatedAt)
            .Take(10)
            .ToList();
        if (recentMovements.Any())
        {
            sb.AppendLine("Son Stok Hareketleri (10 kayıt):");
            foreach (var movement in recentMovements)
            {
                sb.AppendLine(
                    $"- {movement.CreatedAt.ToString("dd.MM.yyyy HH:mm", culture)} | {movement.ProductName} | {movement.TypeText} | Adet: {movement.Quantity} | Açıklama: {movement.Description ?? "-"}");
            }
            sb.AppendLine();
        }

        sb.AppendLine("Kullanıcı Sorusu:");
        sb.AppendLine(question.Trim());
        sb.AppendLine();
        sb.AppendLine("Yönergeler:");
        sb.AppendLine("- Soruyu Türkçe yanıtla.");
        sb.AppendLine("- Cevabı kısa paragraflar veya madde işaretleriyle düzenle.");
        sb.AppendLine("- Tahmin yapman gerekiyorsa varsayımlarını açıkça belirt.");
        sb.AppendLine("- Yanıtın sonunda verilerin hangi tarih aralığına ait olduğuna dair kısa bir özet ekle.");

        return sb.ToString();
    }

    private static string FormatCurrency(decimal value, CultureInfo culture)
    {
        return string.Format(culture, "{0:C}", value);
    }

    private static string FormatPercentage(decimal value)
    {
        return $"{Math.Round(value, 2):0.##}%";
    }
}



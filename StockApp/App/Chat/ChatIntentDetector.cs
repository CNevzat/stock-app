using System.Globalization;
using System.Text.RegularExpressions;
using StockApp.Entities;

namespace StockApp.App.Chat;

public interface IChatIntentDetector
{
    ChatQuestionContext Analyse(string question);
}

public class ChatIntentDetector : IChatIntentDetector
{
    private static readonly CultureInfo TurkishCulture = new("tr-TR");
    private static readonly Dictionary<string, ChatIntent> HelpKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        { "ürün ekle", ChatIntent.HowToAddProduct },
        { "ürün oluştur", ChatIntent.HowToAddProduct },
        { "ürün nasıl eklenir", ChatIntent.HowToAddProduct },
        { "dashboard nasıl", ChatIntent.HowToUseDashboard },
        { "dashboard kullan", ChatIntent.HowToUseDashboard },
        { "uygulama nasıl", ChatIntent.GeneralAppHelp },
        { "nasıl kullan", ChatIntent.GeneralAppHelp },
        { "ne yapabilirsin", ChatIntent.AiAssistantInfo },
        { "ne yapıyorsun", ChatIntent.AiAssistantInfo }
    };

    private static readonly string[] SmallTalkKeywords =
    [
        "merhaba", "selam", "naber", "nasılsın", "teşekkür", "sağol"
    ];

    public ChatQuestionContext Analyse(string question)
    {
        var normalized = (question ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return new ChatQuestionContext(ChatIntent.Unknown, string.Empty);
        }

        var lower = normalized.ToLower(TurkishCulture);

        if (SmallTalkKeywords.Any(k => lower.Contains(k)))
        {
            return new ChatQuestionContext(ChatIntent.SmallTalk, normalized);
        }

        foreach (var kvp in HelpKeywords)
        {
            if (lower.Contains(kvp.Key))
            {
                return new ChatQuestionContext(kvp.Value, normalized);
            }
        }

        if (lower.Contains("ürün güncelle") || lower.Contains("ürünü düzenle") || lower.Contains("ürün düzenleme"))
        {
            return new ChatQuestionContext(ChatIntent.HowToUpdateProduct, normalized);
        }

        if (lower.Contains("ürün sil") || lower.Contains("ürünü sil") || lower.Contains("ürün silme"))
        {
            return new ChatQuestionContext(ChatIntent.HowToDeleteProduct, normalized);
        }

        if (lower.Contains("kategori ekle") || lower.Contains("kategori oluştur") || lower.Contains("kategori düzenle") || lower.Contains("kategori sil"))
        {
            return new ChatQuestionContext(ChatIntent.HowToManageCategory, normalized);
        }

        if (lower.Contains("lokasyon ekle") || lower.Contains("depo ekle") || lower.Contains("lokasyon düzenle") || lower.Contains("lokasyon sil"))
        {
            return new ChatQuestionContext(ChatIntent.HowToManageLocation, normalized);
        }

        if (lower.Contains("öznitelik ekle") || lower.Contains("attribute ekle") || lower.Contains("ürün özelliği ekle"))
        {
            return new ChatQuestionContext(ChatIntent.HowToAddAttribute, normalized);
        }

        if (lower.Contains("öznitelik sil") || lower.Contains("attribute sil") || lower.Contains("öznitelik düzenle") || lower.Contains("attribute düzenle"))
        {
            return new ChatQuestionContext(ChatIntent.HowToManageAttribute, normalized);
        }

        if (lower.Contains("stok hareketleri nasıl") || lower.Contains("stok hareketleri görüntüle") || lower.Contains("stok hareketi nerede") || lower.Contains("stok hareketleri listesi"))
        {
            return new ChatQuestionContext(ChatIntent.HowToViewStockMovements, normalized);
        }

        if (lower.Contains("yapılacaklar") || lower.Contains("todo") || lower.Contains("görev nasıl") || lower.Contains("görev tamamla") || lower.Contains("görev sil") || lower.Contains("görev güncelle"))
        {
            return new ChatQuestionContext(ChatIntent.HowToUseTodos, normalized);
        }

        var range = ParseDateRange(lower);
        var requiresDetail = lower.Contains("liste") || lower.Contains("detay") || lower.Contains("hepsi");
        string? productKeyword = ExtractQuoted(lower);
        string? categoryKeyword = ExtractAfterKeyword(lower, ["kategori", "kategoride", "kategorisi"]);

        var movementType = DetectMovementType(lower);

        // Intent detection heuristics
        if (lower.Contains("stok giriş"))
        {
            if (lower.Contains("en çok") || lower.Contains("hangi"))
            {
                return new ChatQuestionContext(ChatIntent.TopStockInProducts, normalized, range, StockMovementType.In, ProductKeyword: productKeyword, RequiresDetailedList: requiresDetail);
            }

            return new ChatQuestionContext(ChatIntent.StockMovementByType, normalized, range, StockMovementType.In, ProductKeyword: productKeyword, CategoryKeyword: categoryKeyword, RequiresDetailedList: requiresDetail);
        }

        if (lower.Contains("stok çıkış"))
        {
            if (lower.Contains("en çok") || lower.Contains("hangi"))
            {
                return new ChatQuestionContext(ChatIntent.TopStockOutProducts, normalized, range, StockMovementType.Out, ProductKeyword: productKeyword, RequiresDetailedList: requiresDetail);
            }

            return new ChatQuestionContext(ChatIntent.StockMovementByType, normalized, range, StockMovementType.Out, ProductKeyword: productKeyword, CategoryKeyword: categoryKeyword, RequiresDetailedList: requiresDetail);
        }

        if (lower.Contains("stok hareket") || lower.Contains("hareketleri"))
        {
            return new ChatQuestionContext(ChatIntent.StockMovementSummary, normalized, range, movementType, productKeyword, categoryKeyword, RequiresDetailedList: requiresDetail);
        }

        if (lower.Contains("kâr") || lower.Contains("kar") || lower.Contains("marj"))
        {
            return new ChatQuestionContext(ChatIntent.MostProfitableCategory, normalized, range);
        }

        if (lower.Contains("satış potansiyeli") || lower.Contains("beklenen satış") || lower.Contains("potansiyel gelir"))
        {
            return new ChatQuestionContext(ChatIntent.SalesPotential, normalized, range);
        }

        if (lower.Contains("stok değeri") || lower.Contains("envanter değeri") || (lower.Contains("toplam stok") && lower.Contains("değer")))
        {
            return new ChatQuestionContext(ChatIntent.InventoryValue, normalized, range);
        }

        if (lower.Contains("en fazla stok") || lower.Contains("en çok stok") || lower.Contains("stok miktarı en yüksek") || lower.Contains("stokları en yüksek"))
        {
            return new ChatQuestionContext(ChatIntent.TopStockQuantityProduct, normalized, range, ProductKeyword: productKeyword, CategoryKeyword: categoryKeyword, RequiresDetailedList: requiresDetail);
        }

        if (lower.Contains("ortalama fiyat") || lower.Contains("alış fiyatı") && lower.Contains("satış fiyatı"))
        {
            return new ChatQuestionContext(ChatIntent.AveragePrices, normalized, range, ProductKeyword: productKeyword, CategoryKeyword: categoryKeyword);
        }

        if (lower.Contains("ürünün durumu") || lower.Contains("ürünün stoğu") || lower.Contains("ürün stoğu"))
        {
            if (!string.IsNullOrWhiteSpace(productKeyword))
            {
                return new ChatQuestionContext(ChatIntent.ProductCurrentStatus, normalized, range, ProductKeyword: productKeyword);
            }
        }

        if (lower.Contains("kategori özeti") || lower.Contains("kategori stok") || lower.Contains("kategoride ne kadar"))
        {
            if (!string.IsNullOrWhiteSpace(categoryKeyword))
            {
                return new ChatQuestionContext(ChatIntent.CategoryInventorySummary, normalized, range, CategoryKeyword: categoryKeyword);
            }
        }

        return new ChatQuestionContext(ChatIntent.Unknown, normalized, range, movementType, productKeyword, categoryKeyword, requiresDetail);
    }

    private static StockMovementType? DetectMovementType(string text)
    {
        if (text.Contains("giriş") && !text.Contains("çıkış"))
        {
            return StockMovementType.In;
        }

        if (text.Contains("çıkış") && !text.Contains("giriş"))
        {
            return StockMovementType.Out;
        }

        return null;
    }

    private static DateRange? ParseDateRange(string text)
    {
        var now = DateTime.UtcNow;

        if (text.Contains("geçen yıl"))
        {
            var start = new DateTime(now.Year - 1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(now.Year - 1, 12, 31, 23, 59, 59, DateTimeKind.Utc);
            return DateRange.From(start, end);
        }

        if (text.Contains("geçen ay"))
        {
            var firstDayThisMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var start = firstDayThisMonth.AddMonths(-1);
            var end = firstDayThisMonth.AddTicks(-1);
            return DateRange.From(start, end);
        }

        if (text.Contains("bu ay"))
        {
            var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1).AddTicks(-1);
            return DateRange.From(start, end);
        }

        if (text.Contains("geçen hafta"))
        {
            var startOfWeek = StartOfWeekUtc(now, DayOfWeek.Monday).AddDays(-7);
            var endOfWeek = startOfWeek.AddDays(7).AddTicks(-1);
            return DateRange.From(startOfWeek, endOfWeek);
        }

        if (text.Contains("bu hafta"))
        {
            var start = StartOfWeekUtc(now, DayOfWeek.Monday);
            var end = start.AddDays(7).AddTicks(-1);
            return DateRange.From(start, end);
        }

        if (text.Contains("dün"))
        {
            var start = now.Date.AddDays(-1);
            var end = start.AddDays(1).AddTicks(-1);
            return DateRange.From(start, end);
        }

        if (text.Contains("bugün"))
        {
            var start = now.Date;
            var end = start.AddDays(1).AddTicks(-1);
            return DateRange.From(start, end);
        }

        // Gün ismi (örn: "Cuma")
        foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
        {
            var dayName = TurkishCulture.DateTimeFormat.GetDayName(dayOfWeek).ToLower(TurkishCulture);
            if (text.Contains(dayName))
            {
                var reference = StartOfWeekUtc(now, DayOfWeek.Monday);
                var target = reference.AddDays((int)dayOfWeek - (int)DayOfWeek.Monday);
                if (target > now)
                {
                    target = target.AddDays(-7);
                }
                var start = target;
                var end = start.AddDays(1).AddTicks(-1);
                return DateRange.From(start, end);
            }
        }

        // Tarih aralığı (dd.mm.yyyy - dd.mm.yyyy)
        var rangeMatch = Regex.Match(text, @"(\d{1,2}[./-]\d{1,2}[./-]\d{2,4})\s*(?:ile|ve|-|–|—)\s*(\d{1,2}[./-]\d{1,2}[./-]\d{2,4})");
        if (rangeMatch.Success)
        {
            if (DateTime.TryParse(rangeMatch.Groups[1].Value, TurkishCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AdjustToUniversal, out var startDate) &&
                DateTime.TryParse(rangeMatch.Groups[2].Value, TurkishCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AdjustToUniversal, out var endDate))
            {
                var start = startDate.Date;
                var end = endDate.Date.AddDays(1).AddTicks(-1);
                return DateRange.From(start, end);
            }
        }

        return null;
    }

    private static DateTime StartOfWeekUtc(DateTime dt, DayOfWeek startOfWeek)
    {
        int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
        return dt.Date.AddDays(-1 * diff).ToUniversalTime();
    }

    private static string? ExtractQuoted(string text)
    {
        var match = Regex.Match(text, "\"([^\"]+)\"");
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        match = Regex.Match(text, "'([^']+)'");
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        return null;
    }

    private static string? ExtractAfterKeyword(string text, IReadOnlyList<string> keywords)
    {
        foreach (var keyword in keywords)
        {
            var index = text.IndexOf(keyword, StringComparison.Ordinal);
            if (index >= 0)
            {
                var remainder = text[(index + keyword.Length)..].Trim();
                remainder = remainder.TrimStart(':', '-', '=', ' ');
                if (!string.IsNullOrWhiteSpace(remainder))
                {
                    var words = remainder.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length > 0)
                    {
                        return string.Join(' ', words.Take(4)).Trim();
                    }
                }
            }
        }

        return null;
    }
}



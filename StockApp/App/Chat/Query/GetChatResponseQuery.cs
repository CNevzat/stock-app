using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StockApp.App.Dashboard.Query;
using StockApp.App.StockMovement.Query;
using StockApp.Entities;
using StockApp.Services;

namespace StockApp.App.Chat.Query;

public record GetChatResponseQuery(string Question) : IRequest<ChatResponseDto>;

internal class GetChatResponseQueryHandler : IRequestHandler<GetChatResponseQuery, ChatResponseDto>
{
    private readonly IChatIntentDetector _intentDetector;
    private readonly IGeminiService _geminiService;
    private readonly ApplicationDbContext _context;
    private readonly IMediator _mediator;
    private readonly IGeminiIntentClassifier _geminiIntentClassifier;

    public GetChatResponseQueryHandler(
        IChatIntentDetector intentDetector,
        IGeminiService geminiService,
        ApplicationDbContext context,
        IMediator mediator,
        IGeminiIntentClassifier geminiIntentClassifier)
    {
        _intentDetector = intentDetector;
        _geminiService = geminiService;
        _context = context;
        _mediator = mediator;
        _geminiIntentClassifier = geminiIntentClassifier;
    }

    public async Task<ChatResponseDto> Handle(GetChatResponseQuery request, CancellationToken cancellationToken)
    {
        var context = _intentDetector.Analyse(request.Question);
        if (context.Intent == ChatIntent.Unknown)
        {
            var classifiedIntent = await _geminiIntentClassifier.ClassifyAsync(request.Question, cancellationToken);
            if (classifiedIntent != ChatIntent.Unknown)
            {
                context = context with { Intent = classifiedIntent };
            }
        }

        if (IsHelpIntent(context.Intent))
        {
            var helpMessage = BuildHelpResponse(context.Intent);
            return new ChatResponseDto(
                helpMessage,
                context.Intent,
                Suggestions: GetSuggestions(context.Intent) ?? GetFallbackSuggestions(),
                DebugContext: $"Help intent handled directly: {context.Intent}");
        }
        var prompt = new StringBuilder();
        prompt.AppendLine("Sen StockApp adlı stok yönetimi uygulamasının yapay zekâ asistanısın.");
        prompt.AppendLine("Kullanıcının sorusunu yalnızca verilen verileri kullanarak, Türkçe olarak cevapla.");
        prompt.AppendLine("Şirket içi veriler dışındaki bilgileri uydurma; emin değilsen dürüstçe belirt.");
        prompt.AppendLine("Yanıtını kısa paragraflar ve gerekirse madde işaretleri ile düzenle.");
        prompt.AppendLine("Her yanıtın sonunda bir sonraki adım önerisi veya ilgili bir ipucu ver.");
        prompt.AppendLine();

        prompt.AppendLine($"Kullanıcının sorusu: \"{context.OriginalQuestion}\"");
        prompt.AppendLine($"Tespit edilen intent: {context.Intent}");

        var (dataSummary, hasRelevantData) = await BuildContextDataAsync(context, prompt, cancellationToken);

        if (context.Intent == ChatIntent.Unknown || !hasRelevantData)
        {
            var politeMessage = "Üzgünüm, şu anda bu bilgiye doğrudan erişemiyorum. Başka sormak istediğin bir şey var mı?";
            return new ChatResponseDto(
                politeMessage,
                context.Intent,
                Suggestions: GetSuggestions(context.Intent) ?? GetFallbackSuggestions(),
                DebugContext: dataSummary);
        }

        prompt.AppendLine();
        prompt.AppendLine("Yanıt formatı yönergeleri:");
        prompt.AppendLine("- Başta kısa bir özet ver.");
        prompt.AppendLine("- Ardından madde madde önemli bulguları paylaş.");
        prompt.AppendLine("- Son satırda \"Öneri:\" ile başlayan bir öneri sun.");

        var geminiResult = await _geminiService.GenerateTextAsync(prompt.ToString(), cancellationToken);
        if (!geminiResult.Success)
        {
            var fallback = geminiResult.IsConfigured
                ? "Şu anda isteğini işlerken bir sorun oluştu. Başka bir soru sormak ister misin?"
                : "Yapay zekâ servisi yapılandırılmadığı için şu anda bu soruyu yanıtlayamıyorum.";

            return new ChatResponseDto(
                fallback,
                context.Intent,
                Suggestions: GetSuggestions(context.Intent) ?? GetFallbackSuggestions(),
                DebugContext: dataSummary);
        }

        return new ChatResponseDto(
            geminiResult.Message,
            context.Intent,
            Suggestions: GetSuggestions(context.Intent) ?? GetFallbackSuggestions(),
            DebugContext: dataSummary);
    }

    private async Task<(string DebugInfo, bool HasRelevantData)> BuildContextDataAsync(ChatQuestionContext context, StringBuilder prompt, CancellationToken cancellationToken)
    {
        var debugBuilder = new StringBuilder();
        debugBuilder.AppendLine($"Intent: {context.Intent}");
        var hasRelevantData = false;

        if (context.Range is not null)
        {
            debugBuilder.AppendLine($"DateRange: {context.Range}");
            prompt.AppendLine($"Tarih aralığı: {context.Range}");
        }
        else
        {
            prompt.AppendLine("Tarih aralığı: belirtilmedi (varsayılan: son 30 gün)");
        }

        if (!string.IsNullOrWhiteSpace(context.ProductKeyword))
        {
            prompt.AppendLine($"Ürün anahtar kelimesi: {context.ProductKeyword}");
            debugBuilder.AppendLine($"ProductKeyword: {context.ProductKeyword}");
        }

        if (!string.IsNullOrWhiteSpace(context.CategoryKeyword))
        {
            prompt.AppendLine($"Kategori anahtar kelimesi: {context.CategoryKeyword}");
            debugBuilder.AppendLine($"CategoryKeyword: {context.CategoryKeyword}");
        }

        if (context.MovementType.HasValue)
        {
            prompt.AppendLine($"Hareket tipi: {context.MovementType}");
            debugBuilder.AppendLine($"MovementType: {context.MovementType}");
        }

        switch (context.Intent)
        {
            case ChatIntent.InventoryValue:
            case ChatIntent.MostProfitableCategory:
            case ChatIntent.SalesPotential:
            case ChatIntent.AveragePrices:
            case ChatIntent.CategoryInventorySummary:
            case ChatIntent.ProductCurrentStatus:
                await AppendDashboardDrivenData(context, prompt, debugBuilder, cancellationToken);
                hasRelevantData = true;
                break;

            case ChatIntent.TopStockInProducts:
            case ChatIntent.TopStockOutProducts:
            case ChatIntent.StockMovementSummary:
            case ChatIntent.StockMovementByType:
                await AppendStockMovementData(context, prompt, debugBuilder, cancellationToken);
                hasRelevantData = true;
                break;

            case ChatIntent.TopStockQuantityProduct:
                await AppendTopStockProducts(context, prompt, debugBuilder, cancellationToken);
                hasRelevantData = true;
                break;

            case ChatIntent.HowToAddProduct:
            case ChatIntent.HowToUseDashboard:
            case ChatIntent.GeneralAppHelp:
            case ChatIntent.AiAssistantInfo:
                AppendHelpInstructions(context, prompt, debugBuilder);
                hasRelevantData = true;
                break;

            case ChatIntent.SmallTalk:
                prompt.AppendLine("Kullanıcı selamlaşma veya küçük konuşma tarzında bir mesaj gönderdi.");
                debugBuilder.AppendLine("SmallTalk intent.");
                hasRelevantData = true;
                break;

            default:
                prompt.AppendLine("Bu intent için özel veri hazırlığı bulunamadı. Kullanıcıya nazikçe bilgi ver.");
                debugBuilder.AppendLine("Intent not implemented with data enrichment.");
                break;
        }

        return (debugBuilder.ToString(), hasRelevantData);
    }

    private async Task AppendDashboardDrivenData(ChatQuestionContext context, StringBuilder prompt, StringBuilder debugBuilder, CancellationToken cancellationToken)
    {
        var stats = await _mediator.Send(new GetDashboardStatsQuery(), cancellationToken);

        prompt.AppendLine();
        prompt.AppendLine("Dashboard verileri:");
        prompt.AppendLine($"- Toplam stok miktarı: {stats.TotalStockQuantity}");
        prompt.AppendLine($"- Toplam envanter maliyeti: {stats.TotalInventoryCost:N2}");
        prompt.AppendLine($"- Beklenen toplam satış geliri: {stats.TotalExpectedSalesRevenue:N2}");
        prompt.AppendLine($"- Potansiyel kâr: {stats.TotalPotentialProfit:N2}");
        prompt.AppendLine($"- Ortalama marj: {stats.AverageMarginPercentage:N2}%");

        debugBuilder.AppendLine("Dashboard stats appended.");

        if (context.Intent is ChatIntent.MostProfitableCategory or ChatIntent.CategoryInventorySummary)
        {
            var categories = stats.CategoryValueDistribution
                .OrderByDescending(c => c.TotalPotentialProfit)
                .Take(context.RequiresDetailedList ? 10 : 5)
                .ToList();

            prompt.AppendLine();
            prompt.AppendLine("Kategori bazlı değerler:");
            foreach (var cat in categories)
            {
                prompt.AppendLine($"- {cat.CategoryName}: Maliyet={cat.TotalCost:N2}, Beklenen Satış={cat.TotalPotentialRevenue:N2}, Kâr={cat.TotalPotentialProfit:N2}");
            }
        }

        if (context.Intent == ChatIntent.AveragePrices)
        {
            var products = stats.TopValuableProducts.Take(context.RequiresDetailedList ? 10 : 5).ToList();
            prompt.AppendLine();
            prompt.AppendLine("En değerli ürünlerden fiyat özetleri:");
            foreach (var product in products)
            {
                prompt.AppendLine($"- {product.ProductName} ({product.StockCode}): Maliyet={product.InventoryCost:N2}, Beklenen Satış={product.InventoryPotentialRevenue:N2}, Kâr={product.PotentialProfit:N2}");
            }
        }

        if (context.Intent == ChatIntent.ProductCurrentStatus && !string.IsNullOrWhiteSpace(context.ProductKeyword))
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.PriceHistory)
                .Where(p => EF.Functions.Like(p.Name.ToLower(), $"%{context.ProductKeyword!.ToLower()}%"))
                .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (product is not null)
            {
                prompt.AppendLine();
                prompt.AppendLine("İlgili ürün detayı:");
                prompt.AppendLine($"- Ürün: {product.Name} ({product.StockCode})");
                prompt.AppendLine($"- Kategori: {product.Category?.Name ?? "Belirtilmemiş"}");
                prompt.AppendLine($"- Mevcut stok: {product.StockQuantity}");
                prompt.AppendLine($"- Son alış fiyatı: {product.CurrentPurchasePrice:N2}");
                prompt.AppendLine($"- Son satış fiyatı: {product.CurrentSalePrice:N2}");

                var lastMovement = await _context.StockMovements
                    .Where(sm => sm.ProductId == product.Id)
                    .OrderByDescending(sm => sm.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                if (lastMovement is not null)
                {
                    prompt.AppendLine($"- Son stok hareketi: {lastMovement.CreatedAt:dd.MM.yyyy HH:mm}, {lastMovement.Type}, miktar {lastMovement.Quantity}, açıklama: {lastMovement.Description ?? "-"}");
                }
            }
            else
            {
                prompt.AppendLine();
                prompt.AppendLine("Belirtilen ürün adına uygun kayıt bulunamadı. Kullanıcıya nazikçe belirt.");
            }
        }
    }

    private async Task AppendStockMovementData(ChatQuestionContext context, StringBuilder prompt, StringBuilder debugBuilder, CancellationToken cancellationToken)
    {
        var range = context.Range ?? DateRange.From(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
        debugBuilder.AppendLine($"EffectiveRange: {range}");

        var query = _context.StockMovements
            .Include(sm => sm.Product)
            .Include(sm => sm.Category)
            .Where(sm => sm.CreatedAt >= range.Start && sm.CreatedAt <= range.End);

        if (context.MovementType.HasValue)
        {
            query = query.Where(sm => sm.Type == context.MovementType.Value);
        }

        if (!string.IsNullOrWhiteSpace(context.ProductKeyword))
        {
            var keyword = context.ProductKeyword.ToLower();
            query = query.Where(sm => sm.Product.Name.ToLower().Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(context.CategoryKeyword))
        {
            var keyword = context.CategoryKeyword.ToLower();
            query = query.Where(sm => sm.Category.Name.ToLower().Contains(keyword));
        }

        var take = context.RequiresDetailedList ? 20 : 10;
        var movements = await query
            .OrderByDescending(sm => sm.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        if (movements.Count == 0)
        {
            prompt.AppendLine();
            prompt.AppendLine("Belirtilen kriterlere uygun stok hareketi bulunamadı.");
            return;
        }

        prompt.AppendLine();
        prompt.AppendLine("İlgili stok hareketleri (en güncelden başlayarak):");
        foreach (var movement in movements)
        {
            prompt.AppendLine($"- {movement.CreatedAt:dd.MM.yyyy HH:mm} | {movement.Product.Name} | {movement.Type} | Adet: {movement.Quantity} | Birim Fiyat: {movement.UnitPrice:N2} | Açıklama: {movement.Description ?? "-"}");
        }

        var grouped = movements
            .GroupBy(sm => sm.Product.Name)
            .Select(g => new
            {
                Product = g.Key,
                TotalQuantity = g.Sum(x => x.Quantity),
                MovementCount = g.Count()
            })
            .OrderByDescending(g => g.TotalQuantity)
            .Take(5)
            .ToList();

        prompt.AppendLine();
        prompt.AppendLine("Özet (seçilen hareketler):");
        foreach (var item in grouped)
        {
            prompt.AppendLine($"- {item.Product}: Toplam miktar {item.TotalQuantity}, hareket sayısı {item.MovementCount}");
        }
    }

    private async Task AppendTopStockProducts(ChatQuestionContext context, StringBuilder prompt, StringBuilder debugBuilder, CancellationToken cancellationToken)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(context.CategoryKeyword))
        {
            var keyword = context.CategoryKeyword.ToLower();
            query = query.Where(p => p.Category != null && p.Category.Name.ToLower().Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(context.ProductKeyword))
        {
            var keyword = context.ProductKeyword.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(keyword));
        }

        var products = await query
            .OrderByDescending(p => p.StockQuantity)
            .ThenBy(p => p.Name)
            .Take(context.RequiresDetailedList ? 10 : 5)
            .ToListAsync(cancellationToken);

        if (products.Count == 0)
        {
            prompt.AppendLine();
            prompt.AppendLine("Belirtilen kriterlere uygun stok bilgisi bulunamadı.");
            return;
        }

        prompt.AppendLine();
        prompt.AppendLine("Stok miktarı en yüksek ürünler:");
        foreach (var product in products)
        {
            prompt.AppendLine($"- {product.Name} ({product.StockCode}) | Stok: {product.StockQuantity} | Kategori: {product.Category?.Name ?? "-"} | Son Satış Fiyatı: {product.CurrentSalePrice:N2}");
        }

        var topProduct = products.First();
        debugBuilder.AppendLine($"TopStockProduct: {topProduct.Name} ({topProduct.StockQuantity})");
    }

    private static void AppendHelpInstructions(ChatQuestionContext context, StringBuilder prompt, StringBuilder debugBuilder)
    {
        prompt.AppendLine();
        prompt.AppendLine("Bu soru bir yardım / rehber talebidir. Kullanıcıyı adım adım yönlendir.");

        switch (context.Intent)
        {
            case ChatIntent.HowToAddProduct:
                prompt.AppendLine("README'deki ürün ekleme adımları:");
                prompt.AppendLine("1. Ürünler sayfasındaki 'Yeni Ürün Ekle' butonuna tıklanır.");
                prompt.AppendLine("2. Ürün adı, stok miktarı, kategori ve fiyat bilgileri girilir.");
                prompt.AppendLine("3. Gerekirse ürün resmi eklenir ve 'Kaydet' ile işlem tamamlanır.");
                break;

            case ChatIntent.HowToUpdateProduct:
                prompt.AppendLine("Ürün düzenleme adımları:");
                prompt.AppendLine("1. Ürünler tablosunda düzenlemek istediğin ürünün yanındaki 'Düzenle' butonuna tıkla.");
                prompt.AppendLine("2. Açılan formda fiyat, stok, kategori gibi alanları güncelle.");
                prompt.AppendLine("3. Yeni bir resim seçmek istersen aynı formda yükleyebilir, 'Güncelle' ile kaydedebilirsin.");
                prompt.AppendLine("4. Değişiklik sonrası SignalR sayesinde liste otomatik güncellenir.");
                break;

            case ChatIntent.HowToDeleteProduct:
                prompt.AppendLine("Ürün silme adımları:");
                prompt.AppendLine("1. Ürünler tablosunda ilgili ürünün yanındaki 'Sil' butonuna tıkla.");
                prompt.AppendLine("2. Onay penceresinde 'Evet, Sil' diyerek işlemi tamamla.");
                prompt.AppendLine("3. Silinen ürünün stok ve fiyat geçmişi de kaldırılacağını kullanıcıya belirt.");
                break;

            case ChatIntent.HowToUseDashboard:
                prompt.AppendLine("Dashboard kullanımına dair özet:");
                prompt.AppendLine("- Üstte genel finansal kartlar (envanter değeri, beklenen satış vb.) bulunur.");
                prompt.AppendLine("- Kategori bazlı grafik toplam maliyet, beklenen satış ve kârı gösterir.");
                prompt.AppendLine("- Son stok hareketleri tablosu en güncel giriş/çıkışları listeler.");
                break;

            case ChatIntent.HowToManageCategory:
                prompt.AppendLine("Kategori sayfasından direkt olarak ürün ataması yapılamaz. Ürün eklerken kategori seçilerek kategoriye ürün atanmış olunur.");
                break;

            case ChatIntent.HowToManageLocation:
                prompt.AppendLine("Lokasyonunun kendisini değiştirmek istiyorsanız Lokasyonlar sayfasından ilgili lokasyonu güncelleyebilirsiniz. Eğer ürünün lokasyonunu değiştirmek istiyorsanız Ürünler sayfasından ilgili ürün üzerinde 'Düzenle' butonuna tıklayıp açılan ekranda ürünün lokasyonunu değiştirebilirsiniz.");
                break;

            case ChatIntent.HowToAddAttribute:
                prompt.AppendLine("Ürün özniteliği ekleme adımları:");
                prompt.AppendLine("1. Öznitelikler sayfasında 'Yeni Öznitelik' butonuna tıkla.");
                prompt.AppendLine("2. Ürün seç, anahtar ve değer alanlarını doldur (örn. 'Renk' : 'Mavi').");
                prompt.AppendLine("3. 'Kaydet' diyerek özniteliği ekle; ürün detayında görüntülenir.");
                break;

            case ChatIntent.ExplainAttributePurpose:
                prompt.AppendLine("Özniteliklerin amacı ve kullanımına dair özet hazırla.");
                prompt.AppendLine("- Özniteliklerin ürünlere ek bilgi kazandırdığını belirt.");
                prompt.AppendLine("- Filtreleme, raporlama ve ürün detaylarında nasıl avantaj sağladığını açıkla.");
                prompt.AppendLine("- Gerekirse öznitelik ekleme/düzenleme adımlarına kısa değin.");
                break;

            case ChatIntent.HowToExportProductsExcel:
                prompt.AppendLine("Kullanıcıya kısa ve net olarak Excel aktarım adımını anlat.");
                prompt.AppendLine("Mesaj: Üst menüden 'Ürünler' sayfasına gidilir. 'Excel'e Aktar' butonu ile cihazınıza ürünler listesini Excel formatında indirebilirsiniz.");
                break;

            case ChatIntent.HowToViewStockMovements:
                prompt.AppendLine("Kullanıcıya stok hareketlerini Excel’e aktarma adımını kısa anlat.");
                prompt.AppendLine("Mesaj: Üst menüden 'Stok Hareketleri' sayfasına gidilir. 'Excel'e Aktar' butonu ile cihazınıza stok hareketlerini Excel formatında indirebilirsiniz.");
                break;

            case ChatIntent.GeneralAppHelp:
                prompt.AppendLine("Uygulama genel amacı:");
                prompt.AppendLine("- Stok yönetimi, fiyat takibi, raporlama ve mobil uyumlu arayüz.");
                prompt.AppendLine("- Ürünler, kategoriler, lokasyonlar, stok hareketleri ve yapılacaklar modülleri.");
                break;

            case ChatIntent.AiAssistantInfo:
                prompt.AppendLine("Asistan yetenekleri:");
                prompt.AppendLine("- Stok ve finansal verileri analiz eder, Türkçe rapor üretir.");
                prompt.AppendLine("- READMEdaki kullanım adımlarını paylaşabilir.");
                prompt.AppendLine("- Sorular tarih aralığı, ürün/kategori filtresi içerebilir.");
                break;

            case ChatIntent.HowToUseTodos:
                prompt.AppendLine("Yapılacaklar (Todo) modülü kullanımı:");
                prompt.AppendLine("1. Yapılacaklar sayfasında 'Yeni Görev' butonuyla başlık, açıklama, öncelik ve durumu belirle.");
                prompt.AppendLine("2. Görev kartından 'Düzenle' ile alanları güncelle, 'Sil' ile kaldır.");
                prompt.AppendLine("3. Görev tamamlandığında durumu 'Tamamlandı' olarak işaretle; geçmişte referans için listede kalır.");
                prompt.AppendLine("4. Filtrelerden durum/öncelik seçerek aradığın görevlere hızla ulaş.");
                break;
        }

        debugBuilder.AppendLine("Help instructions appended.");
    }

    private static bool IsHelpIntent(ChatIntent intent) =>
        intent is ChatIntent.HowToAddProduct
            or ChatIntent.HowToUpdateProduct
            or ChatIntent.HowToDeleteProduct
            or ChatIntent.HowToUseDashboard
            or ChatIntent.GeneralAppHelp
            or ChatIntent.AiAssistantInfo
            or ChatIntent.HowToManageCategory
            or ChatIntent.HowToManageLocation
            or ChatIntent.HowToAddAttribute
            or ChatIntent.ExplainAttributePurpose
            or ChatIntent.HowToManageAttribute
            or ChatIntent.HowToViewStockMovements
            or ChatIntent.HowToUseTodos
            or ChatIntent.HowToExportProductsExcel;

    private static string BuildHelpResponse(ChatIntent intent) => intent switch
    {
        ChatIntent.HowToAddProduct => """
            Ürün eklemek için:
            1. Ürünler sayfasında “Yeni Ürün Ekle” butonuna tıkla.
            2. Ürün adı, stok miktarı, kategori, alış/satış fiyatı gibi zorunlu alanları doldur.
            3. İsteğe bağlı olarak ürün için görsel ekleyebilir, düşük stok eşiği belirleyebilirsin.
            4. “Kaydet” diyerek ürünü sisteme ekle; bilgiler SignalR sayesinde anında listelere yansır.
            """,

        ChatIntent.HowToUpdateProduct => """
            Ürün güncellemek için:
            1. Ürünler tablosunda düzenlemek istediğin ürünün yanındaki “Düzenle” butonuna tıkla.
            2. Açılan formda stok, fiyat, açıklama, kategori/lokasyon gibi alanları güncelle.
            3. Yeni bir görsel seçmek istersen aynı formda yükleyip “Güncelle” butonuna bas.
            4. Kaydettikten sonra listeler ve stok istatistikleri otomatik olarak güncellenir.
            """,

        ChatIntent.HowToDeleteProduct => """
            Ürün silmek için:
            1. Ürünler tablosunda ilgili satırdaki “Sil” butonuna tıkla.
            2. Onay penceresinde “Evet, Sil” diyerek işlemi tamamla.
            3. Silinen ürünün stok hareketleri ve fiyat geçmişi de kaldırılacağı için işlemi dikkatli yap.
            """,

        ChatIntent.HowToUseDashboard => """
            Dashboard kullanımı:
            - Üstte toplam envanter değeri, beklenen satış ve potansiyel kâr kartlarını incele.
            - Kategori grafiğinde maliyet, beklenen satış ve kârı aynı anda görebilirsin.
            - Son stok hareketleri tablosu en güncel giriş/çıkışları listeler; ihtiyaç olursa Excel’e aktar.
            """,

        ChatIntent.GeneralAppHelp => """
            Stock App ile:
            - Ürün, kategori, lokasyon, stok hareketi ve fiyat yönetimini tek yerden yaparsın.
            - Dashboard gerçek zamanlı SignalR güncellemeleriyle finansal durumu gösterir.
            - Ürün detaylarında fiyat geçmişi grafikleri ve stok hareketleri yer alır.
            """,

        ChatIntent.AiAssistantInfo => """
            Stock App Asistan neler yapar?
            - Stok ve finansal verileri analiz ederek Türkçe rapor üretir.
            - Ürün ekleme, kategori yönetimi gibi uygulama içi adımları aktarır.
            - Tarih aralığı, ürün/kategori filtresi içeren soruları yorumlayıp ilgili tablo verilerini kullanır.
            """,

        ChatIntent.HowToManageCategory => """
            Kategori sayfasından direkt olarak ürün ataması yapılamaz. Ürün eklerken kategori seçilerek kategoriye ürün atanmış olunur.
            """,

        ChatIntent.HowToManageLocation => """
            Lokasyonunun kendisini değiştirmek istiyorsanız Lokasyonlar sayfasından ilgili lokasyonu güncelleyebilirsiniz. Eğer ürünün lokasyonunu değiştirmek istiyorsanız Ürünler sayfasından ilgili ürün üzerinde “Düzenle” butonuna tıklayıp açılan ekranda ürünün lokasyonunu değiştirebilirsiniz.
            """,

        ChatIntent.HowToAddAttribute => """
            Ürün özniteliği ekleme:
            1. Öznitelikler sayfasında “Yeni Öznitelik” butonuna tıkla.
            2. İlgili ürünü seç, anahtar ve değer alanlarını doldur (örn. “Renk”: “Mavi”).
            3. Kaydettiğinde öznitelik, ürün detayında ve listelerde görüntülenir.
            """,

        ChatIntent.ExplainAttributePurpose => """
            Öznitelikler ne işe yarar?
            - Öznitelikler, ürünlere renk, beden, materyal gibi ek bilgiler eklemeni sağlar.
            - Depo ekibi ve satış tarafı ürün detayında bu bilgilere hızlıca ulaşır.
            - Filtreleme ve raporlarda belirli özelliklere göre listeleme yapabilirsin (örn. “Rengi Mavi olanlar”).
            - İhtiyaç duyarsan öznitelikler sayfasından yeni özellik ekleyebilir, düzenleyebilir veya silebilirsin.
            """,

        ChatIntent.HowToManageAttribute => """
            Öznitelik düzenleme / silme:
            1. Öznitelikler sayfasında düzenlemek istediğin satırdaki “Düzenle” butonuna tıkla; anahtar veya değeri güncelle.
            2. Bir özniteliği silmek için aynı satırdaki “Sil” butonunu kullan ve onayla.
            3. Silinen öznitelik ürün detayından kaldırılır; raporlarda da görünmez.
            """,

        ChatIntent.HowToViewStockMovements => """
            Üst menüden “Stok Hareketleri” sayfasına gidilir. “Excel’e Aktar” butonu ile cihazınıza stok hareketlerini Excel formatında indirebilirsiniz.
            """,

        ChatIntent.HowToExportProductsExcel => """
            Üst menüden “Ürünler” sayfasına gidilir. “Excel’e Aktar” butonu ile cihazınıza ürünler listesini Excel formatında indirebilirsiniz.
            """,

        ChatIntent.HowToUseTodos => """
            Yapılacaklar modülü:
            1. Yapılacaklar sayfasında “Yeni Görev” butonuyla başlık, açıklama, öncelik ve durum belirle.
            2. Görev kartındaki “Düzenle” ile içerik güncellenebilir, “Sil” ile kaldırılabilir.
            3. Görev tamamlandığında durumu “Tamamlandı” yap; geçmiş kayıtlar listede kalır.
            4. Durum ve öncelik filtreleriyle aktif görevlerini hızla bulabilirsin.
            """,

        _ => "Bu konu hakkında kısa bir rehber hazırlayamadım. Başka bir soru sorabilirsin."
    };

    private static IReadOnlyList<string>? GetSuggestions(ChatIntent intent)
    {
        return intent switch
        {
            ChatIntent.InventoryValue => new[]
            {
                "Son 7 gündeki stok girişleri",
                "En yüksek kârlı kategori hangisi?"
            },
            ChatIntent.MostProfitableCategory => new[]
            {
                "Belirli bir kategori için stok özetini verir misin?",
                "En çok stok çıkışı yapılan ürün hangisi?"
            },
            ChatIntent.StockMovementSummary => new[]
            {
                "Sadece giriş hareketlerini göster",
                "Geçen ayın stok çıkış özeti"
            },
            ChatIntent.HowToAddProduct => new[]
            {
                "Ürün güncelleme nasıl yapılır?",
                "Excel'e ürünleri nasıl aktarırım?"
            },
            ChatIntent.HowToUpdateProduct => new[]
            {
                "Ürün silme adımları nelerdir?",
                "Ürün fiyatını nasıl güncellerim?"
            },
            ChatIntent.HowToDeleteProduct => new[]
            {
                "Ürünleri kalıcı olarak silmek güvenli mi?",
                "Yeni ürün ekleme adımlarını hatırlatır mısın?"
            },
            ChatIntent.HowToManageCategory => new[]
            {
                "Kategoriye ürün nasıl atanır?",
                "Kategori silince ürünlere ne olur?"
            },
            ChatIntent.HowToManageLocation => new[]
            {
                "Lokasyon değiştirme nasıl yapılır?",
                "Yeni lokasyona ürün nasıl taşınır?"
            },
            ChatIntent.HowToAddAttribute => new[]
            {
                "Öznitelik güncelleme nasıl yapılır?",
                "Öznitelik listesinde filtreleme var mı?"
            },
            ChatIntent.ExplainAttributePurpose => new[]
            {
                "Öznitelik nasıl eklenir?",
                "Hangi ürünlerde öznitelik var?"
            },
            ChatIntent.HowToManageAttribute => new[]
            {
                "Öznitelik ekleme adımları nelerdir?",
                "Öznitelikleri filtreleyebilir miyim?"
            },
            ChatIntent.HowToExportProductsExcel => new[]
            {
                "Stok hareketlerini Excel'e nasıl aktarırım?",
                "Excel çıktı dosyası neleri içeriyor?"
            },
            ChatIntent.HowToViewStockMovements => new[]
            {
                "Son 5 stok girişini gösterir misin?",
                "Stok hareketlerini Excel'e nasıl aktarırım?"
            },
            ChatIntent.HowToUseTodos => new[]
            {
                "Görevleri nasıl filtrelerim?",
                "Görevleri başkasıyla paylaşabilir miyim?"
            },
            ChatIntent.TopStockQuantityProduct => new[]
            {
                "En kârlı ürünler hangileri?",
                "Düşük stok uyarısı olan ürünler var mı?"
            },
            ChatIntent.SmallTalk => new[]
            {
                "Bu hafta stok durumum nasıldı?",
                "Ürün nasıl eklenir?"
            },
            _ => null
        };
    }

    private static IReadOnlyList<string> GetFallbackSuggestions() => new[]
    {
        "Geçen haftaki stok hareketlerini özetler misin?",
        "En kârlı kategori hangisi?",
        "Ürün ekleme adımları nelerdir?"
    };
}



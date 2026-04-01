using MediatR;
using StockApp.Services;

namespace StockApp.App.Chat.Query;

public record GetPublicChatResponseQuery(string Question) : IRequest<PublicChatResponseDto>;

public record PublicChatResponseDto(
    string Answer,
    string Intent,
    IReadOnlyList<string> Suggestions,
    bool TriggerSupportFlow = false);

internal class GetPublicChatResponseQueryHandler : IRequestHandler<GetPublicChatResponseQuery, PublicChatResponseDto>
{
    private readonly IGeminiService _geminiService;
    private readonly ILogger<GetPublicChatResponseQueryHandler> _logger;

    public GetPublicChatResponseQueryHandler(IGeminiService geminiService, ILogger<GetPublicChatResponseQueryHandler> logger)
    {
        _geminiService = geminiService;
        _logger = logger;
    }

    public async Task<PublicChatResponseDto> Handle(GetPublicChatResponseQuery request, CancellationToken cancellationToken)
    {
        var intent = await ClassifyIntentAsync(request.Question, cancellationToken);
        _logger.LogInformation("Public chat intent: {Intent} for question: {Question}", intent, request.Question);

        return intent switch
        {
            PublicChatIntent.HowToRegister => new PublicChatResponseDto(
                """
                StockApp'e kayıt olmak için sistem yöneticinizle iletişime geçmeniz gerekiyor.

                Hesabınız yönetici tarafından oluşturulduktan sonra e-posta adresinize bildirim gelecektir.
                Ardından aşağıdaki adımları takip edebilirsiniz:
                1. Ana sayfadaki "Giriş Yap" butonuna tıklayın.
                2. Size iletilen e-posta ve şifreyle giriş yapın.
                3. İlk girişte şifrenizi değiştirmeniz istenebilir.

                Daha fazla yardım için destek ekibimizle iletişime geçebilirsiniz.
                """,
                intent.ToString(),
                ["Giriş yapmada sorun mu yaşıyorsunuz?", "StockApp ile neler yapabilirim?"],
                TriggerSupportFlow: false),

            PublicChatIntent.WhatCanIDo => new PublicChatResponseDto(
                """
                StockApp ile şunları yapabilirsiniz:

                📦 Stok Yönetimi — Ürünleri ekleyin, güncelleyin, silin; alış/satış hareketlerini kaydedin.
                🔍 Akıllı Arama — Elasticsearch destekli tam metin arama ile binlerce ürün arasında anında bulun.
                📊 Dashboard — Envanter değeri, potansiyel kâr ve kategori bazlı finansal özet.
                🖼 Görsel Envanter — Ürünlere fotoğraf ekleyin, MinIO ile güvenli depolama.
                📍 Lokasyon Takibi — Hangi ürün hangi rafta, depo bazlı filtreleme.
                ✅ Görev Yönetimi — Yapılacaklar listesi ile ekip organizasyonu.
                🤖 Yapay Zekâ Asistan — Verilerinizi analiz eden, Türkçe raporlar üreten AI.
                🔔 Gerçek Zamanlı Bildirim — SignalR ile anlık stok güncellemeleri.
                """,
                intent.ToString(),
                ["Nasıl kayıt olabilirim?", "Giriş yapmada sorun mu yaşıyorsunuz?"],
                TriggerSupportFlow: false),

            PublicChatIntent.LoginIssue => new PublicChatResponseDto(
                """
                Giriş yaparken sorun yaşıyorsanız aşağıdakileri kontrol edebilirsiniz:

                1. E-posta adresinizi doğru yazdığınızdan emin olun.
                2. Şifrenizi kontrol edin — büyük/küçük harf duyarlıdır.
                3. Şifrenizi sıfırlamak için destek ekibimizle iletişime geçin.
                4. Hesabınızın aktif olup olmadığını sistem yöneticinize sorun.

                Destek almak ister misiniz?
                """,
                intent.ToString(),
                ["Destek talebi oluştur", "Nasıl kayıt olabilirim?"],
                TriggerSupportFlow: true),

            PublicChatIntent.SupportRequest => new PublicChatResponseDto(
                "Size yardımcı olmaktan memnuniyet duyarız. Destek talebinizi iletmek için e-posta adresinizi ve konu başlığını paylaşmanız yeterli.",
                intent.ToString(),
                [],
                TriggerSupportFlow: true),

            PublicChatIntent.SmallTalk => new PublicChatResponseDto(
                "Merhaba! Ben Envanter Asistanıyım. Size Envanter hakkında yardımcı olmaktan mutluluk duyarım. Ne öğrenmek istersiniz?",
                intent.ToString(),
                ["Nasıl kayıt olabilirim?", "StockApp ile neler yapabilirim?", "Giriş yapmada sorun mu yaşıyorsunuz?"],
                TriggerSupportFlow: false),

            _ => await HandleGeneralAsync(request.Question, cancellationToken)
        };
    }

    private async Task<PublicChatIntent> ClassifyIntentAsync(string question, CancellationToken cancellationToken)
    {
        var lowerQuestion = question.ToLowerInvariant();

        if (lowerQuestion.Contains("kayıt") || lowerQuestion.Contains("hesap aç") || lowerQuestion.Contains("üye"))
            return PublicChatIntent.HowToRegister;

        if (lowerQuestion.Contains("neler yapabilir") || lowerQuestion.Contains("özellik") || lowerQuestion.Contains("ne işe yarar") || lowerQuestion.Contains("ne yapıyor"))
            return PublicChatIntent.WhatCanIDo;

        if (lowerQuestion.Contains("giriş") && (lowerQuestion.Contains("sorun") || lowerQuestion.Contains("yapamıyor") || lowerQuestion.Contains("hata") || lowerQuestion.Contains("şifre")))
            return PublicChatIntent.LoginIssue;

        if (lowerQuestion.Contains("destek") || lowerQuestion.Contains("yardım") || lowerQuestion.Contains("iletişim") || lowerQuestion.Contains("şikayet"))
            return PublicChatIntent.SupportRequest;

        if (lowerQuestion.Contains("merhaba") || lowerQuestion.Contains("selam") || lowerQuestion.Contains("iyi günler") || lowerQuestion.Contains("nasılsın"))
            return PublicChatIntent.SmallTalk;

        // Gemini ile sınıflandır
        try
        {
            var classificationPrompt = $"""
                Aşağıdaki soruyu yalnızca şu intent kategorilerinden birine ata:
                - HowToRegister: kayıt olma, hesap açma
                - WhatCanIDo: uygulama özellikleri, ne yapılabilir
                - LoginIssue: giriş sorunu, şifre sorunu
                - SupportRequest: destek talebi, yardım isteme
                - SmallTalk: selamlaşma, küçük konuşma
                - General: diğer

                Sadece bir kelime yaz, açıklama yapma.
                Soru: "{question}"
                """;

            var result = await _geminiService.GenerateTextAsync(classificationPrompt, cancellationToken);
            if (result.Success && Enum.TryParse<PublicChatIntent>(result.Message.Trim().Split('\n')[0].Trim(), out var parsed))
                return parsed;
        }
        catch
        {
            // Gemini sınıflandırma başarısız → General'e düş
        }

        return PublicChatIntent.General;
    }

    private async Task<PublicChatResponseDto> HandleGeneralAsync(string question, CancellationToken cancellationToken)
    {
        var prompt = $"""
            Sen Envanter adlı stok yönetimi uygulamasının genel bilgi asistanısın.
            Giriş yapmamış bir ziyaretçiye yardımcı oluyorsun.
            Sadece StockApp uygulamasının genel özellikleri, kayıt/giriş süreçleri ve destek hakkında bilgi ver.
            Sistem içi veriler (stok, ürün, fiyat vb.) hakkında bilgi vermemelisin.
            Kısa ve anlaşılır Türkçe cevap ver.

            Kullanıcı sorusu: "{question}"
            """;

        var result = await _geminiService.GenerateTextAsync(prompt, cancellationToken);
        var answer = result.Success
            ? result.Message
            : "Bu konu hakkında size yardımcı olmak için lütfen sistemimize giriş yapın veya destek ekibimizle iletişime geçin.";

        return new PublicChatResponseDto(
            answer,
            PublicChatIntent.General.ToString(),
            ["Nasıl kayıt olabilirim?", "StockApp ile neler yapabilirim?", "Destek talebi oluştur"],
            TriggerSupportFlow: false);
    }
}

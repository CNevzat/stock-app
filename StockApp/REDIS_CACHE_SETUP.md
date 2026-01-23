# Redis Cache Setup Guide

Bu projede Redis cache + invalidation pattern kullanılmaktadır. Bu dokümantasyon, Redis cache'in nasıl çalıştığını ve nasıl kullanıldığını açıklar.

## 🎯 Amaç

Redis cache, sık sorgulanan verileri (özellikle Dashboard Stats) RAM'de tutarak:
- **Performans artışı**: Veritabanı sorgularını azaltır
- **Yanıt süresi**: Cache'den okuma çok daha hızlıdır
- **Veritabanı yükü**: Sık sorgulanan veriler için DB'ye gitmez

## 📦 Kurulum

### 1. Redis Kurulumu

#### macOS (Homebrew)
```bash
brew install redis
brew services start redis
```

#### Windows
```powershell
# Chocolatey ile
choco install redis-64

# Veya Docker ile
docker run -d -p 6379:6379 redis:latest
```

#### Linux (Ubuntu/Debian)
```bash
sudo apt-get update
sudo apt-get install redis-server
sudo systemctl start redis
```

### 2. appsettings.json Yapılandırması

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=stockapp.db",
    "Redis": "localhost:6379"
  }
}
```

**Not**: Redis connection string formatı: `host:port` veya `host:port,password=xxx`

### 3. Redis Yoksa (Development)

Redis yoksa, uygulama otomatik olarak **in-memory cache** kullanır (IDistributedCache). Bu durumda:
- Cache sadece tek sunucu instance'ında çalışır
- Sunucu restart olduğunda cache temizlenir
- Production'da mutlaka Redis kullanılmalıdır

## 🏗️ Mimari

### Cache Service (ICacheService)

```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}
```

### Cache Keys

Cache key'leri `StockApp.Common.Constants.CacheKeys` sınıfında tanımlıdır:

```csharp
public static class CacheKeys
{
    public const string DashboardStats = "dashboard:stats";
}
```

## 🔄 Cache + Invalidation Pattern

### 1. Cache Okuma (GetDashboardStatsQuery)

```csharp
public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
{
    // 1. Önce cache'den kontrol et
    var cachedStats = await _cacheService.GetAsync<DashboardStatsDto>(CacheKeys.DashboardStats, cancellationToken);
    if (cachedStats != null)
    {
        return cachedStats; // Cache'den dön, DB'ye gitme ✅
    }

    // 2. Cache'de yok, veritabanından hesapla
    var stats = await CalculateStatsFromDb();

    // 3. Cache'e yaz (60 saniye TTL)
    await _cacheService.SetAsync(CacheKeys.DashboardStats, stats, TimeSpan.FromSeconds(60), cancellationToken);

    return stats;
}
```

### 2. Cache Invalidation (Command Handlers)

Veri değiştiğinde cache'i invalidate et (sil):

```csharp
public async Task<CreateProductCommandResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
{
    // 1. Veritabanına kaydet
    await _context.SaveChangesAsync(cancellationToken);

    // 2. Cache'i invalidate et (dashboard stats değişti)
    await _cacheService.RemoveAsync(CacheKeys.DashboardStats, cancellationToken);

    // 3. Yeni stats hesapla ve SignalR ile gönder
    var dashboardStats = await _mediator.Send(new GetDashboardStatsQuery(), cancellationToken);
    await _hubContext.Clients.All.SendAsync("DashboardStatsUpdated", dashboardStats, cancellationToken);
}
```

## 📊 Akış Diyagramı

```
┌─────────────┐
│ Client A    │
│ "Stats göster"│
└──────┬──────┘
       │
       ▼
┌─────────────────┐
│ GetDashboardStats│
│ Query Handler    │
└──────┬──────────┘
       │
       ▼
┌─────────────────┐      Cache'de var mı?
│ Redis Cache     │◄─────┐
│ "dashboard:stats"│      │
└─────────────────┘      │
       │                  │
       │ Yok              │ Var ✅
       ▼                  │
┌─────────────────┐      │
│ SQLite DB       │      │
│ Stats Hesapla   │      │
└──────┬──────────┘      │
       │                  │
       ▼                  │
┌─────────────────┐      │
│ Cache'e Yaz     │      │
│ (60 saniye TTL) │      │
└──────┬──────────┘      │
       │                  │
       └──────────────────┘
              │
              ▼
       ┌─────────────┐
       │ Client A'ya │
       │ Döndür      │
       └─────────────┘

[Ürün Eklendi]
       │
       ▼
┌─────────────────┐
│ CreateProduct   │
│ Command Handler │
└──────┬──────────┘
       │
       ▼
┌─────────────────┐
│ Cache'i Sil     │
│ (Invalidate)    │
└──────┬──────────┘
       │
       ▼
┌─────────────────┐
│ Yeni Stats      │
│ Hesapla         │
└──────┬──────────┘
       │
       ▼
┌─────────────────┐
│ SignalR Push    │
│ Tüm Client'lara │
└─────────────────┘
```

## ✅ Cache Invalidation Yapılan Command'lar

Aşağıdaki command handler'lar dashboard stats'i etkilediği için cache invalidation yapıyor:

- ✅ `CreateProductCommand`
- ✅ `UpdateProductCommand`
- ✅ `DeleteProductCommand`
- ✅ `CreateCategoryCommand`
- ✅ `UpdateCategoryCommand`
- ✅ `DeleteCategoryCommand`
- ✅ `CreateProductAttributeCommand`
- ✅ `UpdateProductAttributeCommand`
- ✅ `DeleteProductAttributeCommand`
- ✅ `CreateStockMovementCommand`
- ✅ `CreateLocationCommand`
- ✅ `UpdateLocationCommand`
- ✅ `DeleteLocationCommand`

## 🎓 Öğrenilen Teknolojiler

Bu implementasyon ile öğrenilen teknolojiler:

1. **Redis**: In-memory data structure store
2. **IDistributedCache**: .NET'in distributed cache abstraction'ı
3. **Cache Invalidation Pattern**: Veri değiştiğinde cache'i temizleme
4. **Cache-Aside Pattern**: Cache'den oku, yoksa DB'den al ve cache'e yaz
5. **TTL (Time To Live)**: Cache'in otomatik expire olması

## 🚀 Kullanım Örnekleri

### Yeni Cache Key Eklemek

1. `CacheKeys` sınıfına ekle:
```csharp
public static class CacheKeys
{
    public const string DashboardStats = "dashboard:stats";
    public const string ProductList = "products:list"; // Yeni
}
```

2. Query handler'da kullan:
```csharp
var cached = await _cacheService.GetAsync<List<ProductDto>>(CacheKeys.ProductList);
if (cached != null) return cached;
// ... DB'den oku ve cache'e yaz
```

3. İlgili command handler'larda invalidate et:
```csharp
await _cacheService.RemoveAsync(CacheKeys.ProductList, cancellationToken);
```

## 📝 Notlar

- **TTL**: Dashboard stats için 60 saniye TTL kullanılıyor. Bu değer ihtiyaca göre ayarlanabilir.
- **Fallback**: Redis yoksa in-memory cache kullanılır (development için uygun).
- **Error Handling**: Cache hataları uygulamayı durdurmaz, sadece log'lanır.
- **SignalR Integration**: Cache invalidation sonrası SignalR ile client'lara yeni veri push edilir.

## 🔍 Debugging

Redis bağlantısını test etmek için:

```bash
# Redis CLI
redis-cli

# Cache'deki key'leri listele
KEYS *

# Belirli bir key'i oku
GET StockApp:dashboard:stats

# Key'i sil
DEL StockApp:dashboard:stats
```

## 📚 Kaynaklar

- [Redis Documentation](https://redis.io/docs/)
- [.NET Distributed Caching](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed)
- [Cache-Aside Pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/cache-aside)







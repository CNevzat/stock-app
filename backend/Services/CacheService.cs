using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace StockApp.Services;

// Redis
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}

public class CacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<CacheService> _logger;

    public CacheService(IDistributedCache distributedCache, ILogger<CacheService> logger)
    {
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var cachedData = await _distributedCache.GetStringAsync(key, cancellationToken);
            if (string.IsNullOrEmpty(cachedData))
            {
                _logger.LogInformation("Cache miss: {Key} (DB'den okuyacak)", key);
                return null;
            }

            _logger.LogInformation("Cache hit: {Key} (Cache'den dönüyor)", key);
            return JsonSerializer.Deserialize<T>(cachedData);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache'den veri okunurken hata oluştu. Key: {Key}", key);
            return null; // Cache hatası durumunda null dön, uygulama çalışmaya devam etsin
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var options = new DistributedCacheEntryOptions();
            
            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration.Value;
            }
            else
            {
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
            }

            var serializedData = JsonSerializer.Serialize(value);
            await _distributedCache.SetStringAsync(key, serializedData, options, cancellationToken);
            _logger.LogInformation("Cache set: {Key} (TTL: {TTL} seconds)", key, expiration?.TotalSeconds ?? 60);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache'e veri yazılırken hata oluştu. Key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _distributedCache.RemoveAsync(key, cancellationToken);
            _logger.LogInformation("Cache invalidated: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache'den veri silinirken hata oluştu. Key: {Key}", key);
        }
    }
}


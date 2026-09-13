using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheService> _logger;


    public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var cachedValue = await _cache.GetStringAsync(key);

        if (cachedValue is null)
        {
            _logger.LogInformation(
                "[Redis] CACHE MISS: {CacheKey}",
                key
            );

            return default;
        }

        _logger.LogInformation(
            "[Redis] CACHE HIT: {CacheKey}",
            key
        );

        return JsonSerializer.Deserialize<T>(cachedValue);
    }

    public async Task SetAsync<T>(
       string key,
       T value,
       TimeSpan expiration)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };

        var json = JsonSerializer.Serialize(value);

        await _cache.SetStringAsync(key, json, options);

        _logger.LogInformation(
            "[Redis] CACHE SET: {CacheKey}, TTL: {Expiration}",
            key,
            expiration
        );
    }
    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);

        _logger.LogInformation(
            "[Redis] CACHE REMOVE: {CacheKey}",
            key
        );
    }
}
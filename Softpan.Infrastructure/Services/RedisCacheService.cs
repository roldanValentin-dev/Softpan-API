
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Softpan.Application.Interfaces;

namespace Softpan.Infrastructure.Services;

public class RedisCacheService(IDistributedCache cache ) : IRedisCacheService
{

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await cache.GetAsync(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new DistributedCacheEntryOptions();
        if (expiration.HasValue)
        {
            options.SetAbsoluteExpiration(expiration.Value);
        }
        else
        {
            options.SetSlidingExpiration(TimeSpan.FromMinutes(30));
        }
        await cache.SetStringAsync(key, JsonSerializer.Serialize(value), options);
    }

    public async Task RemoveAsync(string key)
    {
        await cache.RemoveAsync(key);
    }
    public async Task RemovePatternAsync(string pattern)
    {
        if (pattern == "clientes:*")
        {
            await RemoveAsync("clientes:all");
        }
    }
}

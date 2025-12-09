using Softpan.Application.Interfaces;

namespace Softpan.Infrastructure.Services;

public class NoOpRedisCacheService : IRedisCacheService
{
    public Task<T?> GetAsync<T>(string key) => Task.FromResult<T?>(default);
    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) => Task.CompletedTask;
    public Task RemoveAsync(string key) => Task.CompletedTask;
}

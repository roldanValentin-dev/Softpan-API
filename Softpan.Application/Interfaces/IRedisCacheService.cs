

namespace Softpan.Application.Interfaces
{
    public interface IRedisCacheService
    {
        Task<T?> GetAsync<T>(string key);

        Task RemoveAsync(string key);

        Task RemovePatternAsync(string pattern);

        Task SetAsync<T> (string key, T value,TimeSpan? expiration= null);
    }
}

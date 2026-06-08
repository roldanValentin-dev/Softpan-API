using System.Collections.Concurrent;

namespace Softpan.API.Middlewares;

public class RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
{
    private static readonly ConcurrentDictionary<string, List<DateTime>> _requests = new();
    private static int _totalRequests;
    private readonly int _maxRequests = 100;
    private readonly TimeSpan _timeWindow = TimeSpan.FromMinutes(1);

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var now = DateTime.UtcNow;

        _requests.AddOrUpdate(clientIp, new List<DateTime> { now },
            (key, existing) =>
            {
                existing.RemoveAll(time => now - time > _timeWindow);
                existing.Add(now);
                return existing;
            });

        var requestCount = _requests[clientIp].Count;

        if (requestCount > _maxRequests)
        {
            logger.LogWarning("Rate limit excedido para IP: {IP} - Requests: {Count}", clientIp, requestCount);
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";
            
            var response = new
            {
                status = 429,
                message = "Demasiadas solicitudes",
                detail = "Por favor intente más tarde"
            };
            
            await context.Response.WriteAsJsonAsync(response);
            return;
        }

        // Limpieza periódica cada 500 requests totales para evitar memory leak
        var total = Interlocked.Increment(ref _totalRequests);
        if (total % 500 == 0)
        {
            var cutoff = now - _timeWindow;
            foreach (var kvp in _requests)
            {
                if (kvp.Value.All(t => now - t > _timeWindow))
                {
                    _requests.TryRemove(kvp.Key, out _);
                }
            }
        }

        await next(context);
    }
}


using System.Diagnostics;

namespace Softpan.API.Middlewares
{
    public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            logger.LogInformation($"Iniciando " +
                $"{context.Request.Method} " +
                $"{context.Request.Path} " +
                $"- Usuario: {context.User?.Identity?.Name ?? "Anonimo"} " +
                $"- IP: {context.Connection?.RemoteIpAddress?.ToString()}");
            await next(context);
            stopwatch.Stop();


            var loglevel = context.Response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
            logger.Log(loglevel, $"Finalizando " +
                $"{context.Request.Method} " +
                $"{context.Request.Path} " +
                $"- Status: {context.Response.StatusCode} " +
                $"- Tempo: {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}

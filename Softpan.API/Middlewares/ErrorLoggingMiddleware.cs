namespace Softpan.API.Middlewares;

public class ErrorLoggingMiddleware(RequestDelegate next, ILogger<ErrorLoggingMiddleware> logger, IHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error no controlado en {Method} {Path} - Usuario: {User} - IP: {IP}",
                context.Request.Method,
                context.Request.Path,
                context.User?.Identity?.Name ?? "Anónimo",
                context.Connection.RemoteIpAddress
            );

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var response = new
        {
            status = context.Response.StatusCode,
            message = "Error interno del servidor",
            detail = env.IsDevelopment() ? exception.Message : null
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}


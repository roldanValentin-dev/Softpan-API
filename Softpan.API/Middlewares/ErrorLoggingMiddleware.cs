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

        var (statusCode, message) = exception switch
        {
            Softpan.Application.Exceptions.NotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            Softpan.Application.Exceptions.BadRequestException => (StatusCodes.Status400BadRequest, exception.Message),
            Softpan.Application.Exceptions.UnauthorizedException => (StatusCodes.Status401Unauthorized, exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
        };

        context.Response.StatusCode = statusCode;

        var response = new
        {
            status = statusCode,
            message,
            detail = env.IsDevelopment() && statusCode == 500 ? exception.Message : null
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}


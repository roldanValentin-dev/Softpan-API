using Softpan.API.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Softpan.API.Middlewares;
using Softpan.Application;
using Softpan.Domain.Entities;
using Softpan.Infrastructure;
using Softpan.Infrastructure.Data;
using System.Text;


var logConfig = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
    );

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
if (env != "Production")
{
    logConfig.WriteTo.File(
        "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    );
}

Log.Logger = logConfig.CreateLogger();

// Permite a Npgsql aceptar DateTime con Kind=Unspecified como UTC
// Evita errores al recibir fechas del frontend (ej: FechaEntrega)
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

try
{
    Log.Information("Iniciando Softpan API");

    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        Args = args,
        ContentRootPath = Directory.GetCurrentDirectory(),
        WebRootPath = "wwwroot"
    });

    // Deshabilitar file watchers en producción
    if (builder.Environment.IsProduction())
    {
        builder.Configuration.Sources.Clear();
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddEnvironmentVariables();
    }

    builder.Host.UseSerilog();

    // CORS para frontend
    var corsOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>()
        ?? ["http://localhost:5173", "http://localhost:3000"];
    // También permitir el origen configurado para MercadoPago (frontend de producción)
    var mpBaseUrl = builder.Configuration["MercadoPago:BaseUrl"];
    if (!string.IsNullOrEmpty(mpBaseUrl) && !corsOrigins.Contains(mpBaseUrl))
        corsOrigins = [..corsOrigins, mpBaseUrl];

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins(corsOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
        });
    });

    // Servicios de Infrastructure y Application
    builder.Services.AddInfrastracture(builder.Configuration);
    builder.Services.AddApplication();

    // Identity
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 4;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

    // JWT Authentication
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ValidationActionFilter>();
    });
    builder.Services.AddEndpointsApiExplorer();

    // Swagger con JWT
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "Encabezado de autorización JWT usando el esquema Bearer. Ejemplo: 'Bearer {token}'",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });


    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Serilog request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} respondió {StatusCode} en {Elapsed:0.0000}ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress);
        };
    });

    // Middlewares personalizados (orden importante)
    app.UseMiddleware<ErrorLoggingMiddleware>();
    app.UseMiddleware<RateLimitingMiddleware>();

    app.UseCors("AllowFrontend");
    
    // Servir archivos estáticos (imágenes subidas)
    // Permite acceder a /images/productos/abc123.jpg desde el navegador
    app.UseStaticFiles();
    
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // Health check endpoint
    app.MapGet("/health", async (ApplicationDbContext db) =>
    {
        var canConnect = await db.Database.CanConnectAsync();
        return canConnect ? Results.Ok(new { status = "healthy" }) : Results.StatusCode(503);
    });

    // Redirects para Back URLs de Mercado Pago
    // En producción, configurar MercadoPago:BaseUrl con el dominio del frontend
    var frontendUrl = builder.Configuration["MercadoPago:BaseUrl"] ?? "http://localhost:5173";
    app.MapGet("/pago-exitoso", () => Results.Redirect($"{frontendUrl}/pago-exitoso"));
    app.MapGet("/pago-fallido", () => Results.Redirect($"{frontendUrl}/pago-fallido"));
    app.MapGet("/pago-pendiente", () => Results.Redirect($"{frontendUrl}/pago-pendiente"));

    Log.Information("Softpan API iniciada correctamente");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación falló al iniciar");
}
finally
{
    Log.CloseAndFlush();
}

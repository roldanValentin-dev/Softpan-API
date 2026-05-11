# Unidad 3: Program.cs - El Corazón de la API

## 📋 Contenido
1. [¿Qué es Program.cs?](#qué-es-programcs)
2. [Evolución: Startup.cs vs Program.cs](#evolución-startupcs-vs-programcs)
3. [Builder vs App](#builder-vs-app)
4. [Configuración de Servicios (builder.Services)](#configuración-de-servicios-builderservices)
5. [Configuración de Middlewares (app.Use...)](#configuración-de-middlewares-appuse)
6. [Orden de ejecución](#orden-de-ejecución)
7. [Program.cs completo de Softpan](#programcs-completo-de-softpan)

---

## 🎯 ¿Qué es Program.cs?

**Program.cs** es el **punto de entrada** de toda aplicación ASP.NET Core. Es donde:

✅ Se configuran los servicios (Dependency Injection)
✅ Se configuran los middlewares (Pipeline HTTP)
✅ Se inicia la aplicación

### Analogía de una Fábrica 🏭

Imagina que Program.cs es el **plano de construcción de una fábrica**:

```
Builder (Construcción)          App (Operación)
├── Contratar empleados    →    ├── Recibir materia prima
├── Comprar maquinaria     →    ├── Procesar en línea de producción
├── Instalar equipos       →    ├── Control de calidad
└── Configurar procesos    →    └── Entregar producto final
```

---

## 📜 Evolución: Startup.cs vs Program.cs

### .NET 5 y anteriores (Startup.cs)

Antes se usaban **dos archivos**:

#### Program.cs (antiguo)
```csharp
public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
```

#### Startup.cs (antiguo)
```csharp
public class Startup
{
    // Configurar servicios
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddDbContext<AppDbContext>();
    }

    // Configurar middlewares
    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseAuthorization();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}
```

### .NET 6+ (Program.cs unificado)

Ahora todo está en **un solo archivo** con **Minimal API**:

```csharp
var builder = WebApplication.CreateBuilder(args);

// ConfigureServices (antes en Startup.cs)
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>();

var app = builder.Build();

// Configure (antes en Startup.cs)
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

**Ventajas:**
✅ Menos código boilerplate
✅ Más simple y directo
✅ Más fácil de entender para principiantes
✅ Sigue siendo igual de potente

---

## 🏗️ Builder vs App

### WebApplicationBuilder (builder)

Es el **constructor** de la aplicación. Aquí se **configuran servicios**.

```csharp
var builder = WebApplication.CreateBuilder(args);

// Aquí se REGISTRAN servicios en el contenedor de DI
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddScoped<IProductoService, ProductoService>();
```

**Analogía:** Es como **contratar empleados** antes de abrir la fábrica.

### WebApplication (app)

Es la **aplicación construida**. Aquí se **configuran middlewares**.

```csharp
var app = builder.Build(); // ← Construye la aplicación

// Aquí se CONFIGURAN middlewares (pipeline HTTP)
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run(); // ← Inicia la aplicación
```

**Analogía:** Es la **línea de producción** en funcionamiento.

### Diferencia clave

```csharp
// ❌ ESTO NO FUNCIONA
var builder = WebApplication.CreateBuilder(args);
app.UseRouting(); // ← Error: 'app' no existe aún

// ✅ ESTO SÍ FUNCIONA
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers(); // ← Configurar servicios

var app = builder.Build(); // ← Construir aplicación
app.UseRouting(); // ← Ahora sí existe 'app'
```

---

## ⚙️ Configuración de Servicios (builder.Services)

Los servicios se registran en el **contenedor de Dependency Injection**.

### Servicios básicos

```csharp
var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

### Base de datos (Entity Framework)

```csharp
// PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
```

### Autenticación y Autorización

```csharp
// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });
```

### Servicios personalizados

```csharp
// Scoped (una instancia por request)
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IClienteService, ClienteService>();

// Transient (nueva instancia cada vez)
builder.Services.AddTransient<IEmailService, EmailService>();

// Singleton (una instancia para toda la aplicación)
builder.Services.AddSingleton<ICacheService, CacheService>();
```

### Métodos de extensión (Clean Architecture)

```csharp
// En lugar de registrar uno por uno:
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
// ... 20 servicios más

// Usamos métodos de extensión:
builder.Services.AddApplication();      // Registra servicios de Application
builder.Services.AddInfrastructure(builder.Configuration); // Registra servicios de Infrastructure
```

---

## 🔄 Configuración de Middlewares (app.Use...)

Los middlewares forman el **pipeline HTTP**. Cada request pasa por ellos en orden.

### ¿Qué es un Middleware?

Un middleware es un **componente** que:
1. Recibe un request HTTP
2. Hace algo con él (logging, autenticación, etc.)
3. Pasa el request al siguiente middleware
4. Recibe la response del siguiente middleware
5. Hace algo con la response
6. Devuelve la response

### Flujo de un Request

```
Request →
    [Middleware 1] →
        [Middleware 2] →
            [Middleware 3] →
                [Controller] →
            [Middleware 3] ←
        [Middleware 2] ←
    [Middleware 1] ←
← Response
```

### Middlewares comunes

```csharp
var app = builder.Build();

// 1. Manejo de excepciones (debe ser el primero)
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Errores detallados
}
else
{
    app.UseExceptionHandler("/Error"); // Errores genéricos
}

// 2. HTTPS Redirection
app.UseHttpsRedirection();

// 3. Archivos estáticos (CSS, JS, imágenes)
app.UseStaticFiles();

// 4. Routing
app.UseRouting();

// 5. CORS (debe ir antes de Authentication)
app.UseCors("AllowAll");

// 6. Authentication (debe ir antes de Authorization)
app.UseAuthentication();

// 7. Authorization
app.UseAuthorization();

// 8. Mapear controllers
app.MapControllers();

// 9. Iniciar aplicación
app.Run();
```

### Middlewares personalizados

```csharp
// Middleware inline
app.Use(async (context, next) =>
{
    Console.WriteLine($"Request: {context.Request.Path}");
    await next(); // Llamar al siguiente middleware
    Console.WriteLine($"Response: {context.Response.StatusCode}");
});

// Middleware de clase
app.UseMiddleware<ErrorLoggingMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();
```

---

## ⚡ Orden de ejecución

### ⚠️ El orden importa MUCHO

```csharp
// ❌ MAL - Authorization antes que Authentication
app.UseAuthorization();  // ← Intenta autorizar
app.UseAuthentication(); // ← Pero aún no sabe quién eres

// ✅ BIEN - Authentication antes que Authorization
app.UseAuthentication(); // ← Primero identifica quién eres
app.UseAuthorization();  // ← Luego verifica permisos
```

### Orden correcto estándar

```csharp
var app = builder.Build();

// 1. Manejo de excepciones (primero)
app.UseExceptionHandler("/Error");

// 2. HTTPS
app.UseHttpsRedirection();

// 3. Archivos estáticos
app.UseStaticFiles();

// 4. Routing
app.UseRouting();

// 5. CORS
app.UseCors("Policy");

// 6. Authentication
app.UseAuthentication();

// 7. Authorization
app.UseAuthorization();

// 8. Middlewares personalizados
app.UseMiddleware<CustomMiddleware>();

// 9. Endpoints
app.MapControllers();

// 10. Run (último)
app.Run();
```

### Regla mnemotécnica

**"ESRA-CAE"**
- **E**xception handling
- **S**tatic files
- **R**outing
- **A**uthentication
- **C**ORS
- **A**uthorization
- **E**ndpoints

---

## 💼 Program.cs completo de Softpan

Veamos el Program.cs real de Softpan con explicaciones:

```csharp
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

// ========== CONFIGURACIÓN DE SERILOG (LOGGING) ==========
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

// Solo escribir a archivo en desarrollo (no en producción)
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

try
{
    Log.Information("Iniciando Softpan API");

    // ========== CREAR BUILDER ==========
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        Args = args,
        ContentRootPath = Directory.GetCurrentDirectory(),
        WebRootPath = "wwwroot" // Carpeta para archivos estáticos
    });

    // Deshabilitar file watchers en producción (optimización)
    if (builder.Environment.IsProduction())
    {
        builder.Configuration.Sources.Clear();
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddEnvironmentVariables();
    }

    // Usar Serilog como logger
    builder.Host.UseSerilog();

    // ========== CONFIGURACIÓN DE SERVICIOS ==========

    // CORS para permitir requests desde el frontend
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins(
                "http://localhost:5173",  // Vite
                "http://localhost:3000",  // React/Next.js
                "https://softpan-frontend.vercel.app",
                "https://*.onrender.com"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
        });
    });

    // Servicios de Infrastructure y Application (Clean Architecture)
    builder.Services.AddInfrastracture(builder.Configuration);
    builder.Services.AddApplication();

    // ASP.NET Core Identity (gestión de usuarios)
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
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
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });

    // Controllers con filtro de validación
    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ValidationActionFilter>();
    });
    builder.Services.AddEndpointsApiExplorer();

    // Swagger con soporte para JWT
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

    // ========== CONSTRUIR APLICACIÓN ==========
    var app = builder.Build();

    // ========== CONFIGURACIÓN DE MIDDLEWARES ==========

    // Swagger solo en desarrollo
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Serilog request logging (registra cada request)
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
    app.UseMiddleware<ErrorLoggingMiddleware>();  // Manejo de errores
    app.UseMiddleware<RateLimitingMiddleware>();  // Límite de requests

    // CORS
    app.UseCors("AllowFrontend");
    
    // Servir archivos estáticos (imágenes subidas)
    app.UseStaticFiles();
    
    // HTTPS Redirection
    app.UseHttpsRedirection();
    
    // Authentication y Authorization
    app.UseAuthentication();
    app.UseAuthorization();
    
    // Mapear controllers
    app.MapControllers();

    Log.Information("Softpan API iniciada correctamente");
    
    // ========== INICIAR APLICACIÓN ==========
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
```

### Desglose por secciones

#### 1. Logging (Serilog)
```csharp
// Configurar Serilog ANTES de crear el builder
var logConfig = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt");

Log.Logger = logConfig.CreateLogger();
```

#### 2. Builder
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(); // Usar Serilog
```

#### 3. Servicios
```csharp
// CORS
builder.Services.AddCors(...);

// Clean Architecture
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(...);

// JWT
builder.Services.AddAuthentication(...).AddJwtBearer(...);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddSwaggerGen(...);
```

#### 4. App
```csharp
var app = builder.Build();
```

#### 5. Middlewares
```csharp
// Swagger (solo desarrollo)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Logging
app.UseSerilogRequestLogging();

// Personalizados
app.UseMiddleware<ErrorLoggingMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

// Estándar
app.UseCors("AllowFrontend");
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

#### 6. Run
```csharp
app.Run();
```

---

## 📚 Conceptos Clave para Recordar

### ✅ Program.cs
- Punto de entrada de la aplicación
- Configura servicios y middlewares
- Unifica Startup.cs y Program.cs (desde .NET 6)

### ✅ Builder vs App
- **Builder**: Configurar servicios (DI)
- **App**: Configurar middlewares (pipeline)

### ✅ Servicios
- Se registran con `builder.Services.Add...`
- Dependency Injection
- Ciclos de vida: Transient, Scoped, Singleton

### ✅ Middlewares
- Se configuran con `app.Use...`
- Forman el pipeline HTTP
- El orden es crítico

### ✅ Orden de Middlewares
1. Exception handling
2. Static files
3. Routing
4. CORS
5. Authentication
6. Authorization
7. Endpoints

---

## 🎯 Ejercicios Prácticos

### Ejercicio 1: Program.cs básico
Crea un Program.cs mínimo:
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();
```

### Ejercicio 2: Agregar Swagger
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
```

### Ejercicio 3: Agregar CORS
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ...

app.UseCors("AllowAll");
```

---

## 🔗 Recursos Adicionales

### Documentación Oficial
- [ASP.NET Core Fundamentals](https://docs.microsoft.com/aspnet/core/fundamentals/)
- [Middleware](https://docs.microsoft.com/aspnet/core/fundamentals/middleware/)
- [Dependency Injection](https://docs.microsoft.com/aspnet/core/fundamentals/dependency-injection)

---

## ✅ Checklist de Aprendizaje

- [ ] Entiendo qué es Program.cs
- [ ] Conozco la diferencia entre builder y app
- [ ] Sé configurar servicios con builder.Services
- [ ] Sé configurar middlewares con app.Use
- [ ] Entiendo el orden correcto de middlewares
- [ ] Puedo crear un Program.cs básico
- [ ] Entiendo el Program.cs de Softpan

---

## 🎓 Conclusión

Program.cs es el corazón de tu API. Aquí se configura todo: servicios, middlewares, autenticación, base de datos, etc. Entender este archivo es fundamental para dominar ASP.NET Core.

En la siguiente unidad profundizaremos en **Dependency Injection**, el patrón que hace posible toda esta configuración.

---

**Próxima unidad:** [Unidad 4: Dependency Injection - Fundamentos](./Unidad-04-Dependency-Injection-Fundamentos.md)

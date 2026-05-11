# Unidad 2: Fundamentos de .NET y C#

## 📋 Contenido
1. [¿Qué es .NET?](#qué-es-net)
2. [SDK vs Runtime](#sdk-vs-runtime)
3. [CLI de .NET](#cli-de-net)
4. [Estructura de un proyecto ASP.NET Core](#estructura-de-un-proyecto-aspnet-core)
5. [Archivos .csproj y dependencias](#archivos-csproj-y-dependencias)
6. [Crear tu primer proyecto](#crear-tu-primer-proyecto)
7. [Ejecutar y compilar](#ejecutar-y-compilar)

---

## 🎯 ¿Qué es .NET?

**.NET** es una **plataforma de desarrollo gratuita y de código abierto** creada por Microsoft para construir aplicaciones modernas.

### Historia Rápida

```
.NET Framework (2002)     → Solo Windows
.NET Core (2016)          → Multiplataforma
.NET 5, 6, 7, 8... (2020+) → Unificación (simplemente ".NET")
```

### ¿Por qué .NET 8?

✅ **Multiplataforma**: Windows, Linux, macOS
✅ **Alto rendimiento**: Uno de los frameworks más rápidos
✅ **Moderno**: Soporte para las últimas tecnologías
✅ **LTS (Long Term Support)**: Soporte por 3 años
✅ **Gratuito y Open Source**: Código abierto en GitHub
✅ **Gran ecosistema**: Miles de librerías disponibles

### Tipos de aplicaciones que puedes crear

| Tipo | Descripción | Ejemplo |
|------|-------------|---------|
| **Web APIs** | APIs REST | Softpan API |
| **Web Apps** | Aplicaciones web | Blazor, Razor Pages |
| **Desktop** | Aplicaciones de escritorio | WPF, WinForms |
| **Mobile** | Apps móviles | .NET MAUI |
| **Cloud** | Microservicios | Azure Functions |
| **IoT** | Internet de las cosas | Raspberry Pi |
| **Games** | Videojuegos | Unity |

---

## 📦 SDK vs Runtime

### .NET Runtime
Es el **motor** que ejecuta aplicaciones .NET.

**Analogía:** Es como el motor de un auto. Necesitas el motor para que el auto funcione.

**Cuándo lo necesitas:**
- Para ejecutar aplicaciones ya compiladas
- En servidores de producción
- Más ligero (menos espacio)

### .NET SDK (Software Development Kit)
Es el **conjunto de herramientas** para desarrollar aplicaciones.

**Incluye:**
- Runtime (para ejecutar)
- Compilador (para compilar código)
- CLI (línea de comandos)
- Templates (plantillas de proyectos)
- Herramientas de desarrollo

**Cuándo lo necesitas:**
- Para desarrollar aplicaciones
- Para compilar código
- En tu máquina de desarrollo

### Verificar instalación

```bash
# Ver versión del SDK
dotnet --version
# Salida: 8.0.100

# Ver todos los SDKs instalados
dotnet --list-sdks
# Salida: 8.0.100 [C:\Program Files\dotnet\sdk]

# Ver todos los Runtimes instalados
dotnet --list-runtimes
# Salida: 
# Microsoft.AspNetCore.App 8.0.0 [C:\Program Files\dotnet\shared\...]
# Microsoft.NETCore.App 8.0.0 [C:\Program Files\dotnet\shared\...]
```

### Instalación

**Windows:**
```
Descargar desde: https://dotnet.microsoft.com/download
Ejecutar instalador → Siguiente → Siguiente → Instalar
```

**Linux (Ubuntu):**
```bash
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0
```

**macOS:**
```bash
brew install dotnet-sdk
```

---

## 💻 CLI de .NET

La **CLI (Command Line Interface)** es la herramienta de línea de comandos para trabajar con .NET.

### Comandos Esenciales

#### 1. **dotnet new** - Crear proyectos

```bash
# Ver todas las plantillas disponibles
dotnet new list

# Crear una Web API
dotnet new webapi -n MiAPI

# Crear una solución
dotnet new sln -n MiSolucion

# Crear una librería de clases
dotnet new classlib -n MiLibreria

# Crear un proyecto de pruebas
dotnet new xunit -n MiAPI.Tests
```

**Plantillas más comunes:**

| Plantilla | Comando | Descripción |
|-----------|---------|-------------|
| webapi | `dotnet new webapi` | API REST |
| mvc | `dotnet new mvc` | App web MVC |
| blazor | `dotnet new blazorserver` | App Blazor |
| console | `dotnet new console` | App de consola |
| classlib | `dotnet new classlib` | Librería de clases |
| xunit | `dotnet new xunit` | Proyecto de tests |
| sln | `dotnet new sln` | Solución |

#### 2. **dotnet run** - Ejecutar

```bash
# Ejecutar el proyecto actual
dotnet run

# Ejecutar un proyecto específico
dotnet run --project ./MiAPI/MiAPI.csproj

# Ejecutar con variables de entorno
dotnet run --environment Production
```

#### 3. **dotnet build** - Compilar

```bash
# Compilar el proyecto
dotnet build

# Compilar en modo Release
dotnet build --configuration Release

# Compilar sin restaurar paquetes
dotnet build --no-restore
```

#### 4. **dotnet restore** - Restaurar dependencias

```bash
# Restaurar paquetes NuGet
dotnet restore
```

#### 5. **dotnet add/remove** - Gestionar paquetes

```bash
# Agregar paquete NuGet
dotnet add package Microsoft.EntityFrameworkCore

# Agregar con versión específica
dotnet add package Serilog --version 3.1.1

# Remover paquete
dotnet remove package Serilog

# Agregar referencia a otro proyecto
dotnet add reference ../MiLibreria/MiLibreria.csproj
```

#### 6. **dotnet watch** - Desarrollo con hot reload

```bash
# Ejecutar con recarga automática
dotnet watch run

# Cada vez que guardes un archivo, se recompila automáticamente
```

#### 7. **dotnet publish** - Publicar para producción

```bash
# Publicar aplicación
dotnet publish -c Release -o ./publish

# Publicar como ejecutable único
dotnet publish -c Release -r win-x64 --self-contained
```

#### 8. **dotnet clean** - Limpiar compilaciones

```bash
# Limpiar archivos compilados
dotnet clean
```

### Comandos de Entity Framework

```bash
# Agregar migración
dotnet ef migrations add NombreMigracion

# Aplicar migraciones
dotnet ef database update

# Revertir migración
dotnet ef database update MigracionAnterior

# Eliminar última migración
dotnet ef migrations remove

# Ver lista de migraciones
dotnet ef migrations list

# Generar script SQL
dotnet ef migrations script
```

---

## 📁 Estructura de un proyecto ASP.NET Core

### Crear proyecto de ejemplo

```bash
dotnet new webapi -n EjemploAPI
cd EjemploAPI
```

### Estructura generada

```
EjemploAPI/
│
├── Controllers/              # Controladores (endpoints)
│   └── WeatherForecastController.cs
│
├── Properties/
│   └── launchSettings.json  # Configuración de ejecución
│
├── appsettings.json         # Configuración general
├── appsettings.Development.json  # Configuración de desarrollo
├── EjemploAPI.csproj        # Archivo de proyecto
├── Program.cs               # Punto de entrada de la aplicación
└── WeatherForecast.cs       # Modelo de ejemplo
```

### Comparación con Softpan (Clean Architecture)

```
Softpan/
│
├── Softpan.API/             # Capa de presentación
│   ├── Controllers/
│   ├── Middlewares/
│   ├── Filters/
│   ├── wwwroot/
│   ├── Program.cs
│   └── appsettings.json
│
├── Softpan.Application/     # Capa de aplicación
│   ├── Services/
│   ├── DTOs/
│   ├── Interfaces/
│   ├── Validators/
│   └── DependencyInjections.cs
│
├── Softpan.Domain/          # Capa de dominio
│   ├── Entities/
│   ├── Enums/
│   └── Interfaces/
│
├── Softpan.Infrastructure/  # Capa de infraestructura
│   ├── Data/
│   ├── Repositories/
│   ├── Migrations/
│   └── DependencyInjections.cs
│
└── Softpan.Tests/           # Tests
    ├── Unit/
    └── Integration/
```

---

## 📄 Archivos .csproj y dependencias

### ¿Qué es un archivo .csproj?

Es el **archivo de proyecto** que contiene:
- Configuración del proyecto
- Referencias a paquetes NuGet
- Referencias a otros proyectos
- Configuración de compilación

### Ejemplo básico

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
  </ItemGroup>

</Project>
```

### Ejemplo de Softpan.API.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <!-- Paquetes NuGet -->
  <ItemGroup>
    <!-- Autenticación -->
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.0" />
    
    <!-- Base de datos -->
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
    
    <!-- Logging -->
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
    
    <!-- Documentación -->
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
  </ItemGroup>

  <!-- Referencias a otros proyectos -->
  <ItemGroup>
    <ProjectReference Include="..\Softpan.Application\Softpan.Application.csproj" />
    <ProjectReference Include="..\Softpan.Infrastructure\Softpan.Infrastructure.csproj" />
  </ItemGroup>

</Project>
```

### Elementos importantes

#### PropertyGroup
```xml
<PropertyGroup>
  <TargetFramework>net8.0</TargetFramework>  <!-- Versión de .NET -->
  <Nullable>enable</Nullable>                 <!-- Tipos nullables habilitados -->
  <ImplicitUsings>enable</ImplicitUsings>     <!-- Usings implícitos -->
  <RootNamespace>MiAPI</RootNamespace>        <!-- Namespace raíz -->
</PropertyGroup>
```

#### PackageReference (Paquetes NuGet)
```xml
<ItemGroup>
  <PackageReference Include="NombrePaquete" Version="1.0.0" />
</ItemGroup>
```

#### ProjectReference (Referencias a proyectos)
```xml
<ItemGroup>
  <ProjectReference Include="..\OtroProyecto\OtroProyecto.csproj" />
</ItemGroup>
```

### Gestionar paquetes

**Desde CLI:**
```bash
# Agregar paquete
dotnet add package Microsoft.EntityFrameworkCore

# Remover paquete
dotnet remove package Microsoft.EntityFrameworkCore

# Actualizar paquete
dotnet add package Microsoft.EntityFrameworkCore --version 8.0.1
```

**Desde Visual Studio:**
```
Click derecho en proyecto → Manage NuGet Packages
```

---

## 🚀 Crear tu primer proyecto

### Paso 1: Crear la API

```bash
# Crear carpeta del proyecto
mkdir MiPrimeraAPI
cd MiPrimeraAPI

# Crear proyecto Web API
dotnet new webapi -n MiPrimeraAPI

# Entrar a la carpeta
cd MiPrimeraAPI
```

### Paso 2: Explorar archivos generados

#### Program.cs (Punto de entrada)

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

**Explicación:**
1. **builder**: Configura servicios (AddControllers, AddSwagger)
2. **app**: Configura middlewares (UseSwagger, UseAuthorization)
3. **app.Run()**: Inicia la aplicación

#### WeatherForecastController.cs

```csharp
using Microsoft.AspNetCore.Mvc;

namespace MiPrimeraAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = "Sunny"
        })
        .ToArray();
    }
}
```

**Explicación:**
- `[ApiController]`: Marca la clase como controlador de API
- `[Route("[controller]")]`: Define la ruta base (/weatherforecast)
- `[HttpGet]`: Define un endpoint GET
- `IEnumerable<WeatherForecast>`: Retorna una lista

### Paso 3: Ejecutar

```bash
dotnet run
```

**Salida:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### Paso 4: Probar

**Opción 1: Navegador**
```
https://localhost:5001/weatherforecast
```

**Opción 2: Swagger**
```
https://localhost:5001/swagger
```

**Opción 3: cURL**
```bash
curl https://localhost:5001/weatherforecast
```

**Respuesta:**
```json
[
  {
    "date": "2024-05-02",
    "temperatureC": 15,
    "temperatureF": 58,
    "summary": "Sunny"
  },
  {
    "date": "2024-05-03",
    "temperatureC": 22,
    "temperatureF": 71,
    "summary": "Sunny"
  }
]
```

---

## 🔧 Ejecutar y compilar

### Modos de ejecución

#### 1. **Development (Desarrollo)**
```bash
dotnet run --environment Development
```

**Características:**
- Swagger habilitado
- Errores detallados
- Hot reload
- Logging verbose

#### 2. **Production (Producción)**
```bash
dotnet run --environment Production
```

**Características:**
- Swagger deshabilitado
- Errores genéricos
- Logging mínimo
- Optimizaciones habilitadas

### Configuraciones de compilación

#### Debug (por defecto)
```bash
dotnet build
```

**Características:**
- Símbolos de depuración incluidos
- Sin optimizaciones
- Más rápido de compilar
- Archivos más grandes

#### Release
```bash
dotnet build --configuration Release
```

**Características:**
- Sin símbolos de depuración
- Optimizaciones habilitadas
- Más lento de compilar
- Archivos más pequeños
- Mejor performance

### Publicar para producción

```bash
# Publicar en carpeta
dotnet publish -c Release -o ./publish

# Publicar como ejecutable único (Windows)
dotnet publish -c Release -r win-x64 --self-contained -o ./publish

# Publicar como ejecutable único (Linux)
dotnet publish -c Release -r linux-x64 --self-contained -o ./publish
```

### Hot Reload (Recarga automática)

```bash
# Ejecutar con hot reload
dotnet watch run

# Ahora cada vez que guardes un archivo, se recompila automáticamente
```

---

## 📚 Conceptos Clave para Recordar

### ✅ .NET
- Plataforma multiplataforma
- Gratuita y open source
- Alto rendimiento
- .NET 8 es LTS (soporte por 3 años)

### ✅ SDK vs Runtime
- **SDK**: Para desarrollar (incluye compilador y herramientas)
- **Runtime**: Para ejecutar (solo el motor)

### ✅ CLI de .NET
- `dotnet new`: Crear proyectos
- `dotnet run`: Ejecutar
- `dotnet build`: Compilar
- `dotnet add package`: Agregar paquetes

### ✅ Estructura de proyecto
- **Program.cs**: Punto de entrada
- **Controllers/**: Endpoints de la API
- **.csproj**: Configuración y dependencias
- **appsettings.json**: Configuración

### ✅ Ambientes
- **Development**: Para desarrollar
- **Production**: Para producción

---

## 🎯 Ejercicios Prácticos

### Ejercicio 1: Crear tu primera API
```bash
1. Crear proyecto: dotnet new webapi -n MiAPI
2. Ejecutar: dotnet run
3. Abrir Swagger: https://localhost:5001/swagger
4. Probar endpoint GET /weatherforecast
```

### Ejercicio 2: Agregar un paquete
```bash
1. Agregar Serilog: dotnet add package Serilog.AspNetCore
2. Ver el .csproj y verificar que se agregó
3. Restaurar: dotnet restore
```

### Ejercicio 3: Crear un nuevo controller
```csharp
// Controllers/SaludoController.cs
[ApiController]
[Route("api/[controller]")]
public class SaludoController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { mensaje = "¡Hola Mundo!" });
    }
}
```

```bash
1. Crear el archivo
2. Ejecutar: dotnet run
3. Probar: https://localhost:5001/api/saludo
```

---

## 🔗 Recursos Adicionales

### Documentación Oficial
- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core/)
- [.NET CLI Reference](https://docs.microsoft.com/dotnet/core/tools/)

### Tutoriales
- [Microsoft Learn - .NET](https://learn.microsoft.com/training/dotnet/)
- [.NET YouTube Channel](https://www.youtube.com/dotnet)

### Herramientas
- [Visual Studio](https://visualstudio.microsoft.com/)
- [Visual Studio Code](https://code.visualstudio.com/)
- [JetBrains Rider](https://www.jetbrains.com/rider/)

---

## ✅ Checklist de Aprendizaje

- [ ] Entiendo qué es .NET y sus ventajas
- [ ] Sé la diferencia entre SDK y Runtime
- [ ] Puedo usar la CLI de .NET (dotnet new, run, build)
- [ ] Entiendo la estructura de un proyecto ASP.NET Core
- [ ] Sé qué es un archivo .csproj
- [ ] Puedo agregar paquetes NuGet
- [ ] Puedo crear y ejecutar una API básica
- [ ] Entiendo la diferencia entre Development y Production

---

## 🎓 Conclusión

.NET 8 es una plataforma poderosa y moderna para crear APIs. Con la CLI de .NET puedes crear, compilar y ejecutar proyectos fácilmente. La estructura de proyectos es clara y el archivo .csproj gestiona todas las dependencias.

En la siguiente unidad profundizaremos en **Program.cs**, el corazón de toda aplicación .NET.

---

**Próxima unidad:** [Unidad 3: Program.cs - El Corazón de la API](./Unidad-03-Program-Corazon-API.md)

---

**📌 Nota:** Todos los ejemplos están basados en el proyecto real **Softpan**.

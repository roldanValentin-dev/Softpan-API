# Registro de Cambios — Softpan

## 2026-06-01 — Simplificación de requisitos de contraseña

### Cambios realizados

#### 1. Identity Password Options
**Archivo:** `Softpan.API/Program.cs` (líneas 92-96)

Se simplificaron los requisitos de contraseña de Identity:

| Opción | Antes | Después |
|--------|-------|---------|
| `RequireDigit` | `true` | `false` |
| `RequireLowercase` | `true` | `false` |
| `RequireUppercase` | `true` | `false` |
| `RequireNonAlphanumeric` | `false` | `false` (sin cambio) |
| `RequiredLength` | `6` | `4` |

#### 2. FluentValidation — AuthValidators
**Archivo:** `Softpan.Application/Validators/AuthValidators.cs`

Se actualizaron 3 validadores:

- **`LoginDtoValidator`** (línea 17): `MinimumLength(6)` → `MinimumLength(4)`
- **`RegisterDtoValidator`** (líneas 38-40): `MinimumLength(6)` → `MinimumLength(4)`, se eliminaron las reglas `.Matches` para mayúscula, minúscula y dígito
- **`RegisterClienteOnlineDtoValidator`** (líneas 69-71): Mismos cambios que RegisterDtoValidator

### Nuevos requisitos de contraseña
- Mínimo **4 caracteres**
- Sin requisitos de mayúsculas, minúsculas, números ni caracteres especiales

### Ejemplos de contraseñas válidas
| Contraseña | Válida |
|------------|--------|
| `abcd` | ✅ |
| `1234` | ✅ |
| `ABCD` | ✅ |
| `Hola` | ✅ |
| `pass` | ✅ |
| `a1B.` | ✅ |
| `123` | ❌ (3 caracteres) |

---
## 2026-06-01 — Stock inmediato + Productos en oferta

### Cambios realizados

#### 1. Domain — Entidad Producto
**Archivo:** `Softpan.Domain/Entities/Producto.cs`

Se agregaron 3 campos nuevos:

| Campo | Tipo | Default |
|-------|------|---------|
| `StockInmediato` | `bool` | `false` |
| `EnOferta` | `bool` | `false` |
| `PrecioOferta` | `decimal?` | `null` |

#### 2. Application — DTOs
**Archivo:** `Softpan.Application/DTOs/ProductoDto.cs`

Se agregaron los 3 campos a:
- `ProductoDto` (respuesta)
- `CreateProductoDto` (creación)
- `UpdateProductoDto` (actualización)

#### 3. Domain — Interface IProductoRepository
**Archivo:** `Softpan.Domain/Interfaces/IProductoRepository.cs`

Métodos nuevos:
- `GetProductosInmediatoAsync()` — productos activos con `StockInmediato = true`
- `GetProductosEnOfertaAsync()` — productos activos con `EnOferta = true` y `PrecioOferta != null`

#### 4. Application — Interface IProductoService
**Archivo:** `Softpan.Application/Interfaces/IProductoService.cs`

Métodos nuevos:
- `GetProductosInmediatoAsync()`
- `GetProductosEnOfertaAsync()`

#### 5. Application — ProductoService
**Archivo:** `Softpan.Application/Services/ProductoService.cs`

- Implementación de `GetProductosInmediatoAsync()` con cache key `productos:inmediato` (TTL 5 min)
- Implementación de `GetProductosEnOfertaAsync()` con cache key `productos:oferta` (TTL 5 min)
- Invalidación de `productos:inmediato` y `productos:oferta` en `CreateProductoAsync`, `UpdateProductoAsync` y `DeleteProductoAsync`

#### 6. Infrastructure — ProductoRepository
**Archivo:** `Softpan.Infrastructure/Repositories/ProductoRepository.cs`

- `GetProductosInmediatoAsync()`: `WHERE Activo AND StockInmediato`
- `GetProductosEnOfertaAsync()`: `WHERE Activo AND EnOferta AND PrecioOferta != null`
- Ambos con `AsNoTracking()` e `Include(Imagenes)`

#### 7. API — CatalogoController
**Archivo:** `Softpan.API/Controllers/CatalogoController.cs`

Nuevos endpoints (públicos, `[AllowAnonymous]`):
- `GET /api/catalogo/productos/inmediato`
- `GET /api/catalogo/productos/oferta`

#### 8. Migración EF Core
`AddCamposProducto` — agrega columnas `EnOferta`, `PrecioOferta` y `StockInmediato` a la tabla `Productos`.

### Nuevos endpoints

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/catalogo/productos/inmediato` | Productos con stock para retiro inmediato |
| `GET` | `/api/catalogo/productos/oferta` | Productos en oferta con precio rebajado |

### Pruebas
- Build: ✅ 0 errores, 0 advertencias
- Tests: ✅ 52/52 exitosos

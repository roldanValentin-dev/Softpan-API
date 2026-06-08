# Tareas Pendientes para Producción

**Proyecto:** Softpan API + Tienda Online
**Última actualización:** Junio 2026

---

## 🟡 Leyenda

| Color | Prioridad | Significado |
|-------|-----------|-------------|
| 🔴 | Alta | Debe hacerse antes de salir a producción |
| 🟡 | Media | Debe hacerse pronto, no bloqueante |
| 🟢 | Baja | Mejora deseable |

---

## 🔴 Alta Prioridad

### 1. Configurar ClientSecret (validación de webhooks)

**Archivo:** Backend — variable de entorno

El código ya tiene la validación HMAC-SHA256 en `MercadoPagoService.cs:183-192`, pero actualmente está desactivada porque `ClientSecret` está vacío.

**Acción:** Configurar la variable de entorno:
```
MercadoPago__ClientSecret = <secret de producción>
```

El secreto se obtiene del dashboard de MP: **Producción → Credenciales → Client Secret**.

---

### 2. Manejo de errores en CrearPreferenciaPagoAsync

**Archivo:** `Softpan.Infrastructure/Services/MercadoPagoService.cs:131`

Actualmente cuando MP devuelve error:
```csharp
throw new Exception($"Error de Mercado Pago: {responseBody}");
```

Esto devuelve un **500 Internal Server Error** con el mensaje crudo de MP al frontend.

**Acción:** Reemplazar por un error estructurado:
```csharp
if (!response.IsSuccessStatusCode)
{
    var mpError = JsonSerializer.Deserialize<MpErrorResponse>(responseBody);
    throw new BadRequestException(mpError?.Message ?? "Error al crear preferencia de pago");
}
```

---

### 3. Actualizar redirects de Back URLs

**Archivo:** `Softpan.API/Program.cs`

Los 3 endpoints de redirect actualmente apuntan a `localhost:5173`:
```csharp
app.MapGet("/pago-exitoso", () => Results.Redirect("http://localhost:5173/pago-exitoso"));
```

**Acción:** Cambiar por la URL del frontend en producción:
```csharp
app.MapGet("/pago-exitoso", () => Results.Redirect("https://tudominio.com/pago-exitoso"));
```

---

### 4. Mover URL de API a variable de entorno (frontend)

**Archivo:** `tienda-online/src/config/api.js`

Actualmente:
```js
const API_BASE_URL = 'http://localhost:7097';
```

**Acción:**
```js
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:7097';
```

Y crear `.env` en la raíz del frontend:
```
VITE_API_BASE_URL=https://api.tudominio.com
```

---

### 5. ✅ Cloudinary implementado

**Archivos:**
- `Softpan.Infrastructure/Services/CloudinaryFileStorageService.cs` (NUEVO)
- `Softpan.Infrastructure/DependencyInjections.cs` (DI actualizado con fallback automático)
- NuGet: `CloudinaryDotNet` v1.29.1

**Estado:** ✅ Implementado. Si `Cloudinary:CloudName` está configurado, las imágenes se suben a Cloudinary. Si no, usa almacenamiento local como fallback.

**Acción:** Configurar variables de entorno en el servidor:
```
Cloudinary__CloudName=<tu-cloud-name>
Cloudinary__ApiKey=<tu-api-key>
Cloudinary__ApiSecret=<tu-api-secret>
```

---

### 6. Configurar email (Gmail SMTP)

**Archivos:**
- `Softpan.Infrastructure/Services/EmailService.cs` (ya implementado con MailKit)
- `Softpan.Infrastructure/DependencyInjections.cs` (registrado)

**Estado:** ⚠️ El código está listo pero faltan las credenciales del cliente.

**Acción del cliente (pastelería):**
1. Crear cuenta Gmail (ej: `softpan.pasteleria@gmail.com`)
2. Activar verificación en dos pasos: [https://myaccount.google.com/security](https://myaccount.google.com/security)
3. Generar contraseña de aplicación: [https://myaccount.google.com/apppasswords](https://myaccount.google.com/apppasswords)
4. Pasarte el email y la contraseña generada

**Una vez tengas las credenciales, configurar en el servidor:**
```env
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USER=softpan.pasteleria@gmail.com
SMTP_PASSWORD=abcd efgh ijkl mnop
```

**Límite:** 500 emails/día - **gratis**

---

### 7. Desactivar DEBUG en servicios del frontend

**Archivos:**
- `src/services/CarritoService.js`
- `src/services/PagoService.js`
- `src/services/AdminPedidoService.js`
- `src/services/AdminPagoService.js`
- `src/context/CarritoContext.jsx`

**Acción:** En todos, cambiar:
```js
const DEBUG = true;
```
por:
```js
const DEBUG = false;
```

---

## ✅ Implementados (desde la última actualización)

| # | Tarea | Dónde |
|---|-------|-------|
| ✅ | **Health check** (`GET /health`) | `Program.cs:191` |
| ✅ | **Cloudinary** (reemplazo almacenamiento local) | `CloudinaryFileStorageService.cs` |
| ✅ | **Forgot/Reset password** (2 endpoints) | `AuthController.cs` |
| ✅ | **Costo de envío** (2 endpoints) | `AdminPagoController.cs` |
| ✅ | **Manejo de errores MP** | `MercadoPagoService.cs:131` (usa BadRequestException) |
| ✅ | **Redirects MP** | Usan `MercadoPago:BaseUrl` desde config |
| ✅ | **EmailService** (MailKit + Gmail SMTP, pendiente credenciales) | `EmailService.cs` |

---

## 🟡 Media Prioridad

### 8. Logging sin datos sensibles

**Archivo:** Backend — varios

Verificar que los logs no contengan:
- Access Tokens
- Emails de usuarios
- Payment IDs (no son sensibles pero evitan ruido)

Revisar `ErrorLoggingMiddleware.cs` y la configuración de Serilog en `Program.cs`.

---

### 9. Reconciliación de pagos pendientes

**Archivo:** Backend — nuevo endpoint

Agregar un endpoint simple para monitoreo:
```csharp
app.MapGet("/health", async (ApplicationDbContext db) =>
{
    var canConnect = await db.Database.CanConnectAsync();
    return canConnect ? Results.Ok(new { status = "healthy" }) : Results.StatusCode(503);
});
```

---

### 10. Rate limiting para webhook

**Archivo:** `Softpan.API/Middlewares/RateLimitingMiddleware.cs`

Actualmente hay 100 req/min global. MP puede enviar múltiples webhooks para un mismo pago. Considerar:
- Endpoint `/api/mercadopago/webhook` → límite más permisivo (ej: 200 req/min)
- O mantener el límite global e implementar idempotencia (ya está hecha)

---

### 11. Notificaciones email en pedidos

**Archivo:** Backend — nuevo servicio

Si el webhook de MP nunca llega (error de red, MP caído, etc.), el pedido queda en estado "Pendiente" para siempre.

**Acción:** Implementar un background job (IHostedService) que:
1. Busque pedidos con `TipoPago = MercadoPago` y `EstadoPago = Pendiente` con más de 30 minutos
2. Consulte el estado real en la API de MP
3. Actualice el pedido si corresponde

---

## 🟢 Baja Prioridad

### 12. Historial de cambios de estado (auditoría)

No hay una tabla que registre los cambios de `EstadoPago` a lo largo del tiempo. Dificulta debugging y atención al cliente.

**Propuesta:** Crear tabla `HistorialPago` con: `PedidoId`, `EstadoAnterior`, `EstadoNuevo`, `Fecha`, `Origen` (webhook/admin/sistema).

---

### 13. Notificaciones al usuario (in-app)

Cuando el pago se confirma o rechaza vía webhook, el usuario no recibe ninguna notificación.

**Propuesta:**
- Email de confirmación cuando `EstadoPago = Pagado`
- Notificación in-app (vía polling o WebSocket)

---

### 14. Idempotencia en crear-preferencia

Si el usuario hace doble click en "Pagar con Mercado Pago", se crean preferencias duplicadas.

**Propuesta:** Agregar un chequeo: si el pedido ya tiene `MercadoPagoPreferenceId` y la preferencia fue creada hace menos de N minutos, devolver la preferencia existente en vez de crear una nueva.

---

## Checklist Rápido Pre-Producción

- [ ] ClientSecret configurado (MP producción)
- [ ] Cloudinary configurado en servidor (variables de entorno)
- [ ] Email (Gmail SMTP) - crear cuenta + contraseña de aplicación
- [x] Health check endpoint
- [x] Manejo de errores MP con BadRequestException
- [x] Redirects MP usando variable de entorno
- [ ] API URL en variable de entorno (frontend)
- [ ] DEBUG = false (frontend)
- [ ] CORS actualizado con dominio de producción
- [ ] Backend deployado con HTTPS
- [ ] Webhook configurado en dashboard MP (producción)
- [ ] Tarjetas de prueba no funcionan en prod (verificado manualmente)

# Tareas Pendientes para Producción

**Proyecto:** Softpan API + Tienda Online
**Última actualización:** Mayo 2026

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

### 5. Desactivar DEBUG en servicios del frontend

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

## 🟡 Media Prioridad

### 6. Logging sin datos sensibles

**Archivo:** Backend — varios

Verificar que los logs no contengan:
- Access Tokens
- Emails de usuarios
- Payment IDs (no son sensibles pero evitan ruido)

Revisar `ErrorLoggingMiddleware.cs` y la configuración de Serilog en `Program.cs`.

---

### 7. Health check endpoint

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

### 8. Rate limiting para webhook

**Archivo:** `Softpan.API/Middlewares/RateLimitingMiddleware.cs`

Actualmente hay 100 req/min global. MP puede enviar múltiples webhooks para un mismo pago. Considerar:
- Endpoint `/api/mercadopago/webhook` → límite más permisivo (ej: 200 req/min)
- O mantener el límite global e implementar idempotencia (ya está hecha)

---

### 9. Reconciliación de pagos pendientes

**Archivo:** Backend — nuevo servicio

Si el webhook de MP nunca llega (error de red, MP caído, etc.), el pedido queda en estado "Pendiente" para siempre.

**Acción:** Implementar un background job (IHostedService) que:
1. Busque pedidos con `TipoPago = MercadoPago` y `EstadoPago = Pendiente` con más de 30 minutos
2. Consulte el estado real en la API de MP
3. Actualice el pedido si corresponde

---

## 🟢 Baja Prioridad

### 10. Historial de cambios de estado (auditoría)

No hay una tabla que registre los cambios de `EstadoPago` a lo largo del tiempo. Dificulta debugging y atención al cliente.

**Propuesta:** Crear tabla `HistorialPago` con: `PedidoId`, `EstadoAnterior`, `EstadoNuevo`, `Fecha`, `Origen` (webhook/admin/sistema).

---

### 11. Notificaciones al usuario

Cuando el pago se confirma o rechaza vía webhook, el usuario no recibe ninguna notificación.

**Propuesta:**
- Email de confirmación cuando `EstadoPago = Pagado`
- Notificación in-app (vía polling o WebSocket)

---

### 12. Idempotencia en crear-preferencia

Si el usuario hace doble click en "Pagar con Mercado Pago", se crean preferencias duplicadas.

**Propuesta:** Agregar un chequeo: si el pedido ya tiene `MercadoPagoPreferenceId` y la preferencia fue creada hace menos de N minutos, devolver la preferencia existente en vez de crear una nueva.

---

## Checklist Rápido Pre-Producción

- [ ] ClientSecret configurado
- [ ] Manejo de errores con BadRequestException
- [ ] Redirects apuntando al frontend real
- [ ] API URL en variable de entorno (frontend)
- [ ] DEBUG = false (frontend)
- [ ] CORS actualizado con dominio de producción
- [ ] Backend deployado con HTTPS
- [ ] Webhook configurado en dashboard MP (producción)
- [ ] Tarjetas de prueba no funcionan en prod (verificado manualmente)

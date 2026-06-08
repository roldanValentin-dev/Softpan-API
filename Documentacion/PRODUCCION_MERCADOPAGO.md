# Configuración de Mercado Pago para Producción

**Proyecto:** Softpan API (Backend .NET)
**Última actualización:** Mayo 2026

---

## 1. Variables de Entorno Requeridas

| Variable | Sandbox (prueba) | Producción | ¿Obligatoria? |
|----------|-----------------|------------|---------------|
| `MercadoPago__AccessToken` | `APP_USR-...sandbox...` | `APP_USR-...prod...` | ✅ Sí |
| `MercadoPago__BaseUrl` | `https://tu-ngrok.ngrok-free.dev` | `https://tudominio.com` | ✅ Sí |
| `MercadoPago__NotificationUrl` | `https://tu-ngrok.ngrok-free.dev/api/mercadopago/webhook` | `https://api.tudominio.com/api/mercadopago/webhook` | ❌ Opcional (default: `{BaseUrl}/api/mercadopago/webhook`) |
| `MercadoPago__ClientSecret` | (vacío) | `...secret...` | ❌ Opcional (sin firma HMAC webhook) |

### Dónde obtener las credenciales de producción

1. Ir a [https://mercadopago.com/developers/panel](https://mercadopago.com/developers/panel)
2. Seleccionar la aplicación **Softpan**
3. Ir a **Producción → Credenciales de producción**
4. Copiar **Access Token** y **Client Secret**

---

## 2. Dónde Configurar las Variables según el Entorno

### Opción A: Docker / VPS (docker-compose.yml)

```yaml
environment:
  - MercadoPago__AccessToken=APP_USR-xxxxxxxxxx
  - MercadoPago__BaseUrl=https://tudominio.com
  - MercadoPago__NotificationUrl=https://api.tudominio.com/api/mercadopago/webhook
  - MercadoPago__ClientSecret=xxxxxxxxxx
```

> **Importante:** En producción NO usar `launchSettings.json`. Usar variables de entorno del servidor o Docker.

### Opción B: Railway / Render

Configurar en el panel de Environment Variables con la sintaxis de doble guión bajo:

| Key | Value |
|-----|-------|
| `MercadoPago__AccessToken` | `APP_USR-xxxxxxxxxx` |
| `MercadoPago__BaseUrl` | `https://tudominio.com` |
| `MercadoPago__NotificationUrl` | `https://api.tudominio.com/api/mercadopago/webhook` |
| `MercadoPago__ClientSecret` | `xxxxxxxxxx` |

### Opción C: Variable de sistema (Windows Server)

```powershell
setx MercadoPago__AccessToken "APP_USR-xxxxxxxxxx" /M
setx MercadoPago__BaseUrl "https://tudominio.com" /M
setx MercadoPago__NotificationUrl "https://api.tudominio.com/api/mercadopago/webhook" /M
setx MercadoPago__ClientSecret "xxxxxxxxxx" /M
```

---

## 3. Configurar Webhook en el Dashboard de Mercado Pago

1. Ir a [https://mercadopago.com/developers/panel](https://mercadopago.com/developers/panel)
2. Seleccionar la aplicación **Softpan**
3. Ir a **Notificaciones → Webhooks**
4. En **URL para producción** poner:
   ```
   https://api.tudominio.com/api/mercadopago/webhook
   ```
5. Seleccionar el evento **Pagos**
6. Click en **Guardar configuración**

> Las Back URLs (success, failure, pending) se configuran desde el backend al crear la preferencia, no en el dashboard. Usan `MercadoPago:BaseUrl` como prefijo.

---

## 4. Actualizar CORS en Program.cs

Agregar el dominio de producción a la política CORS en `Softpan.API/Program.cs`:

```csharp
policy.WithOrigins(
    "http://localhost:5173",
    "http://localhost:3000",
    "https://tudominio.com",          // <-- agregar dominio de producción
    "https://softpan-frontend.vercel.app",
    "https://*.onrender.com"
)
```

---

## 5. Frontend: Configurar URL de API

En `src/config/api.js` de la tienda-online:

```js
// Antes (desarrollo):
const API_BASE_URL = 'http://localhost:7097';

// Después (producción):
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://api.tudominio.com';
```

Crear archivo `.env` en la raíz del frontend:

```
VITE_API_BASE_URL=https://api.tudominio.com
```

---

## 6. Frontend: Desactivar DEBUG

Antes de produccion, en los siguientes archivos cambiar `DEBUG = true` a `DEBUG = false`:

| Archivo | Ruta |
|---------|------|
| CarritoService.js | `src/services/CarritoService.js` |
| CarritoContext.jsx | `src/context/CarritoContext.jsx` |
| PagoService.js | `src/services/PagoService.js` |
| AdminPedidoService.js | `src/services/AdminPedidoService.js` |
| AdminPagoService.js | `src/services/AdminPagoService.js` |

---

## 7. Redirección de Back URLs

Cuando se usa ngrok, el backend tiene 3 endpoints de redirect (`GET /pago-exitoso`, `/pago-fallido`, `/pago-pendiente`) definidos en `Program.cs`. En producción, estos redirects deben apuntar al frontend real:

```csharp
app.MapGet("/pago-exitoso", () => Results.Redirect("https://tudominio.com/pago-exitoso"));
app.MapGet("/pago-fallido", () => Results.Redirect("https://tudominio.com/pago-fallido"));
app.MapGet("/pago-pendiente", () => Results.Redirect("https://tudominio.com/pago-pendiente"));
```

---

## 8. Verificación Post-Deploy

Checklist para validar que todo funciona en producción:

- [ ] `POST /api/mercadopago/crear-preferencia` → 200 OK
- [ ] `init_point` redirige a checkout de MP (producción, no sandbox)
- [ ] Pago exitoso con tarjeta real → webhook recibido → pedido pasa a "Confirmado"
- [ ] Pago rechazado → pedido queda en "Fallido"
- [ ] Webhook con firma inválida → rechazado (si configuraste `ClientSecret`)
- [ ] Redirect a `/pago-exitoso` funciona correctamente
- [ ] Frontend muestra el estado correcto del pago
- [ ] Admin puede ver y gestionar pedidos con MP

### Probar con tarjetas de producción

Las tarjetas de prueba NO funcionan en producción. Usar tarjetas reales para verificar.

---

## 9. Rollback

Si algo sale mal:

1. Volver el Access Token a sandbox (`APP_USR-...sandbox...`)
2. Cambiar BaseUrl a ngrok
3. Desactivar Webhook de producción en dashboard MP
4. Revisar logs del backend en `logs/log-.txt`

---

## 10. Resumen de Archivos a Modificar

| Archivo | Cambio | Prioridad |
|---------|--------|-----------|
| Variables de entorno del servidor | Access Token + BaseUrl + NotificationUrl + ClientSecret | 🔴 Alta |
| `Softpan.API/Program.cs` | CORS: agregar dominio producción | 🟡 Media |
| `Softpan.API/Program.cs` | Redirects: cambiar localhost por dominio real | 🟡 Media |
| `frontend/src/config/api.js` | Reemplazar localhost por URL de producción | 🔴 Alta |
| `frontend/.env` | Crear con `VITE_API_BASE_URL` | 🟡 Media |
| 5 archivos de servicio (frontend) | `DEBUG = false` | 🟢 Baja |

# Guía de Configuración para Producción — Softpan

## 📋 Pre-requisitos

| Recurso | Estado | Notas |
|---------|--------|-------|
| Cuenta en **GitHub** | ✅ | Repositorio del backend |
| Cuenta en **Render** | ❌ Pendiente | Para API + PostgreSQL + Redis |
| Cuenta en **Vercel** | ❌ Pendiente | Para frontend (repo separado) |
| Cuenta **Mercado Pago** del cliente | ❌ Pendiente | Credenciales de producción (Access Token + Client Secret) |
| Cuenta en **Cloudinary** | ✅ | `dcj2ysnu3` — API Key y Secret listos |
| Dominio frontend (Vercel) | ❌ Pendiente | Vercel asigna `*.vercel.app` automáticamente |

---

## 1. 🌐 Variables de Entorno

### 1.1 Mapeo completo

| Variable | Dónde se usa | Valor ejemplo |
|----------|-------------|---------------|
| `MP_ACCESS_TOKEN` | `.env` + `render.yaml` | `APP_USR-<token-del-cliente>` |
| `MP_CLIENT_SECRET` | `.env` + `render.yaml` | `<secret-del-cliente>` |
| `MP_BASE_URL` | `.env` + `render.yaml` | `https://<frontend>.vercel.app` |
| `MP_NOTIFICATION_URL` | `.env` + `render.yaml` | `https://<api>.onrender.com/api/mercadopago/webhook` |
| `CLOUDINARY_CLOUD_NAME` | `.env` + `render.yaml` | `dcj2ysnu3` |
| `CLOUDINARY_API_KEY` | `.env` + `render.yaml` | `492868527545858` |
| `CLOUDINARY_API_SECRET` | `.env` + `render.yaml` | `(el que muestra Cloudinary)` |
| `Jwt__Key` | Render (auto-generada) | Se genera automáticamente |
| `Jwt__Issuer` | `render.yaml` | `SoftpanAPI` |
| `Jwt__Audience` | `render.yaml` | `SoftpanFrontend` |
| `CorsOrigins__0` | `render.yaml` | `https://<frontend>.vercel.app` |
| `ConnectionStrings__DefaultConnection` | Render (auto-generada) | Desde PostgreSQL service |
| `Redis__ConnectionString` | Render (auto-generada) | Desde Redis service |

### 1.2 Archivo `.env` local (desarrollo)

```
ASPNETCORE_ENVIRONMENT=Development

# PostgreSQL (docker-compose)
DB_PASSWORD=Softpan123!

# JWT
JWT_KEY=MiClaveSuperSeguraDeAlMenos32Caracteres123
JWT_ISSUER=SoftpanAPI
JWT_AUDIENCE=SoftpanFrontend

# Mercado Pago (producción o sandbox)
MP_ACCESS_TOKEN=APP_USR-<token-del-cliente>
MP_CLIENT_SECRET=<secret-del-cliente>
MP_BASE_URL=http://localhost:5173
MP_NOTIFICATION_URL=https://<api>.onrender.com/api/mercadopago/webhook

# Cloudinary
CLOUDINARY_CLOUD_NAME=dcj2ysnu3
CLOUDINARY_API_KEY=492868527545858
CLOUDINARY_API_SECRET=<el-que-muestra-cloudinary>

# Puerto API
API_PORT=7097
```

---

## 2. 🚀 Pasos de Deployment

### Fase 1 — Implementaciones pendientes en el backend

| # | Qué | Archivos | Tiempo |
|---|-----|----------|--------|
| 1 | **Cloudinary** (reemplazar almacenamiento local) | Crear `CloudinaryFileStorageService.cs` + cambiar DI | 15 min |
| 2 | **Manejo de errores MP** (mensaje estructurado) | `MercadoPagoService.cs:131` | 5 min |
| 3 | **Health check** endpoint | Agregar `GET /health` en `Program.cs` | 5 min |

### Fase 2 — Configurar Render

#### 2.1 Agregar variables faltantes al `render.yaml`

```yaml
# Dentro de envVars del servicio softpan-api, agregar:

# Mercado Pago
- key: MercadoPago__AccessToken
  value: APP_USR-<token-del-cliente>
- key: MercadoPago__ClientSecret
  value: <secret-del-cliente>
- key: MercadoPago__BaseUrl
  value: https://<frontend>.vercel.app
- key: MercadoPago__NotificationUrl
  value: https://<api>.onrender.com/api/mercadopago/webhook

# Cloudinary
- key: Cloudinary__CloudName
  value: dcj2ysnu3
- key: Cloudinary__ApiKey
  value: 492868527545858
- key: Cloudinary__ApiSecret
  value: <secret-de-cloudinary>

# CORS
- key: CorsOrigins__0
  value: https://<frontend>.vercel.app
```

#### 2.2 Deploy en Render

```bash
# 1. Subir cambios a GitHub
git add .
git commit -m "feat: preparacion para produccion"
git push

# 2. En Render:
#    - New → Blueprint
#    - Conectar repositorio de GitHub
#    - Render lee render.yaml y crea los 3 servicios automáticamente
#    - Agregar las variables MP, Cloudinary y CORS (las del paso 2.1) en el dashboard

# 3. Render asigna dominio automático:
#    https://softpan-api.onrender.com
```

### Fase 3 — Frontend (Vercel)

El frontend está en un repositorio separado.

```bash
# 1. En el repo del frontend, configurar variable de entorno en Vercel:
#    VITE_API_BASE_URL = https://<api>.onrender.com

# 2. Desplegar (Vercel se conecta al repo y deployea automáticamente)

# 3. Vercel asigna dominio automático:
#    https://<frontend>.vercel.app
```

### Fase 4 — Mercado Pago (cliente)

| Paso | Qué hacer |
|------|-----------|
| 1 | El cliente ingresa a [https://mercadopago.com/developers/panel](https://mercadopago.com/developers/panel) |
| 2 | Selecciona su aplicación |
| 3 | Va a **Producción → Credenciales** |
| 4 | Copia **Access Token** (empieza con `APP_USR-...`) y **Client Secret** |
| 5 | Te los pasa a vos para configurar en Render |
| 6 | En **Notificaciones → Webhooks**, agrega: `https://<api>.onrender.com/api/mercadopago/webhook` |
| 7 | Selecciona evento **Pagos** y guarda |

---

## 3. ✅ Verificación Post-Deploy

### Backend

- [ ] `GET /health` → `{ "status": "healthy" }`
- [ ] `GET /api/catalogo/productos` → lista de productos activos
- [ ] `GET /api/catalogo/productos/inmediato` → productos con stock inmediato
- [ ] `GET /api/catalogo/productos/oferta` → productos en oferta

### Cloudinary

- [ ] `POST /api/productos/{id}/imagenes` (multipart) → imagen guardada
- [ ] La URL devuelta apunta a `https://res.cloudinary.com/...`
- [ ] La imagen se ve correctamente desde el frontend

### Mercado Pago

- [ ] `POST /api/mercadopago/crear-preferencia` → 200 OK
- [ ] `init_point` redirige a checkout de MP (producción, no sandbox)
- [ ] Pago exitoso con tarjeta real → webhook recibido → pedido pasa a "Confirmado"
- [ ] Pago rechazado → pedido queda en "Fallido"
- [ ] Webhook con firma inválida (sin ClientSecret) → rechazado
- [ ] Redirect a `/pago-exitoso` funciona correctamente
- [ ] Frontend muestra el estado correcto del pago
- [ ] Admin puede ver y gestionar pedidos con MP
- [ ] El dinero se acredita en la cuenta MP del cliente
- [ ] Las tarjetas de prueba NO funcionan en producción

### Frontend

- [ ] Catálogo carga productos desde la API de Render
- [ ] Imágenes se ven desde Cloudinary CDN
- [ ] Registro de usuario funciona
- [ ] Login funciona
- [ ] Agregar al carrito funciona
- [ ] Checkout funciona
- [ ] Pago con MP redirige correctamente
- [ ] Mis pedidos muestra el estado correcto

---

## 4. 🔄 Rollback

Si algo sale mal:

| Problema | Acción |
|----------|--------|
| API no arranca | Revisar logs en dashboard de Render → corregir variables de entorno |
| Pagos no funcionan | Volver Access Token a sandbox (`TEST-...`) temporalmente |
| Imágenes no se ven | Cambiar DI a `LocalFileStorageService` y usar imagen local |
| Webhook no llega | Verificar URL en dashboard MP y `MP_NOTIFICATION_URL` en Render |
| Error crítico | Hacer `git revert` del último commit y redeployar |

---

## 5. 📁 Arquitectura Final

```
Usuario
  │
  ├─ https://<frontend>.vercel.app  (React)
  │       │
  │       ▼
  ├─ https://<api>.onrender.com     (.NET API)
  │       │
  │       ├── PostgreSQL (Render)
  │       ├── Redis (Render)
  │       ├── Cloudinary (imágenes)
  │       └── Mercado Pago (pagos)
  │
  └─ Dashboard MP del cliente
          └── Webhook → https://<api>.onrender.com/api/mercadopago/webhook
```

---

## 6. 📌 Recordatorios

- **Antes del deploy**: implementar Cloudinary, manejo de errores MP y health check
- **No subir el `.env`** a GitHub (está en `.gitignore`)
- El `render.yaml` sí se sube — Render lo lee para crear los servicios automáticamente
- Las variables con `generateValue: true` (como `Jwt__Key`) se generan una sola vez al crear el servicio
- El frontend es un repo separado — no está en este repositorio

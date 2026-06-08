# Softpan — Sistema de Gestión y Ventas Online para Comercios

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-316192?style=for-the-badge&logo=postgresql)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![Clean Architecture](https://img.shields.io/badge/Clean-Architecture-green?style=for-the-badge)

</div>

---

> **⚠️ Este proyecto será renombrado próximamente.** El nombre "Softpan" es temporal.

---

## ¿Qué es?

API backend en .NET 8 que permite a pequeños comercios tener su tienda online con:

- Catálogo público de productos con fotos, precios y categorías
- Carrito de compras y checkout con Mercado Pago
- Gestión de pedidos con seguimiento de estado
- Productos en oferta y retiro inmediato
- Costo de envío configurable (tarifa fija + mínimo gratis)
- Notificaciones por email al cliente
- Recuperación de contraseña olvidada

---

## Evolución del Proyecto

Esto no se construyó de un día para el otro. Arrancó como un sistema diferente
y creció según necesidades reales.

### Fase 1 — POS y Gestión Interna (original)

El sistema comenzó como un punto de venta para una pastelería con venta mayorista.
De esa etapa quedan funcionalidades como:

- Clientes mayoristas con precios personalizados
- Ventas B2B con control de deudas y pagos parciales
- Registro de pagos aplicados a múltiples ventas
- Productos con stock y alertas
- Reportes de ventas

### Fase 2 — Tienda Online

Después se agregó la tienda online para que el mismo comercio pudiera
vender también al público general. Esta es la parte activa hoy:

| Funcionalidad | Descripción |
|---------------|-------------|
| Catálogo público | Productos con fotos, precios, categorías, búsqueda |
| Retiro inmediato | Productos listos para llevar hoy |
| Ofertas | Productos con precio rebajado |
| Carrito de compras | Persistente por cliente |
| Checkout | Con opción de retiro o envío |
| Costo de envío | Tarifa fija configurable + mínimo gratis |
| Mercado Pago | Checkout Pro + webhooks con validación HMAC |
| Pedidos online | Creación, estados, cancelación, historial |
| Autenticación | Login, registro, recuperación de contraseña |
| Notificaciones | Email al cliente (MailKit + Gmail SMTP) |

### Fase 3 — Unificación (hoy)

Hoy el sistema expone ambas caras según el rol del usuario:
el cliente online ve solo la tienda; el admin ve productos,
pedidos y configuración. La API es una sola con Clean Architecture.

---

## Arquitectura

```
┌─────────────────────────────────────────────────────────┐
│                    Softpan.API                          │
│         Controllers • Middlewares • Filters             │
│              (Capa de Presentación)                     │
└────────────────────────┬────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────┐
│                Softpan.Application                      │
│      Services • DTOs • Validators • Interfaces          │
│               (Casos de Uso)                            │
└────────────────────────┬────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────┐
│              Softpan.Infrastructure                     │
│    Repositories • DbContext • Migrations • Cache        │
│            (Acceso a Datos)                             │
└────────────────────────┬────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────┐
│                  Softpan.Domain                         │
│         Entities • Enums • Business Logic               │
│              (Lógica de Negocio)                        │
└─────────────────────────────────────────────────────────┘
```

---

## Stack Tecnológico

| Capa | Tecnología |
|------|-----------|
| Framework | .NET 8 (LTS) |
| ORM | Entity Framework Core 8 |
| Base de datos | PostgreSQL 16 |
| Autenticación | ASP.NET Core Identity + JWT |
| Pagos | Mercado Pago (Checkout Pro + Webhooks HMAC) |
| Imágenes | Cloudinary CDN |
| Email | MailKit + Gmail SMTP |
| Validación | FluentValidation |
| Mapeo | Mapster |
| Logging | Serilog |
| Contenedores | Docker + Docker Compose |
| Caché | Redis (opcional) |

---

## Características

### Tienda Online

- Catálogo público con búsqueda y filtros por categoría
- Productos marcados como retiro inmediato o en oferta
- Múltiples imágenes por producto vía Cloudinary
- Carrito de compras persistente por cliente
- Checkout con selección de retiro local o envío a domicilio
- Costo de envío configurable desde el panel admin

### Pedidos

- 6 estados de pedido (Pendiente → Entregado)
- 3 formas de pago (Efectivo, Transferencia, Mercado Pago)
- Seguimiento de estado de pago
- Cancelación de pedidos por el cliente
- Historial completo de compras

### Mercado Pago

- Checkout Pro con redirección a MP
- Validación HMAC-SHA256 de webhooks
- Back URLs configurables

### Seguridad

- JWT con refresh tokens y expiración
- Roles (Admin, Vendedor, Cliente)
- Rate limiting por endpoint
- Auditoría de acciones críticas
- Validación de entrada con FluentValidation

---

## Endpoints

### Públicos

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/health` | Health check de la API |
| GET | `/api/catalogo/productos` | Listar productos activos |
| GET | `/api/catalogo/productos/inmediato` | Productos retiro inmediato |
| GET | `/api/catalogo/productos/oferta` | Productos en oferta |
| GET | `/api/catalogo/productos/buscar?q=` | Buscar productos |
| GET | `/api/catalogo/productos/categoria/{cat}` | Filtrar por categoría |
| GET | `/api/catalogo/categorias` | Listar categorías |

### Autenticación

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/auth/login` | Iniciar sesión |
| POST | `/api/auth/register` | Registrar empleado |
| POST | `/api/auth/register-cliente` | Registrar cliente online |
| POST | `/api/auth/refresh` | Renovar token |
| POST | `/api/auth/revoke` | Revocar token |
| POST | `/api/auth/forgot-password` | Solicitar reset de contraseña |
| POST | `/api/auth/reset-password` | Restablecer contraseña |

### Carrito

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/carrito` | Ver carrito actual |
| POST | `/api/carrito/items` | Agregar producto |
| PUT | `/api/carrito/items/{id}` | Actualizar cantidad |
| DELETE | `/api/carrito/items/{id}` | Eliminar item |
| DELETE | `/api/carrito` | Vaciar carrito |
| POST | `/api/carrito/checkout` | Procesar checkout |

### Pedidos

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/pedidos` | Crear pedido directo |
| GET | `/api/pedidos/mis-pedidos` | Listar mis pedidos |
| GET | `/api/pedidos/{id}` | Detalle del pedido |
| PUT | `/api/pedidos/{id}/cancelar` | Cancelar pedido |
| POST | `/api/pedidos/{id}/procesar-pago` | Confirmar pago manual |

### Mercado Pago

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/mercadopago/crear-preferencia` | Crear preferencia de pago |
| POST | `/api/mercadopago/webhook` | Webhook MP |

### Admin

| Método | Ruta | Descripción |
|--------|------|-------------|
| CRUD | `/api/productos` | Gestión de productos |
| GET/PUT | `/api/admin/envio/config` | Configurar costo de envío |
| GET/PUT | `/api/admin/configuracion/descuento` | Configurar descuento |
| CRUD | `/api/admin/datos-bancarios` | Cuentas bancarias |
| CRUD | `/api/admin/direccion-retiro` | Dirección de retiro |
| GET/POST | `/api/admin/pedidos/pendientes-pago` | Pedidos pendientes |

---

## Instalación

```bash
git clone https://github.com/roldanValentin-dev/Softpan-API.git
cd Softpan-API
docker-compose up -d
```

API: `http://localhost:7097`  
Swagger: `http://localhost:7097/swagger`

---

## Próximos Pasos

- Renombre del proyecto (nombre definitivo)
- Multi-tenant (un servidor para múltiples comercios)
- Landing page por comercio con dominio, logo y colores propios

## Documentación

- `Documentacion/DOCUMENTACION_FRONTEND.md` — Referencia de endpoints para el frontend
- `Config_Produccion.md` — Guía paso a paso para deploy en producción
- `Documentacion/CHANGELOG.md` — Historial de cambios del proyecto

---

> *"Hazlo funcionar, hazlo bien, hazlo rápido."*  
> — Kent Beck

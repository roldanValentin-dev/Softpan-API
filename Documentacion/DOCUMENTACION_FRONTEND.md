# Documentación API - Frontend (Tienda Online)

**Base URL:** `http://localhost:7097/api`
**Formato:** JSON

---

## Índice

- [Health Check](#health-check)
- [Catálogo (Público)](#catálogo-público)
- [Auth](#auth)
- [Carrito](#carrito)
- [Pedidos](#pedidos)
- [Mercado Pago](#mercado-pago)
- [Clientes Online](#clientes-online)
- [Admin](#admin)
- [Auditoría](#auditoría)
- [Redirects MP](#redirects-mp)

---

## Health Check

### `GET /health`

Endpoint de health check. No requiere autenticación.

**Request:** Sin body.

**Response 200:**
```json
{
  "status": "healthy"
}
```

**Response 503:** Sin conexión a base de datos.

---

## Catálogo (Público)

Todos los endpoints de catálogo son públicos (`[AllowAnonymous]`).

### `GET /api/catalogo/productos`

Lista todos los productos activos.

**Request:** Sin body.

**Response 200:**
```json
[
  {
    "id": 0,
    "nombre": "string",
    "descripcion": "string | null",
    "precioBase": 0,
    "categoria": "string | null",
    "imagenUrl": "string | null",
    "stock": 0,
    "stockMinimo": 0,
    "activo": true,
    "fechaCreacion": "2026-01-01T00:00:00",
    "fechaModificacion": "2026-01-01T00:00:00 | null",
    "stockInmediato": false,
    "enOferta": false,
    "precioOferta": null,
    "imagenes": [
      {
        "id": 0,
        "productoId": 0,
        "url": "string",
        "orden": 0,
        "esPrincipal": false
      }
    ]
  }
]
```

---

### `GET /api/catalogo/productos/{id}`

Detalle de un producto activo.

**Request:** Sin body.

**Response 200:** Mismo DTO que `GET /productos`.
**Response 404:** Producto no encontrado.

---

### `GET /api/catalogo/productos/categoria/{categoria}`

Filtra productos activos por categoría.

**Request:** Sin body.

**Response 200:** Array de `ProductoDto`.

---

### `GET /api/catalogo/productos/buscar?q={texto}`

Busca productos por nombre o descripción.

**Query params:**
| Parámetro | Tipo | Obligatorio | Descripción |
|-----------|------|-------------|-------------|
| q | string | sí | Término de búsqueda |

**Response 200:** Array de `ProductoDto`.

---

### `GET /api/catalogo/productos/inmediato`

Productos con stock disponible para retiro inmediato.

**Request:** Sin body.

**Response 200:**
```json
[
  {
    "id": 0,
    "nombre": "string",
    "descripcion": "string | null",
    "precioBase": 0,
    "categoria": "string | null",
    "imagenUrl": "string | null",
    "stock": 0,
    "stockMinimo": 0,
    "activo": true,
    "fechaCreacion": "2026-01-01T00:00:00",
    "fechaModificacion": "2026-01-01T00:00:00 | null",
    "stockInmediato": true,
    "enOferta": false,
    "precioOferta": null,
    "imagenes": [
      {
        "id": 0,
        "productoId": 0,
        "url": "string",
        "orden": 0,
        "esPrincipal": false
      }
    ]
  }
]
```

**Filtro backend:** `Activo == true && StockInmediato == true`. Cache 5 min.

---

### `GET /api/catalogo/productos/oferta`

Productos con precio rebajado.

**Request:** Sin body.

**Response 200:**
```json
[
  {
    "id": 0,
    "nombre": "string",
    "descripcion": "string | null",
    "precioBase": 0,
    "categoria": "string | null",
    "imagenUrl": "string | null",
    "stock": 0,
    "stockMinimo": 0,
    "activo": true,
    "fechaCreacion": "2026-01-01T00:00:00",
    "fechaModificacion": "2026-01-01T00:00:00 | null",
    "stockInmediato": false,
    "enOferta": true,
    "precioOferta": 0,
    "imagenes": [
      {
        "id": 0,
        "productoId": 0,
        "url": "string",
        "orden": 0,
        "esPrincipal": false
      }
    ]
  }
]
```

**Filtro backend:** `Activo == true && EnOferta == true && PrecioOferta != null`. Cache 5 min.

**Frontend:** Mostrar `precioBase` tachado y `precioOferta` como precio vigente.

---

### `GET /api/catalogo/categorias`

Lista las categorías disponibles.

**Request:** Sin body.

**Response 200:**
```json
["Tortas", "Facturas", "Pan", "Postres"]
```

---

## Auth

### `POST /api/auth/login`

Inicio de sesión.

**Headers:** `Content-Type: application/json`

**Request:**
```json
{
  "email": "string",
  "password": "string"
}
```

**Response 200:**
```json
{
  "token": "string",
  "refreshToken": "string",
  "email": "string",
  "firstName": "string",
  "lastName": "string",
  "roles": ["string"],
  "expiresAt": "2026-01-01T00:00:00"
}
```

| Campo | Tipo | Descripción |
|-------|------|-------------|
| token | string | JWT Bearer token (expira 1 hora) |
| refreshToken | string | Token para refrescar (expira 30 días) |
| email | string | Email del usuario |
| firstName | string | Nombre |
| lastName | string | Apellido |
| roles | string[] | Roles del usuario (Admin, Vendedor, Cliente) |
| expiresAt | datetime | Fecha de expiración del token |

**Response 401:**
```json
{
  "message": "Email o contraseña incorrectos"
}
```

---

### `POST /api/auth/register`

Registro de empleado (Admin/Vendedor).

**Headers:** `Content-Type: application/json`

**Request:**
```json
{
  "email": "string",
  "password": "string",
  "firstName": "string",
  "lastName": "string"
}
```

**Response 200:** Mismo DTO que login.
**Response 400:** Si el usuario ya existe.

---

### `POST /api/auth/register-cliente`

Registro de cliente online.

**Headers:** `Content-Type: application/json`

**Request:**
```json
{
  "email": "string",
  "password": "string",
  "nombre": "string",
  "apellido": "string",
  "telefono": "string | null",
  "direccion": "string | null"
}
```

**Response 200:** Mismo DTO que login.
**Response 400:** Si el usuario ya existe.

---

### `POST /api/auth/refresh`

Refresca el token JWT usando el refresh token.

**Headers:** `Content-Type: application/json`

**Request:**
```json
{
  "token": "string",
  "refreshToken": "string"
}
```

**Response 200:** Mismo DTO que login.
**Response 401:** Token inválido o expirado.

---

### `POST /api/auth/revoke`

Revoca el refresh token del usuario autenticado.

**Headers:** `Authorization: Bearer {token}`

**Request:** Sin body.

**Response 200:**
```json
{
  "message": "Token revocado exitosamente"
}
```

**Response 400:** Error al revocar.

---

### `POST /api/auth/forgot-password`

Solicita un email con link para restablecer contraseña.

**Headers:** `Content-Type: application/json`

**Request:**
```json
{
  "email": "string"
}
```

**Response 200:**
```json
{
  "message": "Si el email existe, recibirás un enlace de recuperación"
}
```

**Nota:** No revela si el email existe por seguridad. El link enviado es `{baseUrl}/reset-password?email=...&token=...`.

---

### `POST /api/auth/reset-password`

Restablece la contraseña con el token recibido por email.

**Headers:** `Content-Type: application/json`

**Request:**
```json
{
  "email": "string",
  "token": "string",
  "newPassword": "string"
}
```

**Response 200:**
```json
{
  "message": "Contraseña actualizada exitosamente"
}
```

**Response 400:**
```json
{
  "message": "Solicitud inválida"
}
```

**Errores posibles:** Usuario no encontrado, token expirado/inválido, contraseña no cumple requisitos.

---

## Carrito

Requiere autenticación de Cliente (`Authorization: Bearer {token}`).

### `GET /api/carrito`

Obtiene el carrito actual del cliente autenticado.

**Headers:** `Authorization: Bearer {token}`

**Request:** Sin body.

**Response 200:**
```json
{
  "id": 0,
  "clienteNombre": "string",
  "items": [
    {
      "productoId": 0,
      "productoNombre": "string",
      "imagenUrl": "string | null",
      "precioUnitario": 0,
      "cantidad": 0,
      "subtotal": 0
    }
  ],
  "total": 0,
  "cantidadItems": 0
}
```

---

### `POST /api/carrito/items`

Agrega un producto al carrito.

**Headers:** `Authorization: Bearer {token}`, `Content-Type: application/json`

**Request:**
```json
{
  "productoId": 0,
  "cantidad": 1
}
```

**Response 200:** `CarritoDto` actualizado.
**Response 400:** Producto inactivo o stock insuficiente.

---

### `PUT /api/carrito/items/{productoId}`

Actualiza la cantidad de un item en el carrito.

**Headers:** `Authorization: Bearer {token}`, `Content-Type: application/json`

**Request:**
```json
{
  "cantidad": 3
}
```

**Response 200:** `CarritoDto` actualizado.
**Response 400:** Stock insuficiente.

---

### `DELETE /api/carrito/items/{productoId}`

Elimina un item del carrito.

**Headers:** `Authorization: Bearer {token}`

**Request:** Sin body.

**Response 200:** `CarritoDto` actualizado.

---

### `DELETE /api/carrito`

Limpia todo el carrito.

**Headers:** `Authorization: Bearer {token}`

**Request:** Sin body.

**Response 200:** `CarritoDto` vacío.

---

### `POST /api/carrito/checkout`

Procesa el checkout desde el carrito.

**Headers:** `Authorization: Bearer {token}`, `Content-Type: application/json`

**Request:**
```json
{
  "tipoPago": 1,
  "esRetiroLocal": true,
  "direccionEntrega": "string | null"
}
```

| Campo | Tipo | Descripción |
|-------|------|-------------|
| tipoPago | number | 1=Efectivo, 2=Transferencia, 3=MercadoPago |
| esRetiroLocal | boolean | true=retiro en local, false=envío |
| direccionEntrega | string \| null | Requerida si `esRetiroLocal = false` |

**Response 200:**
```json
{
  "pedidoId": 0,
  "total": 0,
  "initPoint": "string | null",
  "message": "string"
}
```

**Nota:** Si `tipoPago = 3` (MercadoPago), `initPoint` contiene la URL de redirección a MP.

---

## Pedidos

Requiere autenticación.

### `POST /api/pedidos`

Crea un pedido directamente (sin carrito).

**Headers:** `Authorization: Bearer {token}`, `Content-Type: application/json`

**Request:**
```json
{
  "detalles": [
    {
      "productoId": 0,
      "cantidad": 1
    }
  ],
  "tipoPago": 1,
  "esRetiroLocal": true,
  "direccionEntrega": "string | null"
}
```

**Response 200:**
```json
{
  "id": 0,
  "clienteNombre": "string",
  "fechaPedido": "2026-01-01T00:00:00",
  "total": 0,
  "costoEnvio": 0,
  "estado": "string",
  "tipoPago": "string",
  "estadoPago": "string",
  "detalles": [
    {
      "productoId": 0,
      "productoNombre": "string",
      "cantidad": 0,
      "precioUnitario": 0,
      "subtotal": 0
    }
  ]
}
```

| Campo | Descripción |
|-------|-------------|
| estado | Pendiente, Confirmado, EnPreparacion, Listo, Entregado, Cancelado |
| estadoPago | Pendiente, Pagado, Fallido, Reembolsado |
| tipoPago | Efectivo, Transferencia, MercadoPago |

---

### `GET /api/pedidos/mis-pedidos`

Lista los pedidos del cliente autenticado.

**Headers:** `Authorization: Bearer {token}`

**Request:** Sin body.

**Response 200:** Array de `PedidoDto`.

---

### `GET /api/pedidos/{id}`

Detalle de un pedido del cliente autenticado.

**Headers:** `Authorization: Bearer {token}`

**Request:** Sin body.

**Response 200:** `PedidoDto`.
**Response 404:** Pedido no encontrado.

---

### `PUT /api/pedidos/{id}/cancelar`

Cancela un pedido (solo si está Pendiente).

**Headers:** `Authorization: Bearer {token}`

**Request:** Sin body.

**Response 200:**
```json
{
  "message": "Pedido cancelado exitosamente"
}
```

---

### `POST /api/pedidos/{id}/procesar-pago`

Marca el pedido como pagado por el cliente (para Efectivo/Transferencia).

**Headers:** `Authorization: Bearer {token}`

**Request:** Sin body.

**Response 200:** `PedidoDto` actualizado.

---

### `GET /api/pedidos/{id}/datos-pago`

Obtiene los datos de pago para un pedido (datos bancarios si es transferencia).

**Headers:** `Authorization: Bearer {token}`

**Request:** Sin body.

**Response 200:**
```json
{
  "tipoPago": "string",
  "datosBancarios": { ... } | null,
  "montoConDescuento": 0 | null,
  "initPoint": "string | null"
}
```

---

## Mercado Pago

### `POST /api/mercadopago/crear-preferencia`

Crea una preferencia de pago en Mercado Pago.

**Headers:** `Authorization: Bearer {token}`, `Content-Type: application/json`

**Request:**
```json
{
  "pedidoId": 0,
  "emailPagador": "string"
}
```

**Response 200:**
```json
{
  "preferenceId": "string",
  "initPoint": "string",
  "sandboxInitPoint": "string | null"
}
```

**Frontend:** Redirigir al usuario a `initPoint`.

---

### `POST /api/mercadopago/webhook`

Endpoint para webhooks de Mercado Pago. Público (sin auth).

**Headers:** `Content-Type: application/json`, `X-Signature` (para validación HMAC)

**Request:** Payload raw de Mercado Pago.

**Response 200:**
```json
{
  "message": "Webhook procesado"
}
```

---

## Clientes Online

Requiere autenticación de Cliente.

### `GET /api/clientesonline/perfil`

Obtiene el perfil del cliente autenticado.

**Headers:** `Authorization: Bearer {token}`

**Request:** Sin body.

**Response 200:**
```json
{
  "id": 0,
  "nombre": "string",
  "apellido": "string",
  "email": "string",
  "telefono": "string | null",
  "direccion": "string | null"
}
```

---

### `PUT /api/clientesonline/perfil`

Actualiza el perfil del cliente autenticado.

**Headers:** `Authorization: Bearer {token}`, `Content-Type: application/json`

**Request:**
```json
{
  "telefono": "string | null",
  "direccion": "string | null"
}
```

**Response 200:** `ClienteOnlineDto` actualizado.

---

## Admin

Todos los endpoints de admin requieren `Authorization: Bearer {token}` con rol **Admin**.

### `GET /api/admin/configuracion/descuento`

Obtiene el porcentaje de descuento para Efectivo/Transferencia.

**Response 200:**
```json
{
  "id": 0,
  "clave": "DescuentoEfectivoTransferencia",
  "valor": "10",
  "descripcion": "Descuento para Efectivo/Transferencia (%)"
}
```

---

### `PUT /api/admin/configuracion/descuento`

Actualiza el porcentaje de descuento.

**Request:**
```json
{
  "valor": "15"
}
```

**Response 200:** `ConfiguracionPagoDto`.

---

### `GET /api/admin/datos-bancarios`

Lista las cuentas bancarias configuradas.

**Response 200:**
```json
[
  {
    "id": 0,
    "banco": "string",
    "titular": "string",
    "tipoCuenta": "string",
    "numeroCuenta": "string",
    "cvu": "string | null",
    "alias": "string | null",
    "activo": true
  }
]
```

---

### `POST /api/admin/datos-bancarios`

Crea una cuenta bancaria.

**Request:**
```json
{
  "banco": "string",
  "titular": "string",
  "tipoCuenta": "string",
  "numeroCuenta": "string",
  "cvu": "string | null",
  "alias": "string | null"
}
```

**Response 201:** `DatosBancariosDto`.

---

### `PUT /api/admin/datos-bancarios/{id}`

Actualiza una cuenta bancaria.

**Request:** Mismo DTO que create + `activo`.

**Response 200:** `DatosBancariosDto`.

---

### `DELETE /api/admin/datos-bancarios/{id}`

Elimina una cuenta bancaria.

**Response 204:** Sin contenido.

---

### `GET /api/admin/direccion-retiro`

Obtiene la dirección de retiro configurada. Público.

**Response 200:**
```json
{
  "id": 0,
  "direccion": "string",
  "horarioInicio": "string | null",
  "horarioFin": "string | null",
  "telefono": "string | null"
}
```

---

### `PUT /api/admin/direccion-retiro`

Actualiza la dirección de retiro.

**Request:**
```json
{
  "direccion": "string",
  "horarioInicio": "string | null",
  "horarioFin": "string | null",
  "telefono": "string | null"
}
```

**Response 200:** `DireccionRetiroDto`.

---

### `GET /api/admin/envio/config`

Obtiene configuración de costo de envío.

**Response 200:**
```json
{
  "costoEnvio": 0,
  "minimoGratis": null
}
```

| Campo | Tipo | Descripción |
|-------|------|-------------|
| costoEnvio | number | Tarifa fija de envío |
| minimoGratis | number \| null | Monto mínimo para envío gratis |

---

### `PUT /api/admin/envio/config`

Actualiza configuración de costo de envío.

**Request:**
```json
{
  "costoEnvio": 500.00,
  "minimoGratis": 2000.00
}
```

| Campo | Tipo | Obligatorio | Descripción |
|-------|------|-------------|-------------|
| costoEnvio | number | sí | Tarifa de envío (≥ 0) |
| minimoGratis | number \| null | no | Monto mínimo para envío gratis (≥ 0) |

**Response 200:** Mismo DTO del request.
**Response 400:** Si algún valor es negativo.

---

### `GET /api/admin/pedidos/pendientes-pago`

Lista pedidos pendientes de confirmación de pago (con estado Pagado).

**Response 200:**
```json
[
  {
    "id": 0,
    "clienteNombre": "string",
    "total": 0,
    "montoConDescuento": 0 | null,
    "tipoPago": "string",
    "referenciaTransaccion": "string | null",
    "fechaPago": "2026-01-01T00:00:00",
    "fechaPedido": "2026-01-01T00:00:00"
  }
]
```

---

### `POST /api/admin/pedidos/{id}/confirmar-pago`

Confirma el pago de un pedido y descuenta stock.

**Response 200:** `PedidoDto` actualizado.
**Response 400:** Si ya fue confirmado o no está pendiente.

---

## Auditoría

Requiere autenticación.

### `GET /api/audit`

Lista todos los logs de auditoría.

**Headers:** `Authorization: Bearer {token}`

**Response 200:**
```json
[
  {
    "id": 0,
    "userId": "string",
    "userEmail": "string",
    "accion": "string",
    "entidad": "string",
    "entidadId": "string | null",
    "detalle": "string",
    "ipAddress": "string",
    "fecha": "2026-01-01T00:00:00"
  }
]
```

---

### `GET /api/audit/user/{userId}`

Filtra logs por usuario.

---

### `GET /api/audit/entity/{entity}`

Filtra logs por entidad (ej: "Auth", "Producto", "Pedido").

---

## Redirects MP (GET sin prefijo /api)

Endpoints para las Back URLs de Mercado Pago. Redirigen al frontend configurado en `MercadoPago:BaseUrl`.

| Endpoint | Redirige a |
|----------|-----------|
| `GET /pago-exitoso` | `{frontendUrl}/pago-exitoso` |
| `GET /pago-fallido` | `{frontendUrl}/pago-fallido` |
| `GET /pago-pendiente` | `{frontendUrl}/pago-pendiente` |

---

## Campos nuevos en Producto (Admin CRUD)

### `POST /api/productos`

```json
{
  "nombre": "string",
  "descripcion": "string | null",
  "categoria": "string | null",
  "imagenUrl": "string | null",
  "precioBase": 0,
  "stock": 0,
  "stockMinimo": 5,
  "stockInmediato": false,
  "enOferta": false,
  "precioOferta": null
}
```

### `PUT /api/productos/{id}`

```json
{
  "id": 0,
  "nombre": "string",
  "descripcion": "string | null",
  "categoria": "string | null",
  "imagenUrl": "string | null",
  "precioBase": 0,
  "stock": 0,
  "stockMinimo": 0,
  "activo": true,
  "stockInmediato": false,
  "enOferta": false,
  "precioOferta": null
}
```

| Campo | Tipo | Descripción |
|-------|------|-------------|
| stockInmediato | boolean | Producto disponible para retiro inmediato |
| enOferta | boolean | Producto en oferta |
| precioOferta | number \| null | Precio rebajado (requerido si `enOferta = true`) |

---

## DTOs Referencia

### ProductoDto (response catálogo)

```json
{
  "id": 0,
  "nombre": "string",
  "descripcion": "string | null",
  "precioBase": 0,
  "categoria": "string | null",
  "imagenUrl": "string | null",
  "stock": 0,
  "stockMinimo": 0,
  "activo": true,
  "fechaCreacion": "datetime",
  "fechaModificacion": "datetime | null",
  "stockInmediato": false,
  "enOferta": false,
  "precioOferta": null,
  "imagenes": [
    {
      "id": 0,
      "productoId": 0,
      "url": "string",
      "orden": 0,
      "esPrincipal": false
    }
  ]
}
```

### AuthResponseDto

```json
{
  "token": "string",
  "refreshToken": "string",
  "email": "string",
  "firstName": "string",
  "lastName": "string",
  "roles": ["string"],
  "expiresAt": "datetime"
}
```

### CarritoDto

```json
{
  "id": 0,
  "clienteNombre": "string",
  "items": [
    {
      "productoId": 0,
      "productoNombre": "string",
      "imagenUrl": "string | null",
      "precioUnitario": 0,
      "cantidad": 0,
      "subtotal": 0
    }
  ],
  "total": 0,
  "cantidadItems": 0
}
```

### PedidoDto

```json
{
  "id": 0,
  "clienteNombre": "string",
  "fechaPedido": "datetime",
  "total": 0,
  "costoEnvio": 0,
  "estado": "string",
  "tipoPago": "string",
  "estadoPago": "string",
  "esRetiroLocal": true,
  "direccionEntrega": "string | null",
  "detalles": [
    {
      "productoId": 0,
      "productoNombre": "string",
      "cantidad": 0,
      "precioUnitario": 0,
      "subtotal": 0
    }
  ]
}
```

### Estados posibles

**EstadoPedido:** `Pendiente`, `Confirmado`, `EnPreparacion`, `Listo`, `Entregado`, `Cancelado`

**EstadoPago:** `Pendiente`, `Pagado`, `Fallido`, `Reembolsado`

**TipoPago:** `Efectivo` (1), `Transferencia` (2), `MercadoPago` (3)

---

## Resumen de nuevos endpoints implementados

| # | Endpoint | Método | Auth | Descripción |
|---|----------|--------|------|-------------|
| 1 | `GET /health` | - | ❌ | Health check |
| 2 | `POST /api/auth/forgot-password` | Auth | ❌ | Solicitar reset de contraseña |
| 3 | `POST /api/auth/reset-password` | Auth | ❌ | Ejecutar reset de contraseña |
| 4 | `GET /api/catalogo/productos/inmediato` | Catálogo | ❌ | Productos retiro inmediato |
| 5 | `GET /api/catalogo/productos/oferta` | Catálogo | ❌ | Productos en oferta |
| 6 | `GET /api/admin/envio/config` | Admin | ✅ Admin | Obtener config envío |
| 7 | `PUT /api/admin/envio/config` | Admin | ✅ Admin | Actualizar config envío |

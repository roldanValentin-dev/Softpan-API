# 📘 Softpan API - Documentación para Frontend

## 🔗 Base URL
```
https://localhost:7097/api
```

## 🔐 Autenticación

Todos los endpoints (excepto `/auth/login` y `/auth/register`) requieren un token JWT en el header:
```
Authorization: Bearer {token}
```

---

## 📍 Endpoints

### 🔑 Autenticación

#### POST `/auth/login`
Iniciar sesión y obtener token JWT.

**Request:**
```json
{
  "email": "mojitoawp@gmail.com",
  "password": "Admin123!"
}
```

**Response 200:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "mojitoawp@gmail.com",
  "nombre": "Valentin Roldan",
  "roles": ["Admin"]
}
```

**Response 401:**
```json
{
  "message": "Credenciales inválidas"
}
```

---

#### POST `/auth/register`
Registrar nuevo usuario.

**Request:**
```json
{
  "email": "nuevo@ejemplo.com",
  "password": "Password123!",
  "nombre": "Juan Pérez"
}
```

**Response 200:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "nuevo@ejemplo.com",
  "nombre": "Juan Pérez",
  "roles": ["Vendedor"]
}
```

---

### 📦 Productos

#### GET `/productos`
Obtener todos los productos.

**Response 200:**
```json
[
  {
    "id": 19,
    "nombre": "Pan Francés",
    "descripcion": "Pan blanco tradicional",
    "precioUnitario": 1.50,
    "activo": true
  }
]
```

---

#### GET `/productos/activos`
Obtener solo productos activos.

**Response 200:**
```json
[
  {
    "id": 19,
    "nombre": "Pan Francés",
    "descripcion": "Pan blanco tradicional",
    "precioUnitario": 1.50,
    "activo": true
  }
]
```

---

#### GET `/productos/{id}`
Obtener producto por ID.

**Response 200:**
```json
{
  "id": 19,
  "nombre": "Pan Francés",
  "descripcion": "Pan blanco tradicional",
  "precioUnitario": 1.50,
  "activo": true
}
```

**Response 404:**
```json
{
  "message": "Producto no encontrado"
}
```

---

#### POST `/productos`
Crear nuevo producto.

**Request:**
```json
{
  "nombre": "Pan Integral",
  "descripcion": "Pan integral con semillas",
  "precioUnitario": 2.50
}
```

**Response 201:**
```json
{
  "id": 25,
  "nombre": "Pan Integral",
  "descripcion": "Pan integral con semillas",
  "precioUnitario": 2.50,
  "activo": true
}
```

---

#### PUT `/productos/{id}`
Actualizar producto existente.

**Request:**
```json
{
  "id": 19,
  "nombre": "Pan Francés Premium",
  "descripcion": "Pan blanco tradicional mejorado",
  "precioUnitario": 1.80,
  "activo": true
}
```

**Response 200:**
```json
{
  "id": 19,
  "nombre": "Pan Francés Premium",
  "descripcion": "Pan blanco tradicional mejorado",
  "precioUnitario": 1.80,
  "activo": true
}
```

---

#### DELETE `/productos/{id}`
Eliminar producto (soft delete).

**Response 204:** No Content

**Response 404:**
```json
{
  "message": "Producto no encontrado"
}
```

---

### 👥 Clientes

#### GET `/clientes`
Obtener todos los clientes.

**Response 200:**
```json
[
  {
    "id": 9,
    "nombre": "Juan Pérez",
    "telefono": "555-0001",
    "direccion": "Calle Principal 123",
    "activo": true
  }
]
```

---

#### GET `/clientes/{id}`
Obtener cliente por ID.

**Response 200:**
```json
{
  "id": 9,
  "nombre": "Juan Pérez",
  "telefono": "555-0001",
  "direccion": "Calle Principal 123",
  "activo": true
}
```

---

#### GET `/clientes/con-deudas`
Obtener clientes con deudas pendientes.

**Response 200:**
```json
[
  {
    "id": 11,
    "nombre": "Carlos Rodríguez",
    "telefono": "555-0003",
    "direccion": "Avenida Central 789",
    "activo": true
  }
]
```

---

#### POST `/clientes`
Crear nuevo cliente.

**Request:**
```json
{
  "nombre": "María López",
  "telefono": "555-1234",
  "direccion": "Calle Secundaria 456"
}
```

**Response 201:**
```json
{
  "id": 12,
  "nombre": "María López",
  "telefono": "555-1234",
  "direccion": "Calle Secundaria 456",
  "activo": true
}
```

---

#### PUT `/clientes/{id}`
Actualizar cliente existente.

**Request:**
```json
{
  "id": 9,
  "nombre": "Juan Pérez García",
  "telefono": "555-0001",
  "direccion": "Calle Principal 123",
  "activo": true
}
```

**Response 200:**
```json
{
  "id": 9,
  "nombre": "Juan Pérez García",
  "telefono": "555-0001",
  "direccion": "Calle Principal 123",
  "activo": true
}
```

---

#### DELETE `/clientes/{id}`
Eliminar cliente (soft delete).

**Response 204:** No Content

---

### 🛒 Ventas

#### GET `/ventas`
Obtener todas las ventas.

**Response 200:**
```json
[
  {
    "id": 5,
    "clienteId": 9,
    "nombreCliente": "Juan Pérez",
    "fechaCreacion": "2025-11-25T18:00:00Z",
    "montoTotal": 11.50,
    "montoPagado": 11.50,
    "estado": 2,
    "detalles": [
      {
        "id": 3,
        "productoId": 19,
        "nombreProducto": "Pan Francés",
        "cantidad": 3,
        "precioUnitario": 1.50,
        "subtotal": 4.50
      }
    ]
  }
]
```

---

#### GET `/ventas/{id}`
Obtener venta por ID con detalles.

**Response 200:**
```json
{
  "id": 5,
  "clienteId": 9,
  "nombreCliente": "Juan Pérez",
  "fechaCreacion": "2025-11-25T18:00:00Z",
  "montoTotal": 11.50,
  "montoPagado": 11.50,
  "estado": 2,
  "detalles": [
    {
      "id": 3,
      "productoId": 19,
      "nombreProducto": "Pan Francés",
      "cantidad": 3,
      "precioUnitario": 1.50,
      "subtotal": 4.50
    }
  ]
}
```

---

#### GET `/ventas/pendientes`
Obtener ventas con saldo pendiente.

**Response 200:**
```json
[
  {
    "id": 7,
    "clienteId": 11,
    "nombreCliente": "Carlos Rodríguez",
    "fechaCreacion": "2025-11-27T18:00:00Z",
    "montoTotal": 35.50,
    "montoPagado": 0.00,
    "estado": 0
  }
]
```

---

#### POST `/ventas`
Crear nueva venta.

**Request:**
```json
{
  "clienteId": 9,
  "detalles": [
    {
      "productoId": 19,
      "cantidad": 5,
      "precioUnitario": 1.50
    },
    {
      "productoId": 21,
      "cantidad": 2,
      "precioUnitario": 3.50
    }
  ]
}
```

**Response 201:**
```json
{
  "id": 9,
  "clienteId": 9,
  "nombreCliente": "Juan Pérez",
  "fechaCreacion": "2025-12-01T18:15:00Z",
  "montoTotal": 14.50,
  "montoPagado": 0.00,
  "estado": 0,
  "detalles": [
    {
      "id": 11,
      "productoId": 19,
      "nombreProducto": "Pan Francés",
      "cantidad": 5,
      "precioUnitario": 1.50,
      "subtotal": 7.50
    },
    {
      "id": 12,
      "productoId": 21,
      "nombreProducto": "Croissant",
      "cantidad": 2,
      "precioUnitario": 3.50,
      "subtotal": 7.00
    }
  ]
}
```

---

### 💰 Pagos

#### GET `/pagos/venta/{ventaId}`
Obtener pagos de una venta específica.

**Response 200:**
```json
[
  {
    "id": 1,
    "ventaId": 5,
    "monto": 11.50,
    "fechaPago": "2025-11-25T18:30:00Z",
    "metodoPago": "Efectivo"
  }
]
```

---

#### POST `/pagos`
Registrar nuevo pago para una venta.

**Request:**
```json
{
  "ventaId": 7,
  "monto": 20.00,
  "metodoPago": "Efectivo"
}
```

**Response 201:**
```json
{
  "id": 5,
  "ventaId": 7,
  "monto": 20.00,
  "fechaPago": "2025-12-01T18:20:00Z",
  "metodoPago": "Efectivo"
}
```

---

### 📊 Estadísticas

#### GET `/estadisticas/dashboard`
Obtener resumen completo del dashboard.

**Response 200:**
```json
{
  "ventasHoy": {
    "totalVentas": 0,
    "cantidadTransacciones": 0,
    "ticketPromedio": 0,
    "totalCobrado": 0
  },
  "ventasMes": {
    "totalVentas": 0,
    "cantidadTransacciones": 0,
    "ticketPromedio": 0,
    "totalCobrado": 0
  },
  "deudas": {
    "totalDeudas": 1,
    "cantidadClientesConDeuda": 3,
    "promedioDeudaPorCliente": 0.33
  },
  "topProductos": [
    {
      "productoId": 22,
      "nombreProducto": "Empanada de Carne",
      "cantidadVendida": 8,
      "totalVendido": 32
    }
  ],
  "clientesConMayorDeuda": [
    {
      "clienteId": 11,
      "nombreCliente": "Carlos Rodríguez",
      "montoDeuda": 35.5,
      "cantidadVentasPendientes": 1
    }
  ],
  "comparativaMensual": {
    "ventasPeriodoActual": 0,
    "ventasPeriodoAnterior": 86,
    "diferenciaAbsoluta": -86,
    "porcentajeCrecimiento": -100
  }
}
```

---

#### GET `/estadisticas/ventas/hoy`
Obtener resumen de ventas del día actual.

**Response 200:**
```json
{
  "totalVentas": 0,
  "cantidadTransacciones": 0,
  "ticketPromedio": 0,
  "totalCobrado": 0
}
```

---

#### GET `/estadisticas/ventas/semana`
Obtener resumen de ventas de la semana actual.

**Response 200:**
```json
{
  "totalVentas": 0,
  "cantidadTransacciones": 0,
  "ticketPromedio": 0,
  "totalCobrado": 0
}
```

---

#### GET `/estadisticas/ventas/mes`
Obtener resumen de ventas del mes actual.

**Response 200:**
```json
{
  "totalVentas": 0,
  "cantidadTransacciones": 0,
  "ticketPromedio": 0,
  "totalCobrado": 0
}
```

---

#### GET `/estadisticas/ventas/periodo?fechaInicio={fecha}&fechaFin={fecha}`
Obtener resumen de ventas en un período específico.

**Query Params:**
- `fechaInicio`: Fecha inicio (formato: YYYY-MM-DD)
- `fechaFin`: Fecha fin (formato: YYYY-MM-DD)

**Response 200:**
```json
{
  "totalVentas": 86,
  "cantidadTransacciones": 4,
  "ticketPromedio": 21.5,
  "totalCobrado": 85
}
```

---

#### GET `/estadisticas/productos/top?top={numero}`
Obtener productos más vendidos (todos los tiempos).

**Query Params:**
- `top`: Cantidad de productos (default: 5)

**Response 200:**
```json
[
  {
    "productoId": 22,
    "nombreProducto": "Empanada de Carne",
    "cantidadVendida": 8,
    "totalVendido": 32
  },
  {
    "productoId": 21,
    "nombreProducto": "Croissant",
    "cantidadVendida": 5,
    "totalVendido": 17.5
  }
]
```

---

#### GET `/estadisticas/productos/top/periodo?top={numero}&fechaInicio={fecha}&fechaFin={fecha}`
Obtener productos más vendidos en un período.

**Query Params:**
- `top`: Cantidad de productos (default: 5)
- `fechaInicio`: Fecha inicio (formato: YYYY-MM-DD)
- `fechaFin`: Fecha fin (formato: YYYY-MM-DD)

**Response 200:**
```json
[
  {
    "productoId": 22,
    "nombreProducto": "Empanada de Carne",
    "cantidadVendida": 8,
    "totalVendido": 32
  }
]
```

---

#### GET `/estadisticas/deudas/resumen`
Obtener resumen de deudas pendientes.

**Response 200:**
```json
{
  "totalDeudas": 1,
  "cantidadClientesConDeuda": 3,
  "promedioDeudaPorCliente": 0.33
}
```

---

#### GET `/estadisticas/deudas/clientes?top={numero}`
Obtener clientes con mayor deuda.

**Query Params:**
- `top`: Cantidad de clientes (default: 5)

**Response 200:**
```json
[
  {
    "clienteId": 11,
    "nombreCliente": "Carlos Rodríguez",
    "montoDeuda": 35.5,
    "cantidadVentasPendientes": 1
  },
  {
    "clienteId": 10,
    "nombreCliente": "María González",
    "montoDeuda": 4,
    "cantidadVentasPendientes": 1
  }
]
```

---

#### GET `/estadisticas/comparativa/mensual`
Comparar ventas del mes actual vs mes anterior.

**Response 200:**
```json
{
  "ventasPeriodoActual": 0,
  "ventasPeriodoAnterior": 86,
  "diferenciaAbsoluta": -86,
  "porcentajeCrecimiento": -100
}
```

---

#### GET `/estadisticas/comparativa/semanal`
Comparar ventas de la semana actual vs semana anterior.

**Response 200:**
```json
{
  "ventasPeriodoActual": 0,
  "ventasPeriodoAnterior": 86,
  "diferenciaAbsoluta": -86,
  "porcentajeCrecimiento": -100
}
```

---

#### GET `/estadisticas/ventas/por-dia-semana`
Obtener ventas agrupadas por día de la semana (últimos 30 días).

**Response 200:**
```json
[
  {
    "diaSemana": "Martes",
    "totalVentas": 11.5,
    "cantidadTransacciones": 1
  },
  {
    "diaSemana": "Miércoles",
    "totalVentas": 24,
    "cantidadTransacciones": 1
  },
  {
    "diaSemana": "Jueves",
    "totalVentas": 50.5,
    "cantidadTransacciones": 2
  }
]
```

---

## 📝 Notas Importantes

### Estados de Venta (Enum)
- `0`: Pendiente
- `1`: ParcialmentePagada
- `2`: Pagada

### Roles de Usuario
- `Admin`: Acceso completo
- `Vendedor`: Crear ventas, ver productos y clientes
- `Cajero`: Registrar pagos, ver ventas

### CORS
La API acepta peticiones desde:
- `http://localhost:5173` (Vite)
- `http://localhost:3000` (Create React App)
- Dominios de Vercel (`*.vercel.app`)

### Rate Limiting
- Máximo 100 peticiones por minuto por IP

### Caché
- Productos, clientes y ventas están cacheados en Redis por 5 minutos
- Las estadísticas se cachean por 2 minutos

---

## 🚀 Ejemplo de Uso en React

```typescript
// api/client.ts
const API_BASE_URL = 'https://localhost:7097/api';

export const apiClient = {
  async login(email: string, password: string) {
    const response = await fetch(`${API_BASE_URL}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password })
    });
    return response.json();
  },

  async getProductos(token: string) {
    const response = await fetch(`${API_BASE_URL}/productos`, {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    return response.json();
  },

  async getDashboard(token: string) {
    const response = await fetch(`${API_BASE_URL}/estadisticas/dashboard`, {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    return response.json();
  }
};
```

---

**Última actualización:** 1 de Diciembre 2025

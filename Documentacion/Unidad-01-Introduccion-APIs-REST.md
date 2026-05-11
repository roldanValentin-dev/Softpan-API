# Unidad 1: Introducción a las APIs REST

## 📋 Contenido
1. [¿Qué es una API?](#qué-es-una-api)
2. [Arquitectura Cliente-Servidor](#arquitectura-cliente-servidor)
3. [¿Qué es REST?](#qué-es-rest)
4. [Protocolo HTTP](#protocolo-http)
5. [JSON como formato de intercambio](#json-como-formato-de-intercambio)
6. [Códigos de estado HTTP](#códigos-de-estado-http)
7. [Herramientas para trabajar con APIs](#herramientas-para-trabajar-con-apis)
8. [Ejemplo práctico en Softpan](#ejemplo-práctico-en-softpan)

---

## 🎯 ¿Qué es una API?

**API** significa **Application Programming Interface** (Interfaz de Programación de Aplicaciones).

### Definición Simple
Una API es un **intermediario** que permite que dos aplicaciones se comuniquen entre sí.

### Analogía del Restaurante 🍽️
Imagina que estás en un restaurante:
- **Tú (Cliente)**: Quieres comida
- **Cocina (Servidor)**: Prepara la comida
- **Mesero (API)**: Lleva tu pedido a la cocina y te trae la comida

La API es el **mesero** que:
1. Recibe tu pedido (request)
2. Lo lleva a la cocina (servidor)
3. Te trae la comida (response)

### En el Mundo Real
```
Frontend (React/Vue/Angular)  →  API REST  →  Base de Datos
      Cliente                    Mesero         Cocina
```

---

## 🏗️ Arquitectura Cliente-Servidor

### Componentes

#### 1. Cliente (Frontend)
- Aplicación web (React, Vue, Angular)
- Aplicación móvil (iOS, Android)
- Otra API
- Postman, cURL

**Responsabilidades:**
- Mostrar información al usuario
- Enviar solicitudes (requests)
- Recibir respuestas (responses)

#### 2. Servidor (Backend)
- API REST (.NET, Node.js, Java, Python)
- Base de datos
- Lógica de negocio

**Responsabilidades:**
- Procesar solicitudes
- Acceder a la base de datos
- Aplicar lógica de negocio
- Devolver respuestas

### Flujo de Comunicación

```
┌─────────────┐                    ┌─────────────┐
│   CLIENTE   │                    │  SERVIDOR   │
│  (Frontend) │                    │   (API)     │
└─────────────┘                    └─────────────┘
       │                                  │
       │  1. REQUEST (Solicitud)          │
       │  GET /api/productos              │
       │─────────────────────────────────>│
       │                                  │
       │                                  │  2. Procesa
       │                                  │     Consulta BD
       │                                  │     Aplica lógica
       │                                  │
       │  3. RESPONSE (Respuesta)         │
       │  200 OK + JSON con productos     │
       │<─────────────────────────────────│
       │                                  │
```

---

## 🌐 ¿Qué es REST?

**REST** significa **Representational State Transfer** (Transferencia de Estado Representacional).

### Principios de REST

#### 1. **Stateless (Sin Estado)**
Cada petición es independiente. El servidor no guarda información de peticiones anteriores.

```
❌ MAL (Stateful):
Request 1: Login → Servidor guarda "usuario logueado"
Request 2: Ver productos → Servidor recuerda que estás logueado

✅ BIEN (Stateless):
Request 1: Login → Devuelve TOKEN
Request 2: Ver productos + TOKEN → Servidor valida el token en cada request
```

#### 2. **Cliente-Servidor**
Separación clara entre frontend y backend.

#### 3. **Cacheable**
Las respuestas pueden ser cacheadas para mejorar performance.

#### 4. **Interfaz Uniforme**
Uso de URLs estándar y métodos HTTP.

#### 5. **Sistema de Capas**
El cliente no sabe si está conectado directamente al servidor o a través de intermediarios.

---

## 📡 Protocolo HTTP

HTTP (HyperText Transfer Protocol) es el protocolo de comunicación.

### Métodos HTTP (Verbos)

| Método | Acción | Ejemplo | Descripción |
|--------|--------|---------|-------------|
| **GET** | Leer | `GET /api/productos` | Obtiene datos, no modifica nada |
| **POST** | Crear | `POST /api/productos` | Crea un nuevo recurso |
| **PUT** | Actualizar | `PUT /api/productos/1` | Actualiza un recurso completo |
| **PATCH** | Actualizar parcial | `PATCH /api/productos/1` | Actualiza parte de un recurso |
| **DELETE** | Eliminar | `DELETE /api/productos/1` | Elimina un recurso |

### Anatomía de una Request HTTP

```http
POST /api/auth/login HTTP/1.1
Host: localhost:7097
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

{
  "email": "admin@softpan.com",
  "password": "Admin123!"
}
```

**Partes:**
1. **Método**: POST
2. **URL**: /api/auth/login
3. **Headers**: Content-Type, Authorization
4. **Body**: JSON con datos

### Anatomía de una Response HTTP

```http
HTTP/1.1 200 OK
Content-Type: application/json
Date: Mon, 01 May 2024 10:30:00 GMT

{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "admin@softpan.com",
  "roles": ["Admin"]
}
```

**Partes:**
1. **Status Code**: 200 OK
2. **Headers**: Content-Type, Date
3. **Body**: JSON con respuesta

---

## 📦 JSON como formato de intercambio

**JSON** (JavaScript Object Notation) es el formato estándar para intercambiar datos.

### ¿Por qué JSON?
- ✅ Fácil de leer para humanos
- ✅ Fácil de parsear para máquinas
- ✅ Ligero (menos bytes que XML)
- ✅ Soportado por todos los lenguajes

### Estructura de JSON

```json
{
  "id": 1,
  "nombre": "Torta de Chocolate",
  "precio": 5500.00,
  "activo": true,
  "categoria": "Tortas",
  "tags": ["chocolate", "dulce", "premium"],
  "proveedor": {
    "id": 10,
    "nombre": "Distribuidora ABC"
  },
  "stock": null
}
```

**Tipos de datos:**
- `string`: "Torta de Chocolate"
- `number`: 5500.00, 1
- `boolean`: true, false
- `array`: ["chocolate", "dulce"]
- `object`: { "id": 10, "nombre": "..." }
- `null`: null

### JSON en C# (.NET)

```csharp
// Clase C#
public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public decimal Precio { get; set; }
    public bool Activo { get; set; }
}

// Se serializa automáticamente a JSON:
{
  "id": 1,
  "nombre": "Torta de Chocolate",
  "precio": 5500.00,
  "activo": true
}
```

---

## 🚦 Códigos de estado HTTP

Los códigos de estado indican el resultado de la petición.

### Categorías

| Rango | Categoría | Significado |
|-------|-----------|-------------|
| **1xx** | Informacional | La petición fue recibida, procesando |
| **2xx** | Éxito | La petición fue exitosa |
| **3xx** | Redirección | Se necesita acción adicional |
| **4xx** | Error del Cliente | La petición tiene un error |
| **5xx** | Error del Servidor | El servidor falló al procesar |

### Códigos más comunes

#### ✅ 2xx - Éxito

| Código | Nombre | Cuándo usar | Ejemplo |
|--------|--------|-------------|---------|
| **200** | OK | Operación exitosa | GET /api/productos |
| **201** | Created | Recurso creado | POST /api/productos |
| **204** | No Content | Éxito sin contenido | DELETE /api/productos/1 |

#### ❌ 4xx - Error del Cliente

| Código | Nombre | Cuándo usar | Ejemplo |
|--------|--------|-------------|---------|
| **400** | Bad Request | Datos inválidos | Email mal formateado |
| **401** | Unauthorized | No autenticado | Token inválido o ausente |
| **403** | Forbidden | Sin permisos | Usuario no es Admin |
| **404** | Not Found | Recurso no existe | Producto ID 999 no existe |
| **409** | Conflict | Conflicto de estado | Email ya registrado |
| **422** | Unprocessable Entity | Validación fallida | Password muy corto |
| **429** | Too Many Requests | Rate limit excedido | Más de 100 req/min |

#### 💥 5xx - Error del Servidor

| Código | Nombre | Cuándo usar | Ejemplo |
|--------|--------|-------------|---------|
| **500** | Internal Server Error | Error no manejado | Exception en el código |
| **502** | Bad Gateway | Error de proxy | API caída |
| **503** | Service Unavailable | Servicio no disponible | Base de datos caída |

### Ejemplo en Softpan

```csharp
// 200 OK - Éxito
[HttpGet]
public async Task<IActionResult> GetProductos()
{
    var productos = await _productoService.GetAllAsync();
    return Ok(productos); // 200 OK
}

// 201 Created - Recurso creado
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateProductoDto dto)
{
    var producto = await _productoService.CreateAsync(dto);
    return CreatedAtAction(nameof(GetById), new { id = producto.Id }, producto); // 201 Created
}

// 404 Not Found - No existe
[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id)
{
    var producto = await _productoService.GetByIdAsync(id);
    if (producto == null)
        return NotFound(); // 404 Not Found
    
    return Ok(producto); // 200 OK
}

// 400 Bad Request - Datos inválidos
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateProductoDto dto)
{
    if (dto.Precio <= 0)
        return BadRequest("El precio debe ser mayor a 0"); // 400 Bad Request
    
    // ...
}
```

---

## 🛠️ Herramientas para trabajar con APIs

### 1. **Postman** 📮
La herramienta más popular para probar APIs.

**Características:**
- Enviar requests (GET, POST, PUT, DELETE)
- Guardar colecciones de endpoints
- Variables de entorno
- Tests automatizados
- Documentación automática

**Ejemplo de uso:**
```
1. Crear nuevo request
2. Método: POST
3. URL: http://localhost:7097/api/auth/login
4. Body → raw → JSON:
   {
     "email": "admin@softpan.com",
     "password": "Admin123!"
   }
5. Send
6. Ver respuesta con token
```

### 2. **Swagger UI** 📚
Documentación interactiva automática.

**Ventajas:**
- Se genera automáticamente del código
- Permite probar endpoints desde el navegador
- Documentación siempre actualizada
- Soporta autenticación JWT

**Acceso en Softpan:**
```
http://localhost:7097/swagger
```

### 3. **cURL** 💻
Herramienta de línea de comandos.

**Ejemplo:**
```bash
# GET
curl http://localhost:7097/api/catalogo/productos

# POST con JSON
curl -X POST http://localhost:7097/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@softpan.com","password":"Admin123!"}'

# Con autenticación
curl http://localhost:7097/api/productos \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### 4. **Extensiones de VS Code**
- **REST Client**: Archivos .http para requests
- **Thunder Client**: Postman integrado en VS Code

### 5. **Navegador** 🌐
Solo para requests GET:
```
http://localhost:7097/api/catalogo/productos
```

---

## 💡 Ejemplo práctico en Softpan

### Caso: Obtener lista de productos

#### 1. **Request del Cliente**
```http
GET /api/catalogo/productos HTTP/1.1
Host: localhost:7097
Accept: application/json
```

#### 2. **Procesamiento en el Servidor**

```csharp
// CatalogoController.cs
[HttpGet("productos")]
public async Task<IActionResult> GetProductos()
{
    // 1. Llama al servicio
    var productos = await _productoService.GetProductosActivosAsync();
    
    // 2. Devuelve respuesta
    return Ok(productos); // 200 OK
}
```

#### 3. **Response al Cliente**
```http
HTTP/1.1 200 OK
Content-Type: application/json

[
  {
    "id": 1,
    "nombre": "Torta de Chocolate",
    "descripcion": "Deliciosa torta de chocolate",
    "precioBase": 5500.00,
    "categoria": "Tortas",
    "stock": 10,
    "activo": true,
    "imagenes": [
      {
        "id": 1,
        "url": "/images/productos/torta-chocolate.jpg",
        "esPrincipal": true
      }
    ]
  },
  {
    "id": 2,
    "nombre": "Tarta de Frutilla",
    "precioBase": 6000.00,
    "categoria": "Tartas",
    "stock": 8,
    "activo": true,
    "imagenes": []
  }
]
```

### Caso: Crear un pedido (con autenticación)

#### 1. **Request del Cliente**
```http
POST /api/pedidos HTTP/1.1
Host: localhost:7097
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

{
  "fechaEntrega": "2024-05-15T10:00:00Z",
  "observaciones": "Entregar antes de las 10 AM",
  "detalles": [
    {
      "productoId": 1,
      "cantidad": 2
    }
  ]
}
```

#### 2. **Procesamiento en el Servidor**

```csharp
// PedidosController.cs
[Authorize] // Requiere autenticación
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreatePedidoDto dto)
{
    // 1. Obtiene el usuario del token JWT
    var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    // 2. Crea el pedido
    var pedido = await _pedidoService.CreatePedidoAsync(usuarioId, dto);
    
    // 3. Devuelve 201 Created
    return CreatedAtAction(nameof(GetById), new { id = pedido.Id }, pedido);
}
```

#### 3. **Response al Cliente**
```http
HTTP/1.1 201 Created
Location: /api/pedidos/1
Content-Type: application/json

{
  "id": 1,
  "clienteNombre": "Juan Pérez",
  "fechaPedido": "2024-05-01T15:30:00Z",
  "fechaEntrega": "2024-05-15T10:00:00Z",
  "estado": "Pendiente",
  "total": 11000.00,
  "detalles": [
    {
      "productoNombre": "Torta de Chocolate",
      "cantidad": 2,
      "precioUnitario": 5500.00,
      "subtotal": 11000.00
    }
  ]
}
```

---

## 📚 Conceptos Clave para Recordar

### ✅ API REST
- Interfaz para comunicación entre aplicaciones
- Usa HTTP como protocolo
- Stateless (sin estado)
- Recursos identificados por URLs

### ✅ Métodos HTTP
- **GET**: Leer datos
- **POST**: Crear datos
- **PUT**: Actualizar datos
- **DELETE**: Eliminar datos

### ✅ Códigos de Estado
- **2xx**: Éxito
- **4xx**: Error del cliente
- **5xx**: Error del servidor

### ✅ JSON
- Formato estándar de intercambio
- Ligero y legible
- Soportado universalmente

### ✅ Cliente-Servidor
- Separación clara de responsabilidades
- Cliente solicita, servidor responde
- Comunicación mediante HTTP

---

## 🎯 Ejercicios Prácticos

### Ejercicio 1: Identificar componentes
Dado este escenario, identifica el cliente, servidor y API:
```
Una app móvil de delivery muestra restaurantes cercanos.
```

**Respuesta:**
- Cliente: App móvil
- API: Backend REST que procesa solicitudes
- Servidor: Base de datos con restaurantes

### Ejercicio 2: Elegir método HTTP
¿Qué método HTTP usarías para:
1. Ver tu perfil de usuario
2. Cambiar tu contraseña
3. Eliminar tu cuenta
4. Registrarte

**Respuesta:**
1. GET /api/perfil
2. PUT /api/perfil/password
3. DELETE /api/usuarios/me
4. POST /api/auth/register

### Ejercicio 3: Códigos de estado
¿Qué código de estado devolverías en estos casos?
1. Usuario creado exitosamente
2. Producto no encontrado
3. Token JWT expirado
4. Email ya registrado

**Respuesta:**
1. 201 Created
2. 404 Not Found
3. 401 Unauthorized
4. 409 Conflict

---

## 🔗 Recursos Adicionales

### Documentación Oficial
- [REST API Tutorial](https://restfulapi.net/)
- [HTTP Status Codes](https://httpstatuses.com/)
- [JSON.org](https://www.json.org/)

### Herramientas
- [Postman](https://www.postman.com/)
- [Swagger](https://swagger.io/)
- [JSONPlaceholder](https://jsonplaceholder.typicode.com/) - API de prueba

### Videos Recomendados
- "What is REST API?" - Explicación visual
- "HTTP Methods Explained" - Tutorial completo

---

## ✅ Checklist de Aprendizaje

Marca lo que ya dominas:

- [ ] Entiendo qué es una API y para qué sirve
- [ ] Conozco la arquitectura Cliente-Servidor
- [ ] Sé qué es REST y sus principios
- [ ] Conozco los métodos HTTP (GET, POST, PUT, DELETE)
- [ ] Entiendo JSON y su estructura
- [ ] Conozco los códigos de estado HTTP más comunes
- [ ] Puedo usar Postman para probar APIs
- [ ] Puedo usar Swagger para documentar APIs
- [ ] Entiendo el flujo completo de una request/response

---

## 🎓 Conclusión

Las APIs REST son la base de la comunicación moderna entre aplicaciones. Entender estos conceptos fundamentales te permitirá:

✅ Diseñar APIs claras y consistentes
✅ Comunicarte efectivamente con otros desarrolladores
✅ Debuggear problemas de integración
✅ Construir aplicaciones escalables

En la siguiente unidad veremos cómo .NET implementa estos conceptos y crearemos nuestra primera API.

---

**Próxima unidad:** [Unidad 2: Fundamentos de .NET y C#](./Unidad-02-Fundamentos-NET-CSharp.md)

---

**📌 Nota:** Todos los ejemplos están basados en el proyecto real **Softpan**, una API completa de gestión para pastelerías.

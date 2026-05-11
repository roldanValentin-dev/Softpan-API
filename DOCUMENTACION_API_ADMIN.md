# 📚 Documentación API - Panel Administrativo

## 🎯 Información General

**Base URL:** `http://localhost:7097/api`  
**Autenticación:** Todos los endpoints requieren JWT Bearer Token en el header  
**Roles permitidos:** Admin, Vendedor (según endpoint)

---

## 📦 GESTIÓN DE PRODUCTOS

### 1. Listar Todos los Productos

**Endpoint:** `GET /api/productos`  
**Autenticación:** ✅ Requerida  
**Roles:** Admin, Vendedor  

**Descripción:**  
Obtiene el listado completo de todos los productos del sistema, incluyendo activos e inactivos. Retorna información básica de cada producto con sus imágenes asociadas.

**Respuesta:**  
Lista de productos con: ID, nombre, descripción, precio base, stock actual, stock mínimo, categoría, estado activo/inactivo, y colección de imágenes.

---

### 2. Obtener Producto por ID

**Endpoint:** `GET /api/productos/{id}`  
**Autenticación:** ✅ Requerida  
**Roles:** Admin, Vendedor  

**Descripción:**  
Obtiene la información detallada de un producto específico mediante su ID. Incluye todas las imágenes del producto ordenadas por el campo "orden".

**Parámetros:**
- `id` (int): ID del producto a consultar

**Respuesta:**  
Información completa del producto incluyendo todas sus imágenes con URLs, orden y si es imagen principal.

**Errores:**
- 404: Producto no encontrado

---

### 3. Crear Nuevo Producto

**Endpoint:** `POST /api/productos`  
**Autenticación:** ✅ Requerida  
**Roles:** Admin  
**Content-Type:** `application/json`

**Descripción:**  
Crea un nuevo producto en el sistema. El producto se crea inicialmente sin imágenes, las cuales deben agregarse posteriormente mediante el endpoint de upload de imágenes.

**Campos requeridos:**
- `nombre` (string): Nombre del producto
- `descripcion` (string): Descripción detallada
- `precioBase` (decimal): Precio base del producto
- `categoria` (string): Categoría del producto
- `stock` (int): Cantidad inicial en stock
- `stockMinimo` (int): Nivel mínimo de stock para alertas
- `activo` (bool): Si el producto está activo o no

**Validaciones:**
- Nombre: obligatorio, máximo 200 caracteres
- Precio base: debe ser mayor a 0
- Stock: no puede ser negativo
- Stock mínimo: no puede ser negativo

**Respuesta:**  
Producto creado con su ID asignado y todos los campos proporcionados.

---

### 4. Actualizar Producto

**Endpoint:** `PUT /api/productos/{id}`  
**Autenticación:** ✅ Requerida  
**Roles:** Admin  
**Content-Type:** `application/json`

**Descripción:**  
Actualiza la información de un producto existente. Permite modificar nombre, descripción, precio, categoría y estado activo. No modifica el stock (usar endpoint específico para eso).

**Parámetros:**
- `id` (int): ID del producto a actualizar

**Campos actualizables:**
- `nombre` (string)
- `descripcion` (string)
- `precioBase` (decimal)
- `categoria` (string)
- `activo` (bool)

**Respuesta:**  
Producto actualizado con todos sus campos.

**Errores:**
- 404: Producto no encontrado
- 400: Datos de validación incorrectos

---

### 5. Actualizar Stock del Producto ⭐ NUEVO

**Endpoint:** `PUT /api/productos/{id}/stock`  
**Autenticación:** ✅ Requerida  
**Roles:** Admin  
**Content-Type:** `application/json`

**Descripción:**  
Endpoint específico para actualizar únicamente el stock y stock mínimo de un producto. Útil para ajustes de inventario sin modificar otros datos del producto.

**Parámetros:**
- `id` (int): ID del producto

**Campos:**
- `stock` (int): Nueva cantidad en stock
- `stockMinimo` (int): Nuevo nivel mínimo de stock

**Validaciones:**
- Stock no puede ser negativo
- Stock mínimo no puede ser negativo

**Respuesta:**  
Producto con stock actualizado.

**Casos de uso:**
- Ajuste de inventario manual
- Corrección de stock después de conteo físico
- Modificación de alertas de stock bajo

**Errores:**
- 404: Producto no encontrado
- 400: Valores de stock inválidos

---

### 6. Eliminar Producto

**Endpoint:** `DELETE /api/productos/{id}`  
**Autenticación:** ✅ Requerida  
**Roles:** Admin  

**Descripción:**  
Elimina un producto del sistema de forma permanente. Esta acción también elimina todas las imágenes asociadas al producto (tanto registros en base de datos como archivos físicos del servidor).

**Parámetros:**
- `id` (int): ID del producto a eliminar

**Comportamiento:**
- Elimina el producto de la base de datos
- Elimina todos los registros de imágenes asociadas
- Elimina todos los archivos físicos de imágenes del servidor
- Invalida el caché relacionado

**Respuesta:**  
204 No Content (sin contenido en el body)

**Errores:**
- 404: Producto no encontrado
- 400: No se puede eliminar si tiene ventas o pedidos asociados

**⚠️ Advertencia:**  
Esta acción es irreversible. Se recomienda usar el campo `activo = false` en lugar de eliminar si solo se desea ocultar el producto.

---

## 🖼️ GESTIÓN DE IMÁGENES DE PRODUCTOS ⭐ NUEVO

### 7. Listar Imágenes de un Producto

**Endpoint:** `GET /api/productos/{productoId}/imagenes`  
**Autenticación:** ❌ No requerida (público)  

**Descripción:**  
Obtiene todas las imágenes asociadas a un producto específico. Las imágenes se retornan ordenadas por el campo "orden" de forma ascendente. Este endpoint es público para permitir que el catálogo online muestre las imágenes sin autenticación.

**Parámetros:**
- `productoId` (int): ID del producto

**Respuesta:**  
Lista de imágenes con:
- `id`: ID de la imagen
- `productoId`: ID del producto al que pertenece
- `url`: URL relativa de la imagen (ej: `/images/productos/20240115_abc123.jpg`)
- `orden`: Número de orden para visualización (0, 1, 2...)
- `esPrincipal`: Booleano indicando si es la imagen principal
- `fechaCreacion`: Fecha y hora de creación

**Casos de uso:**
- Mostrar galería de imágenes en el catálogo público
- Cargar imágenes en el panel de administración
- Obtener imagen principal para listados

**Errores:**
- 404: Producto no encontrado

---

### 8. Subir Nueva Imagen ⭐ NUEVO

**Endpoint:** `POST /api/productos/{productoId}/imagenes`  
**Autenticación:** ✅ Requerida  
**Roles:** Admin, Vendedor  
**Content-Type:** `multipart/form-data`

**Descripción:**  
Permite subir una nueva imagen para un producto. El archivo se guarda físicamente en el servidor en la carpeta `wwwroot/images/productos/` con un nombre único generado automáticamente. Se crea un registro en la base de datos con la URL de la imagen.

**Parámetros:**
- `productoId` (int): ID del producto

**Campos del formulario:**
- `file` (archivo): Archivo de imagen a subir (REQUERIDO)
- `orden` (int): Orden de visualización (opcional, default: 0)
- `esPrincipal` (bool): Si es la imagen principal (opcional, default: false)

**Validaciones del archivo:**
- **Formatos permitidos:** .jpg, .jpeg, .png, .gif, .webp
- **Tamaño máximo:** 5 MB
- **Archivo no puede estar vacío**

**Comportamiento:**
1. Valida el tipo y tamaño del archivo
2. Genera un nombre único: `{timestamp}_{guid}.{extension}`
3. Crea la carpeta si no existe
4. Guarda el archivo físicamente en el servidor
5. Crea el registro en la base de datos
6. Si se marca como principal, desmarca las demás imágenes
7. Invalida el caché del producto

**Respuesta:**  
Imagen creada con su ID, URL generada, orden, si es principal y fecha de creación.

**Casos de uso:**
- Agregar fotos de productos nuevos
- Agregar múltiples ángulos de un producto
- Reemplazar imágenes de baja calidad

**Errores:**
- 400: No se proporcionó archivo
- 400: Extensión no permitida
- 400: Archivo excede tamaño máximo (5 MB)
- 400: Archivo vacío
- 404: Producto no encontrado

**💡 Nota importante:**  
El frontend debe enviar el archivo usando `FormData` con el campo llamado exactamente `file`. No incluir `Content-Type` en los headers, el navegador lo configura automáticamente.

---

### 9. Actualizar Imagen (Orden/Principal) ⭐ NUEVO

**Endpoint:** `PUT /api/productos/{productoId}/imagenes/{id}`  
**Autenticación:** ✅ Requerida  
**Roles:** Admin, Vendedor  
**Content-Type:** `application/json`

**Descripción:**  
Actualiza las propiedades de una imagen existente. Permite cambiar el orden de visualización y marcar/desmarcar como imagen principal. No permite cambiar el archivo de imagen en sí (para eso se debe eliminar y subir una nueva).

**Parámetros:**
- `productoId` (int): ID del producto
- `id` (int): ID de la imagen a actualizar

**Campos actualizables:**
- `orden` (int): Nuevo orden de visualización
- `esPrincipal` (bool): Si es la imagen principal

**Comportamiento:**
- Si se marca como principal, automáticamente desmarca las demás imágenes del producto
- Invalida el caché del producto

**Respuesta:**  
Imagen actualizada con todos sus campos.

**Casos de uso:**
- Cambiar el orden de las imágenes en la galería
- Establecer una imagen diferente como principal
- Reorganizar la presentación visual del producto

**Errores:**
- 404: Imagen no encontrada
- 404: Producto no encontrado

---

### 10. Eliminar Imagen ⭐ NUEVO

**Endpoint:** `DELETE /api/productos/{productoId}/imagenes/{id}`  
**Autenticación:** ✅ Requerida  
**Roles:** Admin, Vendedor  

**Descripción:**  
Elimina una imagen de forma permanente. Esta acción elimina tanto el registro de la base de datos como el archivo físico del servidor. Es una operación irreversible.

**Parámetros:**
- `productoId` (int): ID del producto
- `id` (int): ID de la imagen a eliminar

**Comportamiento:**
1. Busca la imagen en la base de datos
2. Elimina el registro de la base de datos
3. Elimina el archivo físico del servidor
4. Invalida el caché del producto

**Respuesta:**  
204 No Content (sin contenido en el body)

**Casos de uso:**
- Eliminar imágenes de baja calidad
- Remover imágenes duplicadas
- Limpiar imágenes obsoletas

**Errores:**
- 404: Imagen no encontrada

**⚠️ Advertencia:**  
Esta acción es irreversible. El archivo físico se elimina permanentemente del servidor.

---

## 🔍 BÚSQUEDA DE PRODUCTOS ⭐ NUEVO

### 11. Buscar Productos

**Endpoint:** `GET /api/catalogo/productos/buscar?q={query}`  
**Autenticación:** ❌ No requerida (público)  

**Descripción:**  
Realiza una búsqueda de productos por texto. Busca coincidencias en el nombre, descripción y categoría del producto. La búsqueda es case-insensitive (no distingue mayúsculas/minúsculas). Los resultados se cachean durante 5 minutos para mejorar el rendimiento.

**Parámetros de query:**
- `q` (string): Texto a buscar (REQUERIDO)

**Comportamiento:**
- Busca en: nombre del producto, descripción y categoría
- Búsqueda case-insensitive
- Solo retorna productos activos
- Incluye las imágenes de cada producto
- Resultados cacheados por 5 minutos

**Respuesta:**  
Lista de productos que coinciden con la búsqueda, incluyendo sus imágenes.

**Casos de uso:**
- Barra de búsqueda en el catálogo público
- Búsqueda rápida en el panel admin
- Filtrado de productos por palabra clave

**Ejemplos de búsqueda:**
- `q=chocolate` → Encuentra "Torta de Chocolate", "Brownie Chocolate", etc.
- `q=torta` → Encuentra todos los productos con "torta" en nombre o descripción
- `q=cumpleaños` → Encuentra productos de la categoría "Cumpleaños"

---

## 📊 GESTIÓN DE STOCK

### Validación Automática de Stock

**Descripción:**  
El sistema valida automáticamente el stock disponible al crear pedidos. Si un producto no tiene stock suficiente, el pedido no se puede crear.

**Flujo de stock en pedidos:**

1. **Al crear pedido (estado: Pendiente):**
   - Se valida que haya stock disponible
   - NO se descuenta el stock todavía
   - El pedido queda en estado "Pendiente"

2. **Al confirmar pedido (Pendiente → Confirmado):**
   - Se descuenta el stock automáticamente
   - Se marca el pedido como `stockDescontado = true`
   - El stock se reduce en la cantidad del pedido

3. **Al cancelar pedido:**
   - Si el stock fue descontado, se restaura automáticamente
   - El pedido pasa a estado "Cancelado"
   - Se registra la fecha de cancelación

**Alertas de stock bajo:**  
Cuando el stock de un producto es menor o igual al `stockMinimo`, el sistema lo marca como "stock bajo" para alertar al administrador.

---

## 🛒 GESTIÓN DE PEDIDOS

### 12. Cancelar Pedido (Cliente)

**Endpoint:** `PUT /api/pedidos/{id}/cancelar`  
**Autenticación:** ✅ Requerida  
**Roles:** Cliente (solo sus propios pedidos)  

**Descripción:**  
Permite a un cliente cancelar su propio pedido. Solo se pueden cancelar pedidos en estado "Pendiente". Si el stock ya fue descontado, se restaura automáticamente.

**Parámetros:**
- `id` (int): ID del pedido a cancelar

**Validaciones:**
- El pedido debe pertenecer al cliente autenticado
- El pedido debe estar en estado "Pendiente"
- No se pueden cancelar pedidos en otros estados

**Comportamiento:**
1. Valida que el pedido pertenezca al cliente
2. Valida que esté en estado "Pendiente"
3. Si el stock fue descontado, lo restaura
4. Cambia el estado a "Cancelado"
5. Registra la fecha de cancelación

**Respuesta:**  
Pedido actualizado con estado "Cancelado" y fecha de cancelación.

**Errores:**
- 404: Pedido no encontrado
- 400: El pedido no se puede cancelar (no está en estado Pendiente)
- 403: El pedido no pertenece al cliente autenticado

---

## 📋 RESUMEN DE FUNCIONALIDADES NUEVAS

### ✅ Sistema de Imágenes Múltiples
- Subir múltiples imágenes por producto
- Marcar una imagen como principal
- Ordenar imágenes para la galería
- Eliminación automática de archivos físicos
- Validación de formato y tamaño

### ✅ Gestión de Stock Inteligente
- Endpoint específico para actualizar stock
- Validación automática al crear pedidos
- Descuento automático al confirmar pedidos
- Restauración automática al cancelar
- Alertas de stock bajo

### ✅ Búsqueda de Productos
- Búsqueda por texto en nombre, descripción y categoría
- Case-insensitive
- Resultados cacheados para mejor rendimiento
- Endpoint público para el catálogo

### ✅ Cancelación de Pedidos
- Clientes pueden cancelar sus pedidos pendientes
- Restauración automática de stock
- Registro de fecha de cancelación
- Validaciones de seguridad

---

## 🔒 Seguridad y Validaciones

### Autenticación
- Todos los endpoints administrativos requieren JWT token
- Los tokens deben enviarse en el header: `Authorization: Bearer {token}`
- Los tokens expiran después de cierto tiempo

### Validaciones de Imágenes
- Solo formatos permitidos: jpg, jpeg, png, gif, webp
- Tamaño máximo: 5 MB por imagen
- Validación server-side (no confiar solo en frontend)
- Nombres únicos para evitar colisiones

### Validaciones de Stock
- Stock no puede ser negativo
- Validación antes de crear pedidos
- Prevención de sobreventa
- Restauración automática al cancelar

### Validaciones de Productos
- Nombre obligatorio (máx 200 caracteres)
- Precio debe ser mayor a 0
- Categoría obligatoria
- Stock y stock mínimo no negativos

---

## 🌐 URLs de Imágenes

### Formato de URL
Las imágenes se retornan con URL relativa: `/images/productos/{nombre_archivo}.jpg`

### Acceso a las imágenes
Para mostrar una imagen en el frontend:
```
URL completa = Base URL + URL de la imagen
Ejemplo: http://localhost:7097/images/productos/20240115120530_abc123.jpg
```

### Nombres de archivo
Los archivos se guardan con formato: `{timestamp}_{guid}.{extension}`
- Ejemplo: `20240115120530_a1b2c3d4e5f6a7b8.jpg`
- Garantiza nombres únicos
- Evita colisiones
- Permite rastrear fecha de creación

---

## 📊 Códigos de Respuesta HTTP

| Código | Significado | Cuándo ocurre |
|--------|-------------|---------------|
| 200 | OK | Operación exitosa (GET, PUT) |
| 201 | Created | Recurso creado exitosamente (POST) |
| 204 | No Content | Eliminación exitosa (DELETE) |
| 400 | Bad Request | Datos inválidos o validación fallida |
| 401 | Unauthorized | Token no proporcionado o inválido |
| 403 | Forbidden | Sin permisos para la operación |
| 404 | Not Found | Recurso no encontrado |
| 500 | Server Error | Error interno del servidor |

---

## 🚀 Próximas Funcionalidades

### En Desarrollo
- Método de pago en pedidos
- Confirmación de pago manual por admin
- Notificaciones por email
- Historial de cambios de estado

### Futuro
- Integración con MercadoPago
- Compresión automática de imágenes
- Migración a Cloudinary para imágenes
- Sistema de cupones y descuentos

---

**Documentación actualizada:** Enero 2024  
**Versión API:** 1.1  
**Sistema:** Softpan - Gestión Integral para Pastelerías

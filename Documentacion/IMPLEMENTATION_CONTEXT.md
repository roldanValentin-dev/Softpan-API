# CONTEXTO PARA IMPLEMENTACIÓN: CARRITO Y PAGO MERCADO PAGO

Este documento proporciona el contexto necesario para que otro agente guíe la implementación de dos características clave en el proyecto Softpan:
1. Persistencia del carrito de compras en el backend
2. Integración de Mercado Pago Checkout Pro para pagos

## 📋 VISIÓN GENERAL DEL PROYECTO

Softpan sigue una arquitectura limpia (Clean Architecture) con las siguientes capas:
- **Softpan.API**: Capa de presentación (controllers, middlewares)
- **Softpan.Application**: Casos de uso, servicios, DTOs, validators
- **Softpan.Domain**: Entidades, enums, interfaces de dominio
- **Softpan.Infrastructure**: Acceso a datos (repositorios, DbContext, migrations)
- **Softpan.Tests**: Pruebas unitarias

Tecnologías: .NET 8.0, Entity Framework Core, PostgreSQL, JWT, FluentValidation, Mapster, Serilog.

## 🛒 PARTE 1: PERSISTENCIA DEL CARRITO EN BACKEND

### Estado Actual Relevante
- Existe entidad `Pedido` en `Softpan.Domain.Entities.Pedido`
- Existe `EstadoPedidoEnum` en `Softpan.Domain.Enums.EstadoPedidoEnum` con valores: Pendiente(1), Confirmado(2), EnPreparacion(3), Listo(4), Entregado(5), Cancelado(6)
- Existe `PedidoService` en `Softpan.Application.Services.PedidoService` que maneja lógica de pedidos
- Existe `IPedidoRepository` y `PedidoRepository` para acceso a datos
- Ya hay endpoints en `PedidosController` para crear, obtener y cancelar pedidos

### Requisitos de Implementación
1. **Agregar estado "Carrito"** a `EstadoPedidoEnum`:
   ```csharp
   public enum EstadoPedidoEnum
   {
       // ... existentes
       Carrito = 7  // Nuevo valor
   }
   ```

2. **Modificar lógica de `PedidoService`**:
   - `CreatePedidoAsync`: NO crear directamente pedidos reales al agregar al carrito
   - Necesitamos nuevos métodos específicos para carrito:
     - `Task<CarritoDto> ObtenerOCrearCarritoAsync(string usuarioId)`
     - `Task<CarritoDto> AgregarItemAlCarritoAsync(string usuarioId, int productoId, int cantidad)`
     - `Task<CarritoDto> ActualizarItemEnCarritoAsync(string usuarioId, int productoId, int cantidad)`
     - `Task<bool> RemoverItemDelCarritoAsync(string usuarioId, int productoId)`
     - `Task<PedidoDto> ProcesarCheckoutDesdeCarritoAsync(string usuarioId, TipoPagoEnum tipoPago)`

3. **Crear DTOs específicos para carrito** (en `Softpan.Application.DTOs`):
   - `CarritoDto`: Representa el carrito con lista de items
   - `CarritoItemDto`: Item individual del carrito
   - Posiblemente actualizar `CreatePedidoDto` para usarlo en checkout

4. **Actualizar repositorios**:
   - Añadir métodos para consultar pedidos por estado `Carrito` y usuario
   - Considerar índices en base de datos para consultas eficientes de carritos activos

5. **Actualizar controladores**:
   - Crear nuevo `CarritoController` o añadir endpoints a `PedidosController`:
     - `GET /api/carrito` → Obtener carrito actual
     - `POST /api/carrito/items` → Agregar item
     - `PUT /api/carrito/items/{productoId}` → Actualizar cantidad
     - `DELETE /api/carrito/items/{productoId}` → Remover item
     - `POST /api/checkout/procesar` → Iniciar proceso de pago desde carrito

### Consideraciones Importantes
- **Validaciones críticas**: Antes de agregar/actualizar items en carrito, validar:
  - Producto existe y está activo (`Producto.Activo == true`)
  - Stock suficiente disponible (`Producto.Stock >= cantidad solicitada`)
  - NO descuentar stock todavía (solo al confirmar pago)
- **Persistencia**: El carrito debe sobrevivir a recargas de página y cambios de dispositivo
- **Limpieza**: Considerar estrategia para carritos abandonados (ej: eliminar después de 7 días)
- **Consistencia**: Reutilizar tanta lógica existente como sea posible (validaciones, mapeos, etc.)

### Archivos Examen Requeridos
Antes de implementar, el agente debería examinar:
- `Softpan.Domain.Entities.Pedido.cs`
- `Softpan.Domain.Enums.EstadoPedidoEnum.cs`
- `Softpan.Application.Services.PedidoService.cs`
- `Softpan.Application.Interfaces/IPedidoService.cs`
- `Softpan.Infrastructure.Repositories/IPedidoRepository.cs` y `PedidoRepository.cs`
- `Softpan.API.Controllers/PedidosController.cs`
- `Softpan.Application.DTOs/` (para entender estructura existente)

## 💳 PARTE 2: INTEGRACIÓN DE MERCADO PAGO CHECKOUT PRO

### Estado Actual Relevante
- Ya existe análisis detallado en `LOGICA_BACKEND_MERCADOPAGO.md`
- Existe enum `TipoPagoEnum` con Efectivo(1) y Transferencia(2) - necesitamos agregar MercadoPago
- Ya hay controladores de pagos (`PagosController`) pero enfocados en pagos manuales internos
- Ya se han identificado campos necesarios para agregar a entidad `Pedido`

### Requisitos de Implementación
1. **Actualizar `TipoPagoEnum`**:
   ```csharp
   public enum TipoPagoEnum
   {
       Efectivo = 1,
       Transferencia = 2,
       MercadoPago = 3  // Nuevo
   }
   ```

2. **Extender entidad `Pedido`** (en `Softpan.Domain.Entities.Pedido.cs`) con:
   ```csharp
   // Campos de pago existentes (del análisis previo)
   public TipoPagoEnum? TipoPago { get; set; }
   public EstadoPagoEnum EstadoPago { get; set; } = EstadoPagoEnum.Pendiente;
   public decimal? MontoConDescuento { get; set; }
   public string? ReferenciaTransaccion { get; set; }
   public DateTime? FechaPago { get; set; }
   
   // Campos específicos Mercado Pago (según documentación oficial)
   public string? MercadoPagoPreferenceId { get; set; }
   public string? MercadoPagoPaymentId { get; set; }
   public string? PaymentGateway { get; set; } // Siempre "mercadopago" para este flujo
   public string? PaymentStatus { get; set; } // Estado interpretado (approved, pending, rejected)
   public string? PaymentStatusDetail { get; set; } // Detalle específico de MP
   public DateTime? PaymentFechaActualizado { get; set; }
   ```

3. **Crear nuevo enum `EstadoPagoEnum`**:
   ```csharp
   public enum EstadoPagoEnum
   {
       Pendiente = 1,
       Pagado = 2,
       Fallido = 3,
       Reembolsado = 4
   }
   ```

4. **Crear servicio `MercadoPagoService`** (en `Softpan.Application.Services`):
   - Implementar `IMercadoPagoService` con métodos:
     ```csharp
     Task<string> CrearPreferenciaPagoAsync(CarritoDto carrito, string usuarioId);
     Task<PagoResultadoDto> ProcesarWebhookMercadoPago(string webhookJson, string webhookId);
     Task<EstadoPagoDto> ConsultarEstadoPago(string preferenceId);
     ```
   - Características críticas:
     - Leer credenciales (`ACCESS_TOKEN`, `CLIENT_SECRET`) SOLO de variables de entorno
     - Validar stock y precios desde backend ANTES de crear preferencia (nunca confiar en frontend)
     - Implementar verificación de firma de webhook (HMAC-SHA256 con `X-Signature`)
     - Manejar idempotencia (almacenar `webhook_id` procesados)
     - Consultar API de Mercado Pago para obtener estado real del pago en webhooks
     - Mapear estados de MP a nuestro `EstadoPagoEnum`

5. **Actualizar `PedidoService`**:
   - Modificar `CreatePedidoAsync` para aceptar `TipoPago` y manejar flujos diferencialmente:
     - Si Efectivo/Transferencia: crear pedido pendiente, aplicar 10% descuento, esperar confirmación manual
     - Si MercadoPago: crear preferencia mediante `MercadoPagoService`, guardar IDs, devolver `init_point`
   - Actualizar lógica de transición de estados: SOLO permitir cambiar a `Confirmado` cuando `EstadoPago == Pagado`

6. **Crear controlador `MercadoPagoController`**:
   ```csharp
   [ApiController]
   [Route("api/mercadopago")]
   public class MercadoPagoController : ControllerBase
   {
       private readonly IMercadoPagoService _mercadoPagoService;
       
       [HttpPost("crear-preferencia")]
       public async Task<IActionResult> CrearPreferencia([FromBody] CarritoDto carrito) 
       
       [HttpPost("webhook")]
       [AllowAnonymous] // Webhooks vienen de Mercado Pago sin auth
       public async Task<IActionResult> RecibirWebhook([FromBody] object webhookData)
   }
   ```

7. **Actualizar `PedidosController`**:
   - Modificar endpoint `Create` para aceptar `TipoPago` y delegar a servicio actualizado
   - Asegurar que cualquier intento de confirmar pedido verifique que el pago esté pagado

### Consideraciones de Seguridad Críticas (NO OPCIONALES)
1. **Credenciales**:
   - NUNCA en código, appsettings.json, o repositorio
   - SOLO en variables de entorno del servidor de alojamiento
   - En desarrollo: usar `launchSettings.json` o variables de entorno local (NO comprometidas)

2. **Validación de Webhooks**:
   - Verificar firma HMAC-SHA256 usando header `X-Signature`
   - Implementar idempotencia (procesar cada webhook exactamente una vez)
   - Rechazar inmediatamente notificaciones sin firma válida

3. **Validación de Datos**:
   - NUNCA confiar en precios, cantidades o productos enviados desde frontend para crear preferencia
   - Siempre validar contra base de datos en el backend
   - Verificar que `external_reference` en preferencia corresponda a un pedido real de nuestro sistema

4. **Principio de Mínimo Privilegio**:
   - Credenciales de Mercado Pago deberían tener permisos limitados a:
     - Creación de preferencias de pago
     - Consulta de pagos específicos
   - Sin permisos para reembolsos, creación de clientes, etc. (a menos que sea estrictamente necesario y justificado)

### Archivos Examen Requeridos
Antes de implementar, el agente debería examinar:
- `LOGICA_BACKEND_MERCADOPAGO.md` (documento de referencia existente)
- `Softpan.Domain.Enums.TipoPagoEnum.cs`
- `Softpan.Domain.Entities.Pedido.cs`
- `Softpan.Application.Services.PedidoService.cs`
- `Softpan.API.Controllers/PagosController.cs` (para entender estructura existente)
- `Softpan.API/Program.cs` (para ver cómo se manejan credenciales y middlewares actualmente)
- Documentación oficial de Mercado Pago consultada anteriormente (endpoints, formato de webhooks, etc.)

## 🔄 FLUJO DE TRABAJO RECOMENDADO

### Para Carrito:
1. Usuario agrega producto → `POST /api/carrito/items` → backend valida stock/precio → crea/actualiza Pedido(Estado=Carrito)
2. Usuario ve carrito → `GET /api/carrito` → backend devuelve Pedido con Estado=Carrito y sus items
3. Usuario modifica cantidad → `PUT /api/carrito/items/{id}` → backend valida nuevo stock → actualiza
4. Usuario inicia checkout → `POST /api/checkout/procesar` → backend:
   - Valida una última vez stock/precios
   - Si TipoPago=Efectivo/Transferencia: crea registro de pago pendiente
   - Si TipoPago=MercadoPago: llama a MercadoPagoService para crear preferencia
   - Devuelve datos necesarios al frontend (init_point para MP o instrucciones para efectivo)

### Para Mercado Pago:
1. Frontend recibe `init_point` → redirige a formulario de pago de Mercado Pago
2. Usuario completa pago en entorno seguro de Mercado Pago
3. Mercado Pago envía webhook a `POST /api/mercadopago/webhook`
4. Backend:
   - Valida firma del webhook (CRÍTICO)
   - Verifica idempotencia
   - Extrae `payment_id` y consulta estado real en API de MP
   - Actualiza Pedido correspondiente:
     - Si `approved`: EstadoPago=Pagado → EstadoPedido=Confirmado → descuenta stock
     - Si `rejected`/`cancelled`: EstadoPago=Fallido → mantiene EstadoPedido=Carrito/Pendiente
     - Si `pending`/`in_process`: mantiene estados pendientes
5. Frontend (opcional): Puede hacer polling a `/api/estado-pago/{preferenceId}` o confiar en redirect URLs

## 🎯 PUNTOS DE VALIDACIÓN Y PRUEBAS

Antes de considerar la implementación completa, asegurar:
1. **Carrito**:
   - [ ] Se puede agregar item al carrito (validando stock)
   - [ ] No se puede agregar más items de los que hay en stock
   - [ ] El carrito persiste entre recargas de página
   - [ ] Se puede modificar cantidad en carrito
   - [ ] Se puede eliminar items del carrito
   - [ ] El checkout desde carrito funciona para ambos tipos de pago

2. **Mercado Pago**:
   - [ ] Se puede crear preferencia de pago correctamente (con datos validados desde backend)
   - [ ] El webhook de pago aprobado se procesa correctamente (firma válida)
   - [ ] El webhook de pago rechazado se procesa correctamente
   - [ ] El stock solo se descuenta cuando el pago está aprobado
   - [ ] Los webhooks duplicados no causan efectos secundarios (idempotencia)
   - [ ] Las credenciales nunca aparecen en logs o código fuente

3. **Seguridad**:
   - [ ] Credenciales de Mercado Pago solo en variables de entorno
   - [ ] Webhook endpoint verifica firma HMAC-SHA256
   - [ ] Validación de idempotencia implementada
   - [ ] Ningún dato sensible (números de tarjeta, etc.) se almacena en nuestra BD

## 📦 PRÓXIMOS PASOS SUGERIDOS PARA EL AGENTE

1. **Primero**: Implementar persistencia de carrito en backend (menos riesgoso, reutiliza lógica existente)
2. **Segundo**: Implementar integración de Mercado Pago (depende de tener el carrito funcionando)
3. **En cada paso**:
   - Examinar los archivos listados en las secciones correspondiente
   - Hacer cambios incrementales
   - Escribir pruebas unitarias para la lógica nueva
   - Probar manualmente los flujos completos
   - Verificar que no se rompa funcionalidad existente

> **Nota importante**: Este contexto está diseñado para que otro agente pueda proporcionar orientación específica de código sin que yo tenga que escribir la implementación directamente, ahorrando tokens según lo solicitado. El agente deberá usar este documento como referencia para guiar al usuario mediante explicaciones, sugerencias de estructura de código, y referencias a archivos específicos que deben modificarse.

---
*Documento generado para facilitar la delegación de implementación a otro agente especializado en guía de código.*
# Integración Completa del Módulo de Pago en Softpan

**Objetivo**: Implementar un módulo de pago seguro y robusto que permita procesar pagos mediante Efectivo/Transferencia (con 10% de descuento, requiere verificación manual) y Mercado Pago (confirmación automática mediante webhook, sin descuento), asegurando que el stock solo se descuente cuando el pago esté confirmado.

---

## 📋 Resumen de Cambios

| Capa | Archivo / Componente | Cambios Principales |
|------|----------------------|---------------------|
| **Dominio** | `Softpan.Domain.Entities.Pedido` | Agregar campos de pago: `TipoPago`, `EstadoPago`, `MontoConDescuento`, `ReferenciaTransaccion`, `FechaPago`, `MercadoPagoPreferenceId`, `MercadoPagoPaymentId`, `PaymentGateway`, `PaymentStatus`, `PaymentStatusDetail`, `PaymentFechaActualizado` |
| | `Softpan.Domain.Enums.EstadoPagoEnum` | Nuevo enumerado: `Pendiente`, `Pagado`, `Fallido`, `Reembolsado` |
| | `Softpan.Domain.Enums.TipoPagoEnum` | Agregar valor `MercadoPago = 3` |
| **Aplicación** | DTOs | `CreatePagoDto`, `PagoDto`, `MercadoPagoPreferenceRequestDto`, `MercadoPagoWebhookDto`, actualizar `CreatePedidoDto` para incluir `TipoPago` |
| | Interfaces | `IMercadoPagoService`, actualizar `IPedidoService` con métodos de procesar pago |
| | Servicios | `MercadoPagoService` (implementa `IMercadoPagoService`), actualizar `PedidoService` para lógica de pago y transición de estados |
| | Validators | Nuevos validators para DTOs de pago |
| **Infraestructura** | Repositorios | Actualizar `IPedidoRepository` y `PedidoRepository` para mapear nuevos campos y agregar índices |
| | DependencyInjections | Registrar `IMercadoPagoService` y configurar SDK de Mercado Pago |
| | Migrations | Migración EF para agregar columnas a tabla `Pedidos` |
| **Presentación** | Controladores | `MercadoPagoController` (crear preferencia, recibir webhook), actualizar `PedidosController` (endpoint procesar pago para efectivo/transferencia, ajustar lógica de creación) |
| | Middleware / Program.cs | Verificación de firma de webhook MP, rate limiting, lectura de credenciales desde variables de entorno |
| | Manejo de Errores | Filtros de excepción específicos para pagos, logging estructurado sin datos sensibles |

---

## 🛠️ Lista Detallada de Tareas

### 1. Dominio
- [ ] Crear/actualizar `EstadoPagoEnum.cs` en `Softpan.Domain.Enums`
- [ ] Actualizar `TipoPagoEnum.cs` añadiendo `MercadoPago`
- [ ] Modificar `Pedido.cs`:
  - Añadir propiedades de pago listadas arriba
  - Implementar método `AplicarDescuentoEfectivoTransferencia()`
  - Implementar método `EsPagoConfirmado()`
  - Actualizar `PuedaCancelarse()` para considerar estado de pago

### 2. Aplicación – DTOs
- [ ] Crear `CreatePagoDto` con `TipoPago`, `ReferenciaTransaccion` (opcional), `Observaciones`
- [ ] Crear `PagoDto` con todos los campos de tracking
- [ ] Crear `MercadoPagoPreferenceRequestDto` (items, datos de pagador, URLs de retorno)
- [ ] Crear `MercadoPagoWebhookDto` (mapear payload de MP)
- [ ] Actualizar `CreatePedidoDto` para incluir `TipoPago` y campos de pago inicial
- [ ] Actualizar `PedidoDto` para incluir nuevos campos de pago

### 3. Aplicación – Interfaces
- [ ] Crear `IMercadoPagoService.cs` con métodos:
  - `Task<string> CrearPreferenciaPagoAsync(CarritoDto carrito, string usuarioId)`
  - `Task<PagoResultadoDto> ProcesarWebhookMercadoPago(string webhookJson)`
  - `Task<EstadoPagoDto> ConsultarEstadoPago(string preferenceId)`
- [ ] Actualizar `IPedidoService.cs`:
  - Añadir `Task<PedidoDto> ProcesarPagoPedidoAsync(int pedidoId, TipoPagoEnum tipoPago)`
  - Modificar `CreatePedidoAsync` para aceptar datos de pago inicial

### 4. Aplicación – Servicios
- [ ] Implementar `MercadoPagoService.cs`:
  - Inyección de `IConfiguration` para leer credenciales de variables de entorno
  - Método para crear preferencia (validar stock/precios desde repositorios de productos)
  - Método para procesar webhook:
    - Verificar firma `X-Signature` usando secreto configurado
    - Verificar idempotencia (almacenar `webhook_id` procesados en tabla nueva o caché)
    - Consultar API de MP para obtener estado real del pago
    - Mapear estado MP a `EstadoPagoEnum` y actualizar pedido mediante `IPedidoRepository`
    - Trigger de acciones posteriores (email, notificaciones) si es aprobado
  - Método para consultar estado de pago (reutilizar lógica de webhook)
- [ ] Actualizar `PedidoService.cs`:
  - En `CreatePedidoAsync`:
    - Si `TipoPago` es Efectivo/Transferencia: aplicar 10% descuento, establecer `EstadoPago = Pendiente`, `EstadoPedido = Pendiente`
    - Si `TipoPago` es MercadoPago: crear preferencia mediante `IMercadoPagoService`, guardar `preference_id`, devolver `init_point` al frontend (no cambiar estado todavía)
  - Implementar `ProcesarPagoPedidoAsync`:
    - Validar que pedido pertenezca al usuario
    - Si tipo es Efectivo/Transferencia: establecer `EstadoPago = Pagado`, `FechaPago = Ahora`, `ReferenciaTransaccion` (proporcionada por admin)
    - Llamar a repositorio para actualizar pedido
    - Si `EstadoPago == Pagado` entonces cambiar `EstadoPedido` a `Confirmado` y descontar stock (reutilizar lógica existente)
  - Asegurar que cualquier transición a `EstadoPedidoEnum.Confirmado` verifique previamente `EstadoPago == Pagado`

### 5. Aplicación – Validators
- [ ] Crear validador para `CreatePagoDto` (FluentValidation)
- [ ] Crear validador para `MercadoPagoPreferenceRequestDto`
- [ ] Actualizar validador de `CreatePedidoDto` para validar `TipoPago` y lógica de descuento

### 6. Infraestructura – Repositorios
- [ ] Actualizar `IPedidoRepository.cs`:
  - Añadir métodos `GetByMercadoPagoPreferenceId(string preferenceId)`
  - Añadir método `UpdatePagoFields(int pedidoId, ...)` o usar `UpdateAsync` general
- [ ] Actualizar `PedidoRepository.cs`:
  - Mapear nuevas propiedades en configuración de Entity (se hace automáticamente si propiedades existen, pero revisar)
  - Añadir índices sugeridos en `OnModelCreating`:
    ```csharp
    builder.HasIndex(p => p.MercadoPagoPreferenceId).IsUnique(false);
    builder.HasIndex(p => p.ReferenciaTransaccion).IsUnique(false);
    ```
- [ ] (Opcional) Crear repositorio simple para almacenar `webhook_id` procesados si no se quiere usar tabla existente

### 7. Infraestructura – Dependency Injections
- [ ] En `Softpan.Infrastructure.DependencyInjections.cs`:
  - Registrar `IMercadoPagoService` como `Scoped`
  - Agregar configuración para leer `MercadoPago:AccessToken` y `MercadoPago:ClientSecret` de `IConfiguration`
  - (Si se crea repositorio de webhook ids, registrarlo también)

### 8. Infraestructura – Migraciones
- [ ] Ejecutar en terminal:
  ```bash
  dotnet ef migrations add AddPagoFieldsToPedido --project Softpan.Infrastructure --startup-project Softpan.API
  ```
- [ ] Revisar archivo de migración generado para asegurarse de que las columnas sean correctas (tipos, nullable, defaults)
- [ ] Aplicar migración:
  ```bash
  dotnet ef database update --project Softpan.Infrastructure --startup-project Softpan.API
  ```

### 9. Presentación – Controladores
- [ ] Crear `MercadoPagoController.cs`:
  - `[HttpPost("crear-preferencia")]`: recibe `MercadoPagoPreferenceRequestDto`, valida carrito mediante repositorios de productos, llama a `IMercadoPagoService.CrearPreferenciaPagoAsync`, devuelve `init_point`
  - `[HttpPost("webhook")]`: `[AllowAnonymous]`, lee cuerpo de request, llama a `IMercadoPagoService.ProcesarWebhookMercadoPago`, devuelve `200 OK` si se procesó correctamente
- [ ] Modificar `PedidosController.cs`:
  - En `Create` (`POST /api/pedidos`): aceptar `TipoPago` y campos de pago, llamar a servicio actualizado
  - Añadir `[HttpPost("{id}/procesar-pago")]`: autorizado, llama a `PedidoService.ProcesarPagoPedidoAsync` para efectivo/transferencia
  - Asegurar que endpoints que cambian estado a `Confirmado` verifiquen que el pago esté pagado (puede ser mediante filtro o validación en service)

### 10. Presentación – Program.cs (Seguridad y Middleware)
- [ ] Lectura de credenciales:
  ```csharp
  var mpAccessToken = builder.Configuration["MercadoPago:AccessToken"];
  var mpClientSecret = builder.Configuration["MercadoPago:ClientSecret"];
  ```
  (Si no están en appsettings.json, asegurarse de que estén en variables de entorno del servidor)
- [ ] Configurar rate limiting específico para endpoints de pago (usando paquete AspNetCoreRateLimit o similar)
- [ ] Agregar middleware de verificación de firma para ruta `/api/mercadopago/webhook`:
  - Calcular HMAC-SHA256 del cuerpo con `mpClientSecret`
  - Comparar con header `X-Signature` (formato `ts={timestamp},v1={hash}`)
  - Rechazar si no coincide
- [ ] Asegurar que logs no incluyan datos sensibles (filtrar propiedades de DTOs de pago en Serilog si es necesario)

### 11. Logging y Monitoreo
- [ ] Configurar Serilog para enriquecer logs con `PedidoId`, `UsuarioId` cuando esté disponible
- [ ] Crear métricas básicas (contadores de pagos exitosos, fallidos, webhooks recibidos) usando `System.Diagnostics.Metrics` o librería como Prometheus.Net si se usa
- [ ] (Opcional) Agregar health check para verificar conectividad con API de Mercado Pago

### 12. Pruebas
- [ ] Pruebas unitarias:
  - `PedidoServiceTests`: crear pedido con cada tipo de pago, validar descuento y estados
  - `MercadoPagoServiceTests`: mock de HTTP para crear preferencia, procesar webhook (éxito, fallo, pending)
  - Validadores de DTOs
- [ ] Pruebas de integración:
  - Usar `WebApplicationFactory` para probar endpoints completos:
    - Crear pedido Efectivo → procesar pago manual → verificar stock descontado solo después de confirmación
    - Crear pedido MercadoPago → llamar endpoint crear-preferencia → simular webhook aprobado → verificar que pedido pasa a Confirmado y stock descontado
- [ ] Pruebas de seguridad:
  - Intentar enviar webhook con firma inválida → debe rechazar
  - Intentar replay de mismo webhook → debe ser idempotente (segundo llamado no cambia estado)

### 13. Documentación
- [ ] Actualizar `README.md` sección de características y roadmap
- [ ] Añadir sección en documentación de API (Swagger) con nuevos endpoints y notas de seguridad
- [ ] (Opcional) Crear `PAGO_INTEGRACION_GUIA.md` con instrucciones de despliegue y variables de entorno requeridas

---

## 📌 Variables de Entorno Necesarias

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `MercadoPago:AccessToken` | Access Token de MP (producción o sandbox) | `APP_USR-...` |
| `MercadoPago:ClientSecret` | Secret para firmar webhooks | `sandbox-...` |
| `MercadoPago:Mode` | `sandbox` o `production` | `sandbox` |
| `WebhookIdsTableName` (opcional) | Nombre de tabla para almacenar webhook ids procesados (si se usa tabla propia) | `MercadoPagoWebhookIds` |

> **Importante**: Nunca colocar estas variables en `appsettings.json` ni en el repositorio. Configurarlas en el entorno de hosting (Azure App Service, Docker secrets, variables de sistema, etc.).

---

## 🚀 Próximos Pasos (Checklist de Ejecución)

1. **Crear rama de feature**: `git checkout -b feature/integracion-pago-robusta`
2. **Implementar cambios de dominio** (enums y entidad Pedido)
3. **Crear/actualizar DTOs, interfaces y servicios**
4. **Implementar MercadoPagoService** con pruebas unitarias de mock
5. **Actualizar PedidoService** con lógica de pago y transición de estados
6. **Crear/actualizar repositorios y aplicar migraciones**
7. **Crear MercadoPagoController y modificar PedidosController**
8. **Configurar Program.cs** (credenciales, verificación de webhook, rate limiting)
9. **Escribir pruebas unitarias y de integración**
10. **Ejecutar migración en base de datos de desarrollo**
11. **Probar manualmente flujos completos** (efectivo/transferencia y Mercado Pago)
12. **Revisar seguridad** (ningún dato sensible en logs, credenciales fuera de código)
13. **Mergear a main** tras aprobación y pruebas en staging
14. **Deploy a producción** y monitorear durante primera semana

---

## ✅ Criterios de Finalización

- [ ] Todos los tipos de pago (Efectivo, Transferencia, Mercado Pago) funcionan según especificación.
- [ ] El stock solo se descuenta cuando el pago está confirmado (`EstadoPago == Pagado`).
- [ ] Las credenciales de Mercado Pago nunca aparecen en código, contenedores o logs.
- [ ] Los webhooks de Mercado Pago son verificados por firma y procesados exactamente una vez (idempotencia).
- [ ] Los endpoints de pago tienen rate limiting y manejo de errores apropiado.
- [ ] Se han escrito pruebas unitarias que cubren al menos 80% de la lógica nueva.
- [ ] La documentación (README y Swagger) refleja los nuevos endpoints y su uso.
- [ ] Se ha realizado un despliegue en staging sin incidencias y se ha validado con datos reales de prueba.

---

> **Nota**: Este plan asume que eres el único desarrollador (backend y frontend). Ajusta el ritmo según tu capacidad, pero no omitir pasos de seguridad y pruebas. Cada cambio debe compilarse y pasar los tests antes de continuar al siguiente.

---
*Documento generado para guiar la implementación completa y robusta del módulo de pago en Softpan.*
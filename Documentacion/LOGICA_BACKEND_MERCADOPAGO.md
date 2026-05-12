# LÓGICA DE INTEGRACIÓN DE MERCADOPAGO EN BACKEND
# Tienda Online Panadería - Enfoque Conceptual

## 🔒 PRINCIPIO FUNDAMENTAL: SEGURIDAD POR DISEÑO
La integración de MercadoPago **DEBE** implementarse principalmente en el backend por razones críticas de seguridad:
- **Nunca exponer credenciales**: El `ACCESS_TOKEN` y `CLIENT_SECRET` de MercadoPago nunca deben estar en el frontend
- **Prevención de fraude**: El frontend puede ser manipulado; solo el backend puede validar auténticamente los pagos
- **Cumplimiento PCI DSS**: El manejo directo de datos de pago requiere backend seguro
- **Fuente única de verdad**: El backend debe ser la autoridad final sobre el estado de pago

## 🎯 RESPONSABILIDADES CENTRALES DEL BACKEND

### 1. CREACIÓN SEGURA DE PREFERENCIA DE PAGO
**Lógica**: 
- Recibir solicitud del frontend con datos del carrito (items, cantidades, precio total)
- Validar que los productos existan, estén activos y tengan stock suficiente
- Calcular el monto total basado en precios actuales del backend (no confiar en frontend)
- Construir el objeto de preferencia para MercadoPago incluyendo:
  - Items con descripción, cantidad, precio unitario
  - Información del pagador (email, nombre desde auth)
  - URLs de retorno (success, failure, pending)
  - Referencia interna (ID de pedido temporal o carrito)
  - Metadata para tracking interno
- Llamar a API de MercadoPago usando credenciales almacenadas de forma segura (variables de entorno)
- Recibir y almacenar el `init_point` y `preference_id` generados
- Asociar el `preference_id` con el carrito/pedido pendiente en base de datos
- Devolver únicamente el `init_point` al frontend para redirección

### 2. PROCESAMIENTO DE WEBHOOKS/NOTIFICACIONES
**Lógica**:
- Exponer endpoint seguro (`/webhooks/mercadopago`) para recibir notificaciones de MercadoPago
- Verificar autenticidad de cada notificación:
  - Validar firma usando `X-Signature` header y secret configurado
  - Confirmar que el `webhook_id` no haya sido procesado previamente (idempotencia)
- Extraer datos relevantes de la notificación:
  - `type`: tipo de evento (payment, merchant_order, etc.)
  - `data.id`: ID del recurso afectado
- Para eventos de pago (`payment.updated` o similare):
  - Recuperar el `payment_id` del `data.id`
  - Consultar API de MercadoPago: `GET /v1/payments/{payment_id}` usando credenciales backend
  - Validar que el pago corresponda a nuestra preferencia mediante `external_reference` o metadata
  - Determinar el estado final del pago:
    - `approved`: Pago exitoso
    - `rejected`: Pago rechazado (revisar `status_detail` para causa)
    - `pending`: Pago pendiente de confirmación
    - `in_process`: En proceso de pago
    - `cancelled`: Pago cancelado
  - Actualizar estado del pedido en base de datos según resultado
  - Trigger de acciones posteriores según estado:
    - Si `approved`: Confirmar pedido, enviar email de confirmación, iniciar proceso de preparación
    - Si `rejected`/`cancelled`: Notificar fallo, mantener carrito disponible, permitir reintento
    - Si `pending`: Mantener estado de espera, notificar al usuario que está en revisión

### 3. CONSULTA DE ESTADO DE PAGO (PARA RECUPERACIÓN Y POLLING OPCIONAL)
**Lógica**:
- Proveer endpoint para consultar estado de pago asociado a una preferencia o pedido
- Utilizar internamente la misma lógica de verificación que los webhooks:
  - Obtener `payment_id` asociado desde base de datos
  - Consultar estado actual en MercadoPago API
  - Devolver estado interpretado (no crudo de MercadoPago) al frontend
- Implementar correctamente el manejo de casos donde el pago aún no existe en MercadoPago
- Respetar límites de tasa de consulta para evitar bloqueo por MercadoPago

### 4. GESTIÓN DE ERRORES Y EXCEPCIONES
**Lógica de manejo de errores**:
- Errores de comunicación con MercadoPago:
  - Reintentos con backoff exponencial para errores transitorios (5xx, timeout)
  - Fallback a estado manual para errores persistentes que requieran intervención
- Errores de validación de negocio:
  - Devolver códigos de error claros al frontend sin exponer detalles internos
  - Loggear suficientemente para auditoría sin datos sensibles
- Escenarios de inconsistencia:
  - Diferencia entre estado en MercadoPago y estado en nuestra BD
  - Pagados no notificados por webhook (resolved by polling de reconciliación periódica)
  - Webhooks duplicados o fuera de orden (manejado por idempotencia y estado actual)

### 5. INTEGRACIÓN CON SISTEMA DE PEDIDOS EXISTENTE
**Lógica de coordinación**:
- Mantener compatibilidad con flujo existente de `PedidoService`
- Extender modelo de pedido con campos específicos de MercadoPago:
  - `mercadopago_preference_id`: ID de preferencia usado
  - `mercadopago_payment_id`: ID de pago en MercadoPago (si aplicable)
  - `payment_gateway`: Siempre "mercadopago" para este flujo
  - `payment_status`: Estado interpretado (approved, pending, rejected, etc.)
  - `payment_status_detail`: Detalle específico de MercadoPago
  - `payment_fecha_actualizado`: Timestamp de última actualización de estado
- Flujo de creación de pedido modificado:
  1. Frontend solicita creación de preferencia
  2. Backend valida carrito y crea registro de pedido pendiente con estado `payment_pending`
  3. Backend crea preferencia en MercadoPago y asocia ID
  4. Frontend redirige a MercadoPago usando `init_point`
  5. MercadoPago procesa pago y envía webhook
  6. Backend actualiza pedido con resultado final
  7. Si aprobado: transición a estado `confirmed` y activación de flujo normal
  8. Si rechazado/fallback: transición a estado `payment_failed` y notificación

## 🔄 FLUJO LÓGICO COMPLETO (PERSPECTIVA DE BACKEND)

**Fase 1: Inicialización**
```
[Frontend] → POST /api/crear-preferencia-pago {carrito_data}
          ← Validar carrito y precios [Backend]
          ← Calcular total seguro [Backend]
          ← CREAR PREFERENCIA EN MP [Backend→MercadoPago]
          ← Guardar preference_id asociado a carrito [Backend→BD]
          ← Devolver init_point [Backend]
[Frontend] ← Redirección a init_point
```

**Fase 2: Procesamiento de Pago (MercadoPago Hosted)**
```
[Usuario] → Completa pago en MercadoPago
[MercadoPago] → Envío webhook a /webhooks/mercadopago
[Backend] ← Verificar firma webhook [Security]
[Backend] ← Verificar no duplicado (idempotencia) [BD]
[Backend] ← Obtener payment_id del webhook [MercadoPago]
[Backend] ← CONSULTAR ESTADO REAL PAGO [Backend→MercadoPago]
[Backend] ← Determinar estado final aprobado/rejected/etc [Logic]
[Backend] ← ACTUALIZAR ESTADO PEDIDO EN BD [Backend→BD]
[Backend] ← TRIGGER ACCIONES POSTERIORES [Logic]
```

**Fase 3: Confirmación al Usuario**
```
[MercadoPago] → Redirección a success_url/failure_url
[Frontend] ← Muestra mensaje basado en retorno URL
[Frontend] ← OPCIONAL: Polling a /api/estado-pago {preference_id}
[Backend] ← Devolver estado interpretado [Backend]
[Frontend] ← Actualiza UI según estado
```

## 🛡️ CONSIDERACIONES DE SEGURIDAD CLAVE

**Protección de credenciales**:
- `ACCESS_TOKEN` y `CLIENT_SECRET` almacenados exclusivamente en variables de entorno del servidor
- Nunca incluidos en código fuente, contenedores o registros de despliegue
- Rotación periódica según políticas de seguridad
- Acceso restringido solo a servicios que realmente los necesitan

**Validación de integridad**:
- Todos los webhooks verificados mediante firma HMAC-SHA256
- Rechazo inmediato de notificaciones sin firma válida o incorrecta
- Validación de que el `external_reference` o metadata corresponda a un recurso interno legítimo
- Prevención de ataques de repetición mediante control de idempotencia (almacenamiento de `webhook_id` procesados)

**Principio de mínimo privilegio**:
- Credenciales de MercadoPago con permisos limitados únicamente a:
  - Creación de preferencias de pago
  - Consulta de pagos específicos
  - No permisos para reembolsos, creación de clientes, etc. (a menos que sea necesario y esté justificado)

**Manejo de datos sensibles**:
- Nunca almacenar números de tarjeta, CVV u otros datos sensibles de pago
- Solo almacenar identificadores de transacción proporcionados por MercadoPago
- Los datos de tarjeta nunca tocan nuestros sistemas (PCI DSS SAQ-A aplicable mediante uso de checkout hosted)

## ⚙️ CONSIDERACIONES OPERACIONALES

**Idempotencia**:
- Cada webhook procesado exactamente una vez (almacenar `webhook_id` procesados)
- Operaciones de actualización de estado diseñadas para ser idempotentes
- Reconciliación periódica para detectar y corregir inconsistencias

**Monitoreo y alertas**:
- Logs estructurados de todas las interacciones con MercadoPago
- Métricas de:
  - Tiempo de respuesta de API de MercadoPago
  - Tasa de éxito/fallo de pagos
  - Webhooks recibidos vs procesados
  - Errores de autenticación o firma
- Alertas para:
  - Fallos repetidos en comunicación con MercadoPago
  - Tasas de rechazo inusualmente altas
  - Webhooks sin firma o con firma inválida

**Escalabilidad y rendimiento**:
- Operaciones de backend optimizadas para ser rápidas (validación mínima necesaria antes de llamar a MP)
- Uso de caching apropiado para datos de productos que no cambian frecuentemente
- Diseño para manejar picos de carga durante promociones o horarios pico
- Tiempo de espera configurado adecuadamente para llamadas a MercadoPago (ni demasiado corto ni excesivo)

**Manejo de edge cases**:
- Pagos en estado `in_process` por períodos prolongados (tiempo límite definido para considerar abandonado)
- Pagos que cambian de estado múltiples veces (solo procesar transición final relevante)
- Inconsistencias entre monto en carrito y monto pagado (validar y manejar según política de negocio)
- Webhooks recibidos fuera de orden cronológico (reconstruir estado basado en consulta directa)

## 📈 INTEGRACIÓN CON FLUJO EXISTENTE DE PEDIDOS

**Compatibilidad hacia atrás**:
- Los pedidos creados antes de la integración de MercadoPago continúan funcionando normalmente
- Nuevos pedidos usan el flujo mejorado pero mantienen la misma estructura de datos esencial
- Servicios existentes como `getMisPedidos()`, `getPedidoById()` continúan funcionando sin cambios

**Extensión mínima del modelo de pedido**:
- Adición de 5-6 campos específicos de pasarela de pago (como se detalló anteriormente)
- Sin cambios en tablas existentes que puedan romper migraciones
- Índices adecuados en nuevos campos para consultas eficientes (ej: por `mercadopago_payment_id`)

**Transiciones de estado claras**:
- Estados de pago bien definidos y documentados
- Transiciones permitidas claramente establecidas (estado máquina)
- Notificaciones automáticas de cambio de estado cuando sea apropiado (email, in-app)
- Historial de cambios de estado para auditoría y servicio al cliente

## ✅ BENEFICIOS DE ESTE ENFOQUE

**Seguridad**:
- Eliminación total de exposición de credenciales en frontend
- Validación independiente y confiable del estado de pago
- Cumplimiento con estándares de la industria para procesamiento de pagos

**Confiabilidad**:
- Fuente única de verdad para estado de pago
- Manejo robusto de fallos de red y servicios externos
- Recuperación automática de estados inconsistentes

**Experiencia de Usuario**:
- Proceso de pago fluido y profesional
- Feedback claro basado en estado real de pago
- Posibilidad de mejorar UX con polling opcional (sin comprometer seguridad)
- Mensajes de error precisos y útiles para el usuario

**Mantenibilidad**:
- Separación clara de responsabilidades (backend = lógica crítica, frontend = presentación)
- Facilidad para probar y validar lógicamente cada componente
- Estructura que permite agregar otras pasarelas de pago en el futuro siguiendo el mismo patrón
- Facilidad para auditoría y cumplimiento regulatorio

Este enfoque lógico asegura que la integración de MercadoPago sea segura, confiable y mantenga la integridad del proceso de pago mientras brinda una experiencia de usuario profesional y fluida.

---
*Documento lógico para implementación de backend - MercadoPago*
*Fecha: 8 de mayo de 2026*
*Enfoque: Responsabilidades, flujos y consideraciones de seguridad - Sin código*
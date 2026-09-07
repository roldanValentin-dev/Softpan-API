# Propuesta de sistemas de pedidos online para pastelería

## Modelos de negocio analizados

---

## Modelo A: Todo junto (fecha única)

**Cómo funciona:**
- Cada producto tiene un tiempo de preparación en días
- Ej: shots en 0 días, torta en 3 días
- Si el cliente compra shots + torta, el pedido completo se entrega cuando esté **todo listo** (día 3)
- Las shots se producen el día antes de la entrega

**Casos de uso:**
- Cliente pide solo shots → las recibe hoy mismo
- Cliente pide solo torta → la recibe en 3 días
- Cliente pide shots + torta → recibe todo junto en 3 días

**Lo bueno:**
- Un solo pedido, simple
- Cero desperdicio (se produce contra pedido)
- La pastelería programa toda la producción sabiendo cuándo entregar

**Lo malo:**
- Si el cliente quería las shots para hoy, no puede porque "esperan" a la torta

---

## Modelo B: Entregas separadas (split)

**Cómo funciona:**
- El cliente paga **una sola vez**
- El sistema separa automáticamente: lo que está listo hoy se entrega hoy, lo que necesita días se agenda
- Ej: shots se retiran hoy, torta se entrega el sábado

**Casos de uso:**
- Cliente pide shots (hoy) + torta (3 días) → recibe shots hoy, torta el sábado
- Cliente pide solo torta (3 días) → recibe el sábado (es un solo pedido, no hay split)
- Cliente pide 3 productos con el mismo tiempo → todo junto, un solo pedido

**Lo bueno:**
- El cliente tiene lo urgente hoy y lo demás cuando corresponde
- Cada producto con su fecha justa
- Cero desperdicio

**Lo malo:**
- Más complejo: el sistema genera 2 pedidos, el admin ve 2 pedidos, hay que vincularlos al mismo pago

---

## Modelo C: Solo productos disponibles (sin encargos en la web)

**Cómo funciona:**
- En la web solo se vende lo que ya está listo o se hace en el día
- Los encargos (tortas personalizadas) se muestran pero con un cartel que dice "Consultar por WhatsApp/Instagram"
- Todo se retira/envía hoy

**Casos de uso:**
- Cliente ve shots, macarons, alfajores → compra y retira hoy
- Cliente quiere torta personalizada → te escribe por WhatsApp

**Lo bueno:**
- Súper simple, se implementa rápido
- No hay que manejar fechas de entrega
- Ideal para arrancar

**Lo malo:**
- Perdés la venta online de tortas personalizadas
- El cliente tiene que contactarte aparte para los encargos

---

## Modelo D: Todo a pedido (48hs o plazo fijo)

**Cómo funciona:**
- Todo producto requiere la misma cantidad de días de preparación (ej: 48hs)
- El cliente paga y después coordina la fecha de entrega por WhatsApp/email
- No hay date picker, no hay cálculo de fechas

**Casos de uso:**
- Cliente compra una torta Oreo → paga, la pastelería se contacta para coordinar entrega
- Cliente compra shots + torta → paga todo, después coordinan cada cosa

**Lo bueno:**
- Simple, sin lógica de fechas en la web
- Flexible para la pastelería
- Cero desperdicio

**Lo malo:**
- El cliente no sabe exactamente cuándo le llega hasta que habla con la pastelería
- La pastelería tiene que contactar al cliente manualmente

---

## Tabla comparativa

| Aspecto | A: Todo junto | B: Entregas separadas | C: Solo disponible | D: Plazo fijo post-pago |
|---|---|---|---|---|
| Mixed cart | ✅ Todo en 1 entrega | ✅ Cada cosa en su fecha | ❌ No hay mixed cart | ✅ Se coordina después |
| Cliente recibe hoy | ✅ (si es 0 días) | ✅ | ✅ | ❌ No |
| Cero desperdicio | ✅ | ✅ | ❌ (hay stock listo) | ✅ |
| Complejidad técnica | Baja | Media | Mínima | Mínima |
| Fecha la define el cliente | ✅ En el checkout | ✅ En el checkout | Hoy siempre | ❌ Después, por WhatsApp |
| Producción programada | ✅ | ✅ | Parcial | ✅ |
| Ideal para empezar rápido | ✅ | ❌ | ✅ | ✅ |

---

## Lo que falta definir con el negocio

1. ¿Venden productos que ya están hechos (vitrina) o todo se produce contra pedido?
2. ¿Aceptan entregar por partes o prefieren que todo salga junto?
3. ¿Quieren que el cliente elija la fecha en la web o prefieren coordinar después?
4. ¿Manejan tortas personalizadas como encargos separados del resto?
5. ¿Tienen delivery o solo retiro en local?
6. ¿Tienen horario de corte (ej: "pedidos hasta las 12 del mediodía para retirar hoy")?
7. ¿Manejan variantes de producto (tamaños, precios distintos)?

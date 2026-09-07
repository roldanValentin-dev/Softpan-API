# Definiciones pendientes — Tienda Online

**Para:** Dueña de la pastelería  
**De parte de:** Equipo de desarrollo  
**Objetivo:** Definir todas las reglas de negocio para lanzar la tienda online.

---

## Identidad y contacto

| # | Pregunta |
|---|----------|
| 1 | Nombre de la pastelería |
| 2 | WhatsApp (con código de país) |
| 3 | Instagram |
| 4 | Facebook |
| 5 | Dirección del local (si tiene) |
| 6 | Logo (si no, lo armamos con tipografía) |

---

## Productos

| # | Pregunta |
|---|----------|
| 7 | Lista de productos (mínimo 5). De cada uno: nombre, precio, categoría, descripción breve |
| 8 | Stock inmediato — ¿Qué productos ya están hechos y se pueden entregar hoy? |
| 9 | Encargos — ¿Qué productos se hacen a pedido y necesitan días de preparación? |
| 10 | Días de preparación — Para los encargos: ¿cuántos días tarda cada uno? |
| 11 | Fotos — Celular, luz natural, fondo limpio. Si no hay ahora, arrancamos igual. |
| 12 | Tortas personalizadas — ¿Se venden con precio fijo en la web o se consultan por WhatsApp? |
| 13 | Personalización — El sistema tiene selector de frosting/tamaño/mensaje. ¿Aplica a todo o solo a tortas? |

---

## Carrito mixto — Modelo de negocio

**Realidad:** vas a tener productos con **stock inmediato** (listos hoy) y productos **por encargo** (necesitan días).

**Problema:** si un cliente mete ambos en el mismo carrito, ¿qué hacemos?

**Ejemplo concreto:**

| Producto | Tipo | Precio |
|----------|------|--------|
| Shots (6 unidades) | Stock inmediato → listo hoy | $3.500 |
| Torta Opera | Encargo → tarda 3 días | $12.000 |

---

### Modelo A: Todo junto

Las shots + la torta se entregan TODO junto el día 3. La pastelería produce las shots el día 2.

- 1 pedido, 1 entrega, simple
- El cliente no recibe las shots hasta el día 3 aunque estaban listas hoy

### Modelo B: Entregas separadas (split)

Las shots se entregan HOY. La torta se entrega el día 3. 1 solo pago, 2 pedidos vinculados.

- El cliente recibe lo urgente hoy
- Cada producto en su fecha justa
- La pastelería ve 2 pedidos, coordina 2 entregas

### Modelo C: Solo stock inmediato en la web

La torta no se puede comprar online. Aparece con cartel "Consultar por WhatsApp". El cliente compra solo las shots hoy.

- Simple
- Se pierde la venta online de la torta

### Modelo D: Post-pago con coordinación

El cliente paga todo junto. La pastelería lo contacta para coordinar fechas.

- Flexible
- El cliente no sabe cuándo recibe hasta que habla

---

| Aspecto | A: Todo junto | B: Split | C: Solo stock | D: Post-pago |
|---------|:---:|:---:|:---:|:---:|
| Stock inmediato se entrega hoy | ❌ | ✅ | ✅ | ✅ |
| Encargos se venden online | ✅ | ✅ | ❌ | ✅ |
| 1 sola entrega | ✅ | ❌ | ✅ | Depende |
| Complejidad técnica | Baja | Media | Mínima | Mínima |

**¿Cuál preferís?**

---

## Envío

| # | Pregunta |
|---|----------|
| 14 | ¿Solo delivery o también retiro en local? |
| 15 | Costo de envío |
| 16 | ¿Monto mínimo para envío gratis? |
| 17 | ¿Precio varía por zona? |
| 18 | ¿A qué zonas llegan? |
| 19 | ¿Hay zonas a las que NO llegan? |
| 20 | ¿Quién hace las entregas? (cadetería propia, tercerizada, o la dueña) |

---

## Días, horarios y fechas

| # | Pregunta |
|---|----------|
| 21 | ¿Qué días trabajan? |
| 22 | Horarios de atención |
| 23 | Horario de corte — ¿Si piden después de X hora se procesa al día siguiente? |
| 24 | Ventanas de entrega — ¿Mañana, tarde, o cuando sea? |
| 25 | ¿Entregan los domingos? |
| 26 | Días de preparación — ¿Se cuentan días corridos o solo hábiles? |
| 27 | Feriados — ¿Hay días que no se entrega? |

---

## Pagos

| # | Pregunta |
|---|----------|
| 28 | ¿Qué métodos de pago ofrecer? — MP siempre. ¿Efectivo y transferencia con 10% OFF también? |
| 29 | Datos para transferencia — CBU / Alias / Titular / Banco |
| 30 | Mercado Pago Access Token (PRODUCCIÓN) |
| 31 | Mercado Pago Client Secret |

---

## Gmail

| # | Acción |
|---|--------|
| 32 | Crear un Gmail para la pastelería (si no tiene) |
| 33 | Activar verificación en dos pasos |
| 34 | Generar contraseña de aplicación y pasarla |

---

## Políticas

| # | Pregunta |
|---|----------|
| 35 | Cancelaciones — ¿El cliente puede cancelar? ¿Hasta cuándo? ¿Con penalidad? |
| 36 | Cambios y devoluciones — ¿Si llega en mal estado o no es lo que pidió? |
| 37 | Términos y condiciones — Los generamos. |
| 38 | Política de privacidad — La generamos. |

---

## Prueba previa

| # | Pregunta |
|---|----------|
| 39 | ¿Querés hacer pedidos de prueba antes de abrir al público? |

---

## Checklist rápido

| Prioridad | Item | Tiempo |
|:---------:|------|:------:|
| 🔴 | Nombre + WhatsApp + Instagram | 5 min |
| 🔴 | Access Token MP + Client Secret | 5 min |
| 🔴 | Gmail + contraseña de aplicación | 10 min |
| 🔴 | **Elegir modelo de carrito (A/B/C/D)** | 10 min |
| 🟡 | Lista de productos (stock inmediato y encargos) | 20 min |
| 🟡 | Costo de envío + zonas + horarios | 10 min |
| 🟡 | Métodos de pago + CBU | 10 min |
| 🟡 | Política de cancelación | 10 min |
| 🟢 | Fotos de productos | 1 hora |
| 🟢 | Prueba previa | 1 hora |

---

**Respondiendo estos puntos, en una semana la tienda está online y vendiendo.**

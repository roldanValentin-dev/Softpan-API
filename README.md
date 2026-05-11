# 🍰 Softpan - Sistema Integral de Gestión para Pastelerías

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-316192?style=for-the-badge&logo=postgresql)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![Clean Architecture](https://img.shields.io/badge/Clean-Architecture-green?style=for-the-badge)

**Transformando la gestión de pastelerías con tecnología moderna**

[Características](#-características) • [Instalación](#-instalación-rápida) • [Documentación](#-documentación-de-api) • [Demo](#-demo)

</div>

---

## 🎯 ¿Qué es Softpan?

**Softpan** es una solución completa de gestión empresarial diseñada específicamente para pastelerías modernas que necesitan:

- 📊 **Gestionar su negocio mayorista** con control total de clientes, ventas y pagos
- 🛒 **Vender online** con una tienda integrada para clientes finales
- 📈 **Tomar decisiones** basadas en datos con reportes y estadísticas en tiempo real
- 🔐 **Operar con seguridad** mediante autenticación robusta y control de accesos

### 💡 El Problema que Resuelve

Las pastelerías tradicionales enfrentan desafíos diarios:

- ❌ Gestión manual de pedidos y ventas (Excel, papel)
- ❌ Falta de control de stock en tiempo real
- ❌ Dificultad para gestionar precios personalizados por cliente
- ❌ No tienen presencia online para captar nuevos clientes
- ❌ Pérdida de información sobre pagos y deudas
- ❌ Imposibilidad de analizar qué productos se venden más

### ✅ La Solución que Ofrece Softpan

**Softpan** unifica todo en una sola plataforma:

1. **Sistema de Gestión Interna** 🏢
   - Control completo de clientes mayoristas
   - Precios personalizados por cliente
   - Gestión de ventas con estados (Pendiente, Parcialmente Pagada, Pagada)
   - Registro de pagos con aplicación automática a ventas
   - Cálculo automático de deudas y saldos

2. **Tienda Online** 🛒
   - Catálogo público con búsqueda avanzada
   - Sistema de pedidos online 24/7
   - Gestión automática de stock
   - Múltiples imágenes por producto
   - Seguimiento de pedidos en tiempo real

3. **Inteligencia de Negocio** 📊
   - Reportes de ventas por período
   - Productos más vendidos
   - Análisis de clientes
   - Estadísticas de pedidos online

---

## ✨ Características Principales

### 🏢 Para el Negocio (Sistema Interno)

<table>
<tr>
<td width="50%">

#### 👥 Gestión de Clientes
- Tipos de cliente (Mayorista, Minorista, Común)
- Precios personalizados por cliente
- Historial completo de compras
- Control de deudas en tiempo real

#### 💰 Ventas y Pagos
- Registro de ventas con múltiples productos
- Estados de venta (Pendiente, Parcial, Pagada)
- Múltiples formas de pago
- Aplicación de pagos a varias ventas
- Cálculo automático de saldos

</td>
<td width="50%">

#### 📦 Productos
- Catálogo completo de productos
- Precios base y personalizados
- Control de stock con alertas
- Múltiples imágenes por producto
- Categorización flexible

#### 📊 Reportes
- Ventas del mes
- Productos más vendidos
- Clientes con deuda
- Análisis de rentabilidad

</td>
</tr>
</table>

### 🛒 Para los Clientes (Tienda Online)

<table>
<tr>
<td width="50%">

#### 🔍 Experiencia de Compra
- Catálogo público sin necesidad de registro
- Búsqueda inteligente de productos
- Filtrado por categorías
- Galería de imágenes por producto
- Carrito de compras persistente

#### 📱 Gestión de Pedidos
- Pedidos online simples y rápidos
- 6 estados de pedido (Pendiente → Entregado)
- Cancelación de pedidos
- Historial completo de compras
- Notificaciones de estado

</td>
<td width="50%">

#### 👤 Perfil de Cliente
- Registro rápido y seguro
- Perfil editable
- Historial de pedidos
- Seguimiento en tiempo real

#### 🔐 Seguridad
- Autenticación JWT
- Datos encriptados
- Validación de stock
- Protección contra sobreventa

</td>
</tr>
</table>

---

## 🏗️ Arquitectura Técnica

Softpan está construido siguiendo **Clean Architecture**, garantizando:

- ✅ **Mantenibilidad**: Código organizado y fácil de modificar
- ✅ **Escalabilidad**: Preparado para crecer con tu negocio
- ✅ **Testabilidad**: Cada componente puede probarse independientemente
- ✅ **Independencia**: No atado a frameworks específicos

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

## 🛠️ Stack Tecnológico

### Backend
- **Framework**: .NET 8.0 (última versión LTS)
- **ORM**: Entity Framework Core 8.0
- **Base de Datos**: PostgreSQL 16
- **Autenticación**: ASP.NET Core Identity + JWT
- **Validación**: FluentValidation
- **Mapeo**: Mapster
- **Logging**: Serilog
- **API Docs**: Swagger/OpenAPI

### Infraestructura
- **Contenedores**: Docker + Docker Compose
- **Caché**: Redis (opcional)
- **CI/CD**: GitHub Actions (configurado)

### Patrones y Principios
- ✅ Clean Architecture
- ✅ Repository Pattern
- ✅ Unit of Work
- ✅ Dependency Injection
- ✅ SOLID Principles
- ✅ RESTful API

---

## 🚀 Instalación Rápida

### Opción 1: Docker (Recomendado - 2 minutos)

```bash
# 1. Clonar el repositorio
git clone https://github.com/tu-usuario/softpan.git
cd softpan

# 2. Levantar todo con Docker Compose
docker-compose up -d

# 3. ¡Listo! La API está corriendo en:
# http://localhost:7097
# Swagger: http://localhost:7097/swagger
```

### Opción 2: Local (Desarrollo)

```bash
# 1. Clonar el repositorio
git clone https://github.com/tu-usuario/softpan.git
cd softpan

# 2. Restaurar dependencias
dotnet restore

# 3. Aplicar migraciones
dotnet ef database update --project Softpan.Infrastructure --startup-project Softpan.API

# 4. Ejecutar
cd Softpan.API
dotnet run
```

---

## 📡 Documentación de API

### Endpoints Principales

#### 🔐 Autenticación
```http
POST /api/auth/login              # Login
POST /api/auth/register           # Registro empleado
POST /api/auth/register-cliente   # Registro cliente online
POST /api/auth/refresh            # Renovar token ⭐ NUEVO
POST /api/auth/revoke             # Revocar token ⭐ NUEVO
```

#### 🛒 Catálogo Público (Sin autenticación)
```http
GET  /api/catalogo/productos                      # Listar productos
GET  /api/catalogo/productos/buscar?q={query}     # Buscar
GET  /api/catalogo/productos/categoria/{cat}      # Por categoría
GET  /api/catalogo/categorias                     # Listar categorías
```

#### 📦 Pedidos Online
```http
POST /api/pedidos                    # Crear pedido
GET  /api/pedidos/mis-pedidos        # Mis pedidos
PUT  /api/pedidos/{id}/cancelar      # Cancelar pedido
```

#### 🏷️ Gestión de Productos (Admin)
```http
GET    /api/productos                # Listar todos
POST   /api/productos                # Crear
PUT    /api/productos/{id}           # Actualizar
PUT    /api/productos/{id}/stock     # Actualizar stock
DELETE /api/productos/{id}           # Eliminar
```

#### 🖼️ Imágenes de Productos
```http
GET    /api/productos/{id}/imagenes           # Listar imágenes
POST   /api/productos/{id}/imagenes           # Agregar imagen
PUT    /api/productos/{id}/imagenes/{imgId}   # Actualizar
DELETE /api/productos/{id}/imagenes/{imgId}   # Eliminar
```

**📚 Documentación completa**: http://localhost:7097/swagger

---

## 🎨 Características Técnicas Destacadas

### 🔒 Seguridad Robusta
- JWT con refresh tokens (renovación automática)
- Sistema de auditoría de acciones críticas
- Roles y permisos granulares
- Rate limiting para prevenir ataques
- Validación exhaustiva de entrada
- CORS configurado correctamente
- Revocación manual de tokens

### ⚡ Performance
- Caché de consultas frecuentes
- Eager loading optimizado
- Índices en base de datos
- Paginación en listados grandes
- Compresión de respuestas

### 🧪 Calidad de Código
- Validaciones con FluentValidation
- Manejo centralizado de excepciones
- Logging estructurado con Serilog
- DTOs para todas las respuestas
- Mapeo automático con Mapster

### 📊 Gestión de Stock Inteligente
- Validación automática al crear pedidos
- Descuento de stock al confirmar
- Restauración al cancelar
- Alertas de stock bajo
- Prevención de sobreventa

---

## 🗄️ Modelo de Datos

### Entidades Principales

```
Sistema Online:
├── ClienteOnline (Clientes de tienda)
├── Pedido (Pedidos online)
├── PedidoDetalle (Items del pedido)
├── Producto (Catálogo)
└── ProductoImagen (Galería de imágenes)

Sistema Interno:
├── Cliente (Mayoristas)
├── Venta (Ventas mayoristas)
├── DetalleVenta (Items de venta)
├── Pago (Pagos recibidos)
├── PagoVenta (Aplicación de pagos)
└── PrecioCliente (Precios personalizados)

Seguridad:
└── ApplicationUser (Usuarios del sistema)
```

---

## 🐳 Docker

### Servicios Incluidos

```yaml
services:
  postgres:    # Base de datos PostgreSQL 16
  api:         # API .NET 8.0
```

### Comandos Útiles

```bash
# Ver logs en tiempo real
docker-compose logs -f api

# Reiniciar servicios
docker-compose restart

# Reconstruir después de cambios
docker-compose build --no-cache api
docker-compose up -d api

# Acceder a la base de datos
docker exec -it softpan-postgres psql -U softpan -d SoftpanDB
```

---

## 📈 Roadmap

### ✅ Implementado (v1.0)
- [x] Sistema de gestión interna completo
- [x] Tienda online funcional
- [x] Gestión de stock automática
- [x] Sistema de múltiples imágenes
- [x] Búsqueda de productos
- [x] Cancelación de pedidos
- [x] Autenticación JWT con roles
- [x] Refresh tokens ⭐ NUEVO
- [x] Sistema de auditoría ⭐ NUEVO
- [x] Validadores robustos ⭐ NUEVO

### 🚧 En Desarrollo (v1.1)
- [x] Upload de imágenes (local storage) ✅
- [ ] Método de pago en pedidos
- [ ] Confirmación de pago manual
- [ ] Notificaciones por email
- [ ] Historial de cambios de estado

### 🔮 Futuro (v2.0)
- [ ] Integración con MercadoPago
- [ ] Notificaciones push
- [ ] App móvil (React Native)
- [ ] Sistema de cupones y descuentos
- [ ] Programa de fidelización
- [ ] Reportes avanzados con gráficos
- [ ] Integración con WhatsApp Business

---

## 🧪 Testing

### Datos de Prueba Incluidos

El sistema viene con datos de prueba pre-cargados:

**Usuarios:**
```
Admin:
  Email: admin@softpan.com
  Password: Admin123!

Vendedor:
  Email: vendedor@softpan.com
  Password: Vendedor123!

Cliente:
  Email: cliente@softpan.com
  Password: Cliente123!
```

**Productos:**
- 19 productos de ejemplo
- 3 categorías (Tortas, Tartas, Pasteles)
- Imágenes de muestra

---

## 📚 Documentación Adicional

- 🔐 [Seguridad Implementada](SEGURIDAD_IMPLEMENTADA.md) ⭐ NUEVO
- 📝 [Changelog de Seguridad](CHANGELOG_SEGURIDAD.md) ⭐ NUEVO
- 📖 [Guía de Docker](README_DOCKER.md)
- 📸 [Sistema de Upload de Imágenes](UPLOAD_IMAGENES.md) ⭐ NUEVO
- 💻 [Ejemplo Frontend Upload](EJEMPLO_FRONTEND_UPLOAD.jsx) ⭐ NUEVO
- 📝 [Tareas Pendientes](TAREAS_PENDIENTES.md)
- 🐛 [Bugs Corregidos](BUGS_CORREGIDOS.md)
- 🔧 [Script de Datos de Prueba](datos_prueba.sql)

---

## 🤝 Contribuir

Las contribuciones son bienvenidas. Para contribuir:

1. Fork el proyecto
2. Crea una rama (`git checkout -b feature/NuevaCaracteristica`)
3. Commit tus cambios (`git commit -m 'Agregar nueva característica'`)
4. Push a la rama (`git push origin feature/NuevaCaracteristica`)
5. Abre un Pull Request

### Guía de Estilo
- Seguir Clean Architecture
- Código en inglés, comentarios en español
- Usar DTOs para todas las respuestas
- Validaciones con FluentValidation
- Tests unitarios para lógica de negocio

---

## 📄 Licencia

Este proyecto está bajo la Licencia MIT. Ver [LICENSE](LICENSE) para más detalles.

---

## 👨‍💻 Creador

<div align="center">

### **Valentín Roldán**

Desarrollador Full Stack especializado en arquitecturas limpias y soluciones empresariales

[![GitHub](https://img.shields.io/badge/GitHub-100000?style=for-the-badge&logo=github&logoColor=white)](https://github.com/valentin-roldan)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white)](https://linkedin.com/in/valentin-roldan)
[![Email](https://img.shields.io/badge/Email-D14836?style=for-the-badge&logo=gmail&logoColor=white)](mailto:valentin.roldan@example.com)

**"Construyendo soluciones que transforman negocios"**

</div>

### 💼 Sobre el Creador

Valentín Roldán es un desarrollador apasionado por crear soluciones tecnológicas que resuelven problemas reales. Con experiencia en:

- 🏗️ Arquitecturas limpias y escalables
- 🔧 Backend con .NET y Node.js
- 🎨 Frontend con React y Vue
- 🐳 DevOps con Docker y CI/CD
- 📊 Bases de datos SQL y NoSQL

**Softpan** nació de la necesidad de digitalizar pastelerías tradicionales, combinando gestión interna robusta con una experiencia de compra online moderna.

---

## 🙏 Agradecimientos

- ASP.NET Core Team por el excelente framework
- Entity Framework Core Team por el ORM más completo
- Comunidad de .NET por el apoyo constante
- PostgreSQL por la base de datos confiable
- Docker por simplificar el deployment

---

## 📞 Soporte y Contacto

¿Necesitas ayuda o tienes alguna pregunta?

- 📧 **Email**: valentin.roldan@example.com
- 💬 **Issues**: [GitHub Issues](https://github.com/valentin-roldan/softpan/issues)
- 📖 **Documentación**: [Wiki del Proyecto](https://github.com/valentin-roldan/softpan/wiki)
- 🐛 **Reportar Bug**: [Nuevo Issue](https://github.com/valentin-roldan/softpan/issues/new)

---

## 📊 Estadísticas del Proyecto

<div align="center">

![GitHub stars](https://img.shields.io/github/stars/valentin-roldan/softpan?style=social)
![GitHub forks](https://img.shields.io/github/forks/valentin-roldan/softpan?style=social)
![GitHub watchers](https://img.shields.io/github/watchers/valentin-roldan/softpan?style=social)

![GitHub last commit](https://img.shields.io/github/last-commit/valentin-roldan/softpan)
![GitHub issues](https://img.shields.io/github/issues/valentin-roldan/softpan)
![GitHub pull requests](https://img.shields.io/github/issues-pr/valentin-roldan/softpan)

</div>

---

<div align="center">

## ⭐ Si te gusta este proyecto, dale una estrella en GitHub ⭐

**Hecho con ❤️, ☕ y mucho código**

*Transformando pastelerías tradicionales en negocios digitales*

---

**© 2024 Valentín Roldán. Todos los derechos reservados.**

</div>

# 🐳 Softpan API - Docker Setup

## Requisitos Previos
- Docker Desktop instalado
- Docker Compose instalado

## 🚀 Cómo Correr la API con Docker

### Opción 1: Usar Docker Compose (Recomendado)

Esto levanta PostgreSQL, Redis y la API juntos:

```bash
# Desde la carpeta raíz del proyecto (d:\Repos\Softpan)
docker-compose up -d
```

La API estará disponible en: `https://localhost:7097`

### Opción 2: Solo la API (si ya tienes PostgreSQL y Redis corriendo)

```bash
# Build de la imagen
docker build -t softpan-api .

# Correr el contenedor
docker run -d -p 7097:8080 ^
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5432;Database=SoftpanDB;Username=softpan;Password=Softpan123!" ^
  -e Redis__ConnectionString="host.docker.internal:6379" ^
  --name softpan-api ^
  softpan-api
```

## 📋 Comandos Útiles

### Ver logs de la API
```bash
docker-compose logs -f api
```

### Ver logs de PostgreSQL
```bash
docker-compose logs -f postgres
```

### Ver logs de Redis
```bash
docker-compose logs -f redis
```

### Detener todos los servicios
```bash
docker-compose down
```

### Detener y eliminar volúmenes (borra la base de datos)
```bash
docker-compose down -v
```

### Reiniciar solo la API
```bash
docker-compose restart api
```

### Reconstruir la API después de cambios en el código
```bash
docker-compose up -d --build api
```

## 🔧 Aplicar Migraciones

Después de levantar los contenedores por primera vez:

```bash
# Entrar al contenedor de la API
docker exec -it softpan-api bash

# Aplicar migraciones (si tienes EF Core Tools instalado en el contenedor)
dotnet ef database update

# Salir del contenedor
exit
```

O desde tu máquina local (apuntando a la base de datos en Docker):

```bash
cd Softpan.API
dotnet ef database update
```

## 🌐 URLs de Acceso

- **API**: http://localhost:7097
- **Swagger**: http://localhost:7097/swagger
- **PostgreSQL**: localhost:5432
- **Redis**: localhost:6379

## 📊 Conectar a PostgreSQL desde pgAdmin

- **Host**: localhost
- **Port**: 5432
- **Database**: SoftpanDB
- **Username**: softpan
- **Password**: Softpan123!

## 🔍 Verificar que todo está corriendo

```bash
docker-compose ps
```

Deberías ver 3 servicios corriendo:
- softpan-postgres
- softpan-redis
- softpan-api

## ⚠️ Troubleshooting

### La API no se conecta a PostgreSQL
Verifica que PostgreSQL esté healthy:
```bash
docker-compose ps postgres
```

### La API no inicia
Revisa los logs:
```bash
docker-compose logs api
```

### Puerto 7097 ya está en uso
Cambia el puerto en docker-compose.yml:
```yaml
ports:
  - "8080:8080"  # Usa otro puerto
```

## 🔄 Actualizar después de cambios en el código

```bash
# Reconstruir y reiniciar
docker-compose up -d --build
```

## 🧹 Limpiar todo

```bash
# Detener y eliminar contenedores, redes y volúmenes
docker-compose down -v

# Eliminar la imagen de la API
docker rmi softpan-api
```

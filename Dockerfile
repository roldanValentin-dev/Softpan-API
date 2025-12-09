# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["Softpan.API/Softpan.API.csproj", "Softpan.API/"]
COPY ["Softpan.Application/Softpan.Application.csproj", "Softpan.Application/"]
COPY ["Softpan.Domain/Softpan.Domain.csproj", "Softpan.Domain/"]
COPY ["Softpan.Infrastructure/Softpan.Infrastructure.csproj", "Softpan.Infrastructure/"]

# Restaurar dependencias
RUN dotnet restore "Softpan.API/Softpan.API.csproj"

# Copiar todo el código
COPY . .

# Build
WORKDIR "/src/Softpan.API"
RUN dotnet build "Softpan.API.csproj" -c Release -o /app/build

# Etapa 2: Publish
FROM build AS publish
RUN dotnet publish "Softpan.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Softpan.API.dll"]

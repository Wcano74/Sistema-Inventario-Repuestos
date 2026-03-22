# ==============================================================
# Dockerfile - Sistema de Inventario (ASP.NET Core 9)
# Multi-stage build para imagen optimizada de producción
# ==============================================================

# --- Etapa 1: Build ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivo de proyecto y restaurar dependencias (aprovechar cache de capas)
COPY SistemaInventario/SistemaInventario.csproj ./SistemaInventario/
RUN dotnet restore ./SistemaInventario/SistemaInventario.csproj

# Copiar el resto del código fuente
COPY SistemaInventario/ ./SistemaInventario/

# Compilar y publicar en modo Release
RUN dotnet publish ./SistemaInventario/SistemaInventario.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# --- Etapa 2: Runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Configuración de entorno
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Copiar archivos publicados desde la etapa de build
COPY --from=build /app/publish .

# Exponer puerto
EXPOSE 8080

# Punto de entrada
ENTRYPOINT ["dotnet", "SistemaInventario.dll"]

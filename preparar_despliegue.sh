#!/bin/bash
# Script de Preparación para Despliegue en Cliente Windows
# Control Librería - Sistema de Gestión

echo "================================================"
echo "Control Librería - Preparación de Despliegue"
echo "================================================"
echo ""

# Variables
PROJECT_DIR="/Volumes/Mac NVME/Control Librería/ControlLibrería/ControlLibrería"
DIST_DIR="/Volumes/Mac NVME/Control Librería/ControlLibrería/Distribucion_Cliente"
PUBLISH_DIR="$PROJECT_DIR/publish"
DB_SCRIPT="/Volumes/Mac NVME/Control Librería/ControlLibrería/DatabaseScripts_Complete.sql"

echo "📦 Paso 1: Limpiando directorios anteriores..."
rm -rf "$PUBLISH_DIR"
rm -rf "$DIST_DIR"

echo "✅ Directorios limpiados"
echo ""

echo "🔨 Paso 2: Publicando proyecto para Windows..."
cd "$PROJECT_DIR"

dotnet publish -c Release \
    -o "$PUBLISH_DIR" \
    --runtime win-x64 \
    --self-contained false \
    /p:PublishTrimmed=false

if [ $? -eq 0 ]; then
    echo "✅ Publicación exitosa"
else
    echo "❌ Error en la publicación"
    exit 1
fi
echo ""

echo "📁 Paso 3: Creando estructura de distribución..."
mkdir -p "$DIST_DIR/Aplicacion"
mkdir -p "$DIST_DIR/Scripts"
mkdir -p "$DIST_DIR/Documentacion"

echo "✅ Estructura creada"
echo ""

echo "📋 Paso 4: Copiando archivos..."

# Copiar aplicación publicada
echo "  - Copiando aplicación..."
cp -r "$PUBLISH_DIR/"* "$DIST_DIR/Aplicacion/"

# Copiar script de base de datos
echo "  - Copiando script de base de datos..."
cp "$DB_SCRIPT" "$DIST_DIR/Scripts/"

# Copiar documentación
echo "  - Copiando documentación..."
cp "/Volumes/Mac NVME/Control Librería/ControlLibrería/INSTALACION_BASE_DATOS.md" "$DIST_DIR/Documentacion/"
cp "/Volumes/Mac NVME/Control Librería/ControlLibrería/GUIA_DESPLIEGUE_WINDOWS.md" "$DIST_DIR/Documentacion/"
cp "/Volumes/Mac NVME/Control Librería/ControlLibrería/CORRECCIONES_OPCIONALES.md" "$DIST_DIR/Documentacion/"

echo "✅ Archivos copiados"
echo ""

echo "📝 Paso 5: Creando archivo README para el cliente..."
cat > "$DIST_DIR/LEEME.txt" << 'EOF'
================================================================================
CONTROL LIBRERÍA - PAQUETE DE INSTALACIÓN
================================================================================

CONTENIDO DEL PAQUETE:
----------------------
1. Aplicacion/          - Archivos de la aplicación web
2. Scripts/             - Script de base de datos SQL Server
3. Documentacion/       - Guías de instalación y configuración

PASOS RÁPIDOS DE INSTALACIÓN:
------------------------------
1. Instalar SQL Server 2019 o superior
2. Instalar .NET 9.0 Runtime + ASP.NET Core Hosting Bundle
   Descargar de: https://dotnet.microsoft.com/download/dotnet/9.0
3. Habilitar IIS en Windows
4. Ejecutar script: Scripts/DatabaseScripts_Complete.sql
5. Copiar carpeta Aplicacion/ a: C:\inetpub\wwwroot\ControlLibreria
6. Configurar IIS según guía: Documentacion/GUIA_DESPLIEGUE_WINDOWS.md

DOCUMENTACIÓN INCLUIDA:
-----------------------
- GUIA_DESPLIEGUE_WINDOWS.md    : Instalación completa en IIS
- INSTALACION_BASE_DATOS.md     : Instalación de base de datos
- CORRECCIONES_OPCIONALES.md    : Mejoras opcionales (no necesarias)

PRIMER ACCESO:
--------------
Usuario: 00001
Contraseña: Admin123!

IMPORTANTE: Cambiar la contraseña después del primer inicio de sesión.

SOPORTE:
--------
Para problemas durante la instalación, consultar las guías en la carpeta
Documentacion/ o revisar los logs de la aplicación.

================================================================================
Versión: 1.0
Fecha: 2026-01-17
================================================================================
EOF

echo "✅ README creado"
echo ""

echo "📦 Paso 6: Creando archivo ZIP para transferencia..."
cd "/Volumes/Mac NVME/Control Librería/ControlLibrería"

# Eliminar ZIP anterior si existe
rm -f "ControlLibreria_Instalacion.zip"

# Crear nuevo ZIP
zip -r "ControlLibreria_Instalacion.zip" "Distribucion_Cliente/" -x "*.DS_Store"

if [ $? -eq 0 ]; then
    echo "✅ Archivo ZIP creado exitosamente"
    
    # Mostrar tamaño del archivo
    ZIP_SIZE=$(du -h "ControlLibreria_Instalacion.zip" | cut -f1)
    echo "   Tamaño: $ZIP_SIZE"
else
    echo "❌ Error al crear ZIP"
    exit 1
fi
echo ""

echo "📊 Paso 7: Resumen de archivos generados..."
echo ""
echo "Carpeta de distribución:"
echo "  $DIST_DIR"
echo ""
echo "Contenido:"
tree -L 2 "$DIST_DIR" 2>/dev/null || find "$DIST_DIR" -maxdepth 2 -print
echo ""
echo "Archivo para transferir al cliente:"
echo "  /Volumes/Mac NVME/Control Librería/ControlLibrería/ControlLibreria_Instalacion.zip"
echo ""

echo "================================================"
echo "✅ PREPARACIÓN COMPLETADA EXITOSAMENTE"
echo "================================================"
echo ""
echo "PRÓXIMOS PASOS:"
echo "1. Transferir ControlLibreria_Instalacion.zip al cliente"
echo "2. Descomprimir en máquina Windows"
echo "3. Seguir instrucciones en LEEME.txt"
echo "4. Consultar GUIA_DESPLIEGUE_WINDOWS.md para detalles"
echo ""
echo "================================================"

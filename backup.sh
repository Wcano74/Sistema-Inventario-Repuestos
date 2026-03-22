#!/bin/bash
# ==============================================================
# Backup de Base de Datos - Sistema de Inventario
# Genera un archivo .bak con fecha y hora en la carpeta ./backups/
# ==============================================================
# Uso:
#   ./backup.sh              (backup normal)
#   ./backup.sh --con-nombre  (pide un nombre descriptivo)
# ==============================================================

CONTAINER="inventario-sqlserver"
DB_NAME="SistemaInventarioDB"
SA_PASSWORD="SistemaInventario77!"
BACKUP_DIR="./backups"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")

# Verificar que el contenedor esté corriendo
if ! docker ps --format '{{.Names}}' | grep -q "^${CONTAINER}$"; then
    echo "ERROR: El contenedor '${CONTAINER}' no esta corriendo."
    echo "Inicia el sistema con: docker compose up -d"
    exit 1
fi

# Crear directorio de backups si no existe
mkdir -p "$BACKUP_DIR"

# Nombre del archivo
if [ "$1" == "--con-nombre" ]; then
    read -p "Descripcion del backup (ej: antes-de-actualizacion): " DESCRIPCION
    DESCRIPCION=$(echo "$DESCRIPCION" | tr ' ' '-' | tr -cd '[:alnum:]-_')
    BACKUP_FILE="${DB_NAME}_${TIMESTAMP}_${DESCRIPCION}.bak"
else
    BACKUP_FILE="${DB_NAME}_${TIMESTAMP}.bak"
fi

echo "================================================"
echo "  Backup de Base de Datos"
echo "================================================"
echo "  Base de datos: $DB_NAME"
echo "  Archivo:       $BACKUP_FILE"
echo "  Destino:       $BACKUP_DIR/"
echo "================================================"
echo ""
echo "Creando backup..."

# Ejecutar backup dentro del contenedor
docker exec "$CONTAINER" /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$SA_PASSWORD" -C \
    -Q "BACKUP DATABASE [$DB_NAME] TO DISK = N'/var/opt/mssql/backups/$BACKUP_FILE' WITH FORMAT, INIT, NAME = N'$DB_NAME-Backup-$TIMESTAMP'"

if [ $? -eq 0 ]; then
    # Verificar que el archivo existe
    if [ -f "$BACKUP_DIR/$BACKUP_FILE" ]; then
        FILE_SIZE=$(du -h "$BACKUP_DIR/$BACKUP_FILE" | cut -f1)
        echo ""
        echo "================================================"
        echo "  BACKUP EXITOSO"
        echo "  Archivo: $BACKUP_DIR/$BACKUP_FILE"
        echo "  Tamano:  $FILE_SIZE"
        echo "================================================"

        # Limpiar backups antiguos (mantener los ultimos 10)
        BACKUP_COUNT=$(ls -1 "$BACKUP_DIR"/*.bak 2>/dev/null | wc -l)
        if [ "$BACKUP_COUNT" -gt 10 ]; then
            echo ""
            echo "Limpiando backups antiguos (manteniendo los 10 mas recientes)..."
            ls -1t "$BACKUP_DIR"/*.bak | tail -n +11 | xargs rm -f
            echo "Limpieza completada."
        fi
    else
        echo ""
        echo "ADVERTENCIA: El backup se ejecuto pero el archivo no se encontro en $BACKUP_DIR/"
        echo "Verifica los permisos del directorio."
    fi
else
    echo ""
    echo "ERROR: Fallo al crear el backup."
    exit 1
fi

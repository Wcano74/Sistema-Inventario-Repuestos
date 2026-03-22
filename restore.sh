#!/bin/bash
# ==============================================================
# Restaurar Base de Datos - Sistema de Inventario
# Restaura un archivo .bak en el contenedor de SQL Server
# ==============================================================
# Uso:
#   ./restore.sh                          (muestra lista de backups disponibles)
#   ./restore.sh backups/archivo.bak      (restaura un backup especifico)
# ==============================================================

CONTAINER="inventario-sqlserver"
DB_NAME="SistemaInventarioDB"
SA_PASSWORD="SistemaInventario77!"
BACKUP_DIR="./backups"

# Verificar que el contenedor esté corriendo
if ! docker ps --format '{{.Names}}' | grep -q "^${CONTAINER}$"; then
    echo "ERROR: El contenedor '${CONTAINER}' no esta corriendo."
    echo "Inicia el sistema con: docker compose up -d"
    exit 1
fi

# Si no se pasa argumento, mostrar backups disponibles
if [ -z "$1" ]; then
    echo "================================================"
    echo "  Backups Disponibles"
    echo "================================================"

    if [ ! -d "$BACKUP_DIR" ] || [ -z "$(ls -A "$BACKUP_DIR"/*.bak 2>/dev/null)" ]; then
        echo "  No hay backups disponibles en $BACKUP_DIR/"
        echo "  Crea uno primero con: ./backup.sh"
        exit 1
    fi

    echo ""
    i=1
    declare -a BACKUPS
    for f in $(ls -1t "$BACKUP_DIR"/*.bak 2>/dev/null); do
        FILE_SIZE=$(du -h "$f" | cut -f1)
        FILE_DATE=$(stat -f "%Sm" -t "%Y-%m-%d %H:%M" "$f" 2>/dev/null || stat -c "%y" "$f" 2>/dev/null | cut -d. -f1)
        BACKUPS[$i]="$f"
        echo "  [$i] $(basename "$f")  ($FILE_SIZE)  $FILE_DATE"
        i=$((i + 1))
    done

    echo ""
    read -p "Selecciona el numero del backup a restaurar (0 para cancelar): " SELECTION

    if [ "$SELECTION" == "0" ] || [ -z "$SELECTION" ]; then
        echo "Operacion cancelada."
        exit 0
    fi

    BACKUP_FILE="${BACKUPS[$SELECTION]}"
    if [ -z "$BACKUP_FILE" ]; then
        echo "Seleccion no valida."
        exit 1
    fi
else
    BACKUP_FILE="$1"
fi

# Verificar que el archivo existe
if [ ! -f "$BACKUP_FILE" ]; then
    echo "ERROR: No se encontro el archivo: $BACKUP_FILE"
    exit 1
fi

BACKUP_FILENAME=$(basename "$BACKUP_FILE")

echo ""
echo "================================================"
echo "  ADVERTENCIA: Restaurar Base de Datos"
echo "================================================"
echo "  Archivo:  $BACKUP_FILENAME"
echo "  Base:     $DB_NAME"
echo ""
echo "  ESTO REEMPLAZARA TODOS LOS DATOS ACTUALES"
echo "  de la base de datos con los del backup."
echo "================================================"
echo ""
read -p "Estas seguro? Escribe 'SI' para confirmar: " CONFIRM

if [ "$CONFIRM" != "SI" ]; then
    echo "Operacion cancelada."
    exit 0
fi

echo ""
echo "Restaurando backup..."

# Poner la BD en single user para forzar desconexiones
docker exec "$CONTAINER" /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$SA_PASSWORD" -C \
    -Q "IF DB_ID('$DB_NAME') IS NOT NULL ALTER DATABASE [$DB_NAME] SET SINGLE_USER WITH ROLLBACK IMMEDIATE" \
    2>/dev/null

# Restaurar el backup
docker exec "$CONTAINER" /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$SA_PASSWORD" -C \
    -Q "RESTORE DATABASE [$DB_NAME] FROM DISK = N'/var/opt/mssql/backups/$BACKUP_FILENAME' WITH REPLACE, RECOVERY"

if [ $? -eq 0 ]; then
    # Volver a multi user
    docker exec "$CONTAINER" /opt/mssql-tools18/bin/sqlcmd \
        -S localhost -U sa -P "$SA_PASSWORD" -C \
        -Q "ALTER DATABASE [$DB_NAME] SET MULTI_USER" \
        2>/dev/null

    echo ""
    echo "================================================"
    echo "  RESTAURACION EXITOSA"
    echo "  La base de datos '$DB_NAME' fue restaurada."
    echo "  Reinicia la app: docker compose restart webapp"
    echo "================================================"
else
    # Intentar volver a multi user en caso de error
    docker exec "$CONTAINER" /opt/mssql-tools18/bin/sqlcmd \
        -S localhost -U sa -P "$SA_PASSWORD" -C \
        -Q "ALTER DATABASE [$DB_NAME] SET MULTI_USER" \
        2>/dev/null

    echo ""
    echo "ERROR: Fallo al restaurar el backup."
    exit 1
fi

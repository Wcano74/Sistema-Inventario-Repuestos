#!/bin/bash
# ==============================================================
# Configurar Backup Automatico - Sistema de Inventario
# Instala un cron job para hacer backups automaticos diarios
# ==============================================================
# Uso:
#   ./setup-auto-backup.sh          (configura backup diario a las 2:00 AM)
#   ./setup-auto-backup.sh --remove (elimina el backup automatico)
# ==============================================================

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
BACKUP_SCRIPT="$SCRIPT_DIR/backup.sh"
CRON_LOG="$SCRIPT_DIR/backups/cron-backup.log"
CRON_MARKER="# SistemaInventario-AutoBackup"

if [ "$1" == "--remove" ]; then
    echo "Removiendo backup automatico..."
    crontab -l 2>/dev/null | grep -v "$CRON_MARKER" | crontab -
    echo "Backup automatico removido."
    exit 0
fi

# Verificar que el script de backup existe
if [ ! -f "$BACKUP_SCRIPT" ]; then
    echo "ERROR: No se encontro el script de backup en: $BACKUP_SCRIPT"
    exit 1
fi

echo "================================================"
echo "  Configurar Backup Automatico"
echo "================================================"
echo ""
echo "  Opciones de frecuencia:"
echo "  [1] Diario a las 2:00 AM (recomendado)"
echo "  [2] Cada 12 horas (2:00 AM y 2:00 PM)"
echo "  [3] Cada 6 horas"
echo ""
read -p "Selecciona una opcion (1-3): " OPTION

case $OPTION in
    1)
        CRON_SCHEDULE="0 2 * * *"
        FREQ_DESC="Diario a las 2:00 AM"
        ;;
    2)
        CRON_SCHEDULE="0 2,14 * * *"
        FREQ_DESC="Cada 12 horas (2:00 AM y 2:00 PM)"
        ;;
    3)
        CRON_SCHEDULE="0 */6 * * *"
        FREQ_DESC="Cada 6 horas"
        ;;
    *)
        echo "Opcion no valida."
        exit 1
        ;;
esac

# Remover cron anterior si existe
CURRENT_CRON=$(crontab -l 2>/dev/null | grep -v "$CRON_MARKER")

# Agregar nuevo cron
(echo "$CURRENT_CRON"; echo "$CRON_SCHEDULE cd $SCRIPT_DIR && ./backup.sh >> $CRON_LOG 2>&1 $CRON_MARKER") | crontab -

if [ $? -eq 0 ]; then
    echo ""
    echo "================================================"
    echo "  BACKUP AUTOMATICO CONFIGURADO"
    echo "  Frecuencia: $FREQ_DESC"
    echo "  Log:        $CRON_LOG"
    echo "  Los 10 backups mas recientes se mantienen,"
    echo "  los mas antiguos se eliminan automaticamente."
    echo ""
    echo "  Para verificar: crontab -l"
    echo "  Para remover:   ./setup-auto-backup.sh --remove"
    echo "================================================"
else
    echo "ERROR: No se pudo configurar el cron job."
    exit 1
fi

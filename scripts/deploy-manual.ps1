# ==============================================================
# deploy-manual.ps1
# Deploy manual de emergencia — modelo ghcr.io (sin codigo fuente)
#
# Uso:
#   cd "D:\Repuestos Davis"
#   .\scripts\deploy-manual.ps1
# ==============================================================

$ErrorActionPreference = "Stop"
$deployPath = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$image      = "ghcr.io/wcano74/sistema-inventario:latest"
$composeFile = Join-Path $deployPath "docker-compose.prod.yml"

function Write-Banner($t) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  $t" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
}
function Write-OK($t)   { Write-Host "[OK] $t" -ForegroundColor Green }
function Write-WARN($t) { Write-Host "[!!] $t" -ForegroundColor Yellow }
function Write-FAIL($t) { Write-Host "[XX] $t" -ForegroundColor Red }

Write-Banner "DEPLOY MANUAL - Sistema Inventario"
Write-Host "  Ruta:   $deployPath"
Write-Host "  Imagen: $image"
Write-Host "  Fecha:  $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')"

if (-not [System.IO.Directory]::Exists($deployPath)) {
    Write-FAIL "No se encontro la carpeta: $deployPath"
    exit 1
}
if (-not (Test-Path $composeFile)) {
    Write-FAIL "No se encontro docker-compose.prod.yml en $deployPath"
    exit 1
}

# ─── Paso 1: Backup ────────────────────────────────────────────
Write-Banner "Paso 1/4: Backup de base de datos"

New-Item -ItemType Directory -Force -Path (Join-Path $deployPath "backups") | Out-Null

$activo = docker ps --format "{{.Names}}" | Select-String "inventario-sqlserver"
if (-not $activo) {
    Write-WARN "SQL Server no esta corriendo, omitiendo backup."
} else {
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $archivo   = "SistemaInventarioDB_${timestamp}_manual-deploy.bak"
    Write-Host "Creando backup: $archivo"

    docker exec inventario-sqlserver /opt/mssql-tools18/bin/sqlcmd `
        -S localhost -U sa -P "SistemaInventario77!" -C `
        -Q "BACKUP DATABASE [SistemaInventarioDB] TO DISK = N'/var/opt/mssql/backups/$archivo' WITH FORMAT, INIT, COMPRESSION"

    if ($LASTEXITCODE -ne 0) {
        Write-FAIL "Backup fallo. Abortando para proteger los datos."
        exit 1
    }
    Write-OK "Backup creado: backups\$archivo"
}

# ─── Paso 2: Descargar imagen ──────────────────────────────────
Write-Banner "Paso 2/4: Descargar imagen desde ghcr.io"

docker pull $image
if ($LASTEXITCODE -ne 0) {
    Write-FAIL "No se pudo descargar la imagen. Verifica conexion y credenciales."
    exit 1
}
Write-OK "Imagen descargada: $image"

# ─── Paso 3: Deploy ────────────────────────────────────────────
Write-Banner "Paso 3/4: Deploy"

Set-Location -LiteralPath $deployPath
docker compose -f docker-compose.prod.yml up -d --no-build webapp
if ($LASTEXITCODE -ne 0) {
    Write-FAIL "Deploy fallo."
    exit 1
}
Write-OK "Contenedor reiniciado con la nueva imagen"

# ─── Paso 4: Health check ──────────────────────────────────────
Write-Banner "Paso 4/4: Verificar salud del sistema"

Write-Host "Esperando 35 segundos..."
Start-Sleep 35

docker compose -f docker-compose.prod.yml ps

$ok = $false
for ($i = 1; $i -le 5; $i++) {
    try {
        $r = Invoke-WebRequest -Uri "http://localhost:8080" -MaximumRedirection 5 -TimeoutSec 15 -UseBasicParsing
        Write-OK "Sistema respondiendo: HTTP $($r.StatusCode)"
        $ok = $true
        break
    } catch {
        Write-WARN "Intento $i/5 fallido, reintentando en 10s..."
        Start-Sleep 10
    }
}

if (-not $ok) {
    Write-Host "Ultimos logs:" -ForegroundColor Yellow
    docker logs inventario-webapp --tail 30
    Write-FAIL "El sistema no responde. Revisa los logs."
    exit 1
}

# ─── Resumen ───────────────────────────────────────────────────
Write-Banner "DEPLOY COMPLETADO"
Write-Host "  Imagen: $image"
Write-Host "  Fecha:  $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')"
Write-Host "  URL:    http://localhost:8080"
Write-Host ""

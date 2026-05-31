# ==============================================================
# deploy-manual.ps1
# Ejecuta el mismo proceso de deploy que el CI/CD pero manualmente.
# Util para el primer despliegue o si necesitas forzar un update.
# ==============================================================
# Uso:
#   cd C:\SistemaInventario
#   .\scripts\deploy-manual.ps1
# ==============================================================

param(
    [string]$Rama = "main"
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$deployPath = Split-Path -Parent $scriptDir
Set-Location $deployPath

function Write-Banner($texto) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  $texto" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
}

function Write-OK($texto)    { Write-Host "[OK] $texto" -ForegroundColor Green }
function Write-WARN($texto)  { Write-Host "[!!] $texto" -ForegroundColor Yellow }
function Write-FAIL($texto)  { Write-Host "[XX] $texto" -ForegroundColor Red }

# ─── Validaciones ──────────────────────────────────────────────
Write-Banner "DEPLOY MANUAL - Sistema Inventario"
Write-Host "Ruta: $deployPath"
Write-Host "Rama: $Rama"
Write-Host "Fecha: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')"

if (-not (Test-Path "$deployPath\docker-compose.yml")) {
    Write-FAIL "No se encontro docker-compose.yml en $deployPath"
    exit 1
}

# ─── Paso 1: Backup ────────────────────────────────────────────
Write-Banner "Paso 1/5: Backup de base de datos"

New-Item -ItemType Directory -Force -Path ".\backups" | Out-Null

$contenedorActivo = docker ps --format "{{.Names}}" | Select-String "inventario-sqlserver"
if (-not $contenedorActivo) {
    Write-WARN "SQL Server no esta corriendo, omitiendo backup"
} else {
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $archivo   = "SistemaInventarioDB_${timestamp}_manual-deploy.bak"

    Write-Host "Creando backup: $archivo"
    docker exec inventario-sqlserver /opt/mssql-tools18/bin/sqlcmd `
        -S localhost -U sa -P "SistemaInventario77!" -C `
        -Q "BACKUP DATABASE [SistemaInventarioDB] TO DISK = N'/var/opt/mssql/backups/$archivo' WITH FORMAT, INIT, COMPRESSION"

    if ($LASTEXITCODE -ne 0) {
        Write-FAIL "Backup fallo. Abortando para proteger datos."
        exit 1
    }
    Write-OK "Backup creado: backups\$archivo"
}

# ─── Paso 2: Git pull ──────────────────────────────────────────
Write-Banner "Paso 2/5: Actualizar codigo"

git fetch origin
git reset --hard "origin/$Rama"

$commit  = git rev-parse --short HEAD
$mensaje = git log -1 --pretty=%s
Write-OK "Codigo en commit: $commit — $mensaje"

# ─── Paso 3: Build ─────────────────────────────────────────────
Write-Banner "Paso 3/5: Build Docker"

docker compose build --no-cache webapp
if ($LASTEXITCODE -ne 0) {
    Write-FAIL "Build de Docker fallo."
    exit 1
}
Write-OK "Build completado"

# ─── Paso 4: Deploy ────────────────────────────────────────────
Write-Banner "Paso 4/5: Deploy"

docker compose up -d --force-recreate --no-deps webapp
if ($LASTEXITCODE -ne 0) {
    Write-FAIL "Deploy fallo."
    exit 1
}
Write-OK "Contenedor reiniciado"

# ─── Paso 5: Health check ──────────────────────────────────────
Write-Banner "Paso 5/5: Verificar salud"

Write-Host "Esperando 35 segundos..."
Start-Sleep 35

docker compose ps

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
Write-Host "  Commit:  $commit"
Write-Host "  Cambio:  $mensaje"
Write-Host "  Fecha:   $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')"
Write-Host "  URL:     http://localhost:8080"
Write-Host ""

# ==============================================================
# setup-initial.ps1
# Clona el repositorio y hace el primer despliegue en la PC
# cliente (Windows). Ejecutar solo la PRIMERA VEZ.
# ==============================================================
# Uso (como Administrador):
#   powershell -ExecutionPolicy Bypass -File setup-initial.ps1 `
#     -GitHubRepo "TuUsuario/sistema-inventario" `
#     -RutaDestino "C:\SistemaInventario"
# ==============================================================

param(
    [Parameter(Mandatory = $true)]
    [string]$GitHubRepo,        # ej: TuUsuario/sistema-inventario

    [Parameter(Mandatory = $false)]
    [string]$RutaDestino = "C:\SistemaInventario",

    [Parameter(Mandatory = $false)]
    [string]$Rama = "master"
)

$ErrorActionPreference = "Stop"

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  INSTALACION INICIAL - Sistema Inventario"   -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

# Verificar Docker
try { docker version | Out-Null; Write-Host "[OK] Docker disponible" -ForegroundColor Green }
catch { Write-Error "Docker Desktop no esta instalado o no esta corriendo."; exit 1 }

# Verificar Git
try { git --version | Out-Null; Write-Host "[OK] Git disponible" -ForegroundColor Green }
catch { Write-Error "Git no esta instalado. Descarga desde https://git-scm.com/download/win"; exit 1 }

# Clonar repositorio
if (-not (Test-Path $RutaDestino)) {
    Write-Host "Clonando repositorio en $RutaDestino ..."
    git clone "https://github.com/$GitHubRepo" $RutaDestino
    Write-Host "[OK] Repositorio clonado" -ForegroundColor Green
} else {
    Write-Host "[!!] La ruta $RutaDestino ya existe, omitiendo clone." -ForegroundColor Yellow
}

Set-Location $RutaDestino

# Crear directorios necesarios
New-Item -ItemType Directory -Force -Path ".\backups"                  | Out-Null
New-Item -ItemType Directory -Force -Path ".\SistemaInventario\wwwroot\images\products" | Out-Null

# Primer levantamiento
Write-Host "Levantando el sistema con Docker Compose..."
docker compose up --build -d

Write-Host ""
Write-Host "Esperando que los servicios inicien (60s)..."
Start-Sleep 60

docker compose ps

Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "  SISTEMA INSTALADO" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host "  URL:          http://localhost:8080"
Write-Host "  Usuario:      00001"
Write-Host "  Contrasena:   Admin123!"
Write-Host "  Ruta:         $RutaDestino"
Write-Host ""
Write-Host "SIGUIENTE PASO: Instala el runner de GitHub Actions"
Write-Host "  cd $RutaDestino\scripts"
Write-Host "  .\install-runner.ps1 -GitHubRepo '$GitHubRepo' -Token TU_TOKEN"
Write-Host ""

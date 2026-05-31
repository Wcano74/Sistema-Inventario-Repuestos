# ==============================================================
# install-runner.ps1
# Script de instalacion del GitHub Actions Self-hosted Runner
# Ejecutar como ADMINISTRADOR en la PC Windows del cliente
# ==============================================================
# Uso:
#   1. Abrir PowerShell como Administrador
#   2. cd C:\SistemaInventario\scripts
#   3. .\install-runner.ps1
# ==============================================================

param(
    [Parameter(Mandatory = $true)]
    [string]$GitHubRepo,         # ej: TuUsuario/sistema-inventario

    [Parameter(Mandatory = $true)]
    [string]$Token,              # Token de registro (se obtiene en GitHub)

    [Parameter(Mandatory = $false)]
    [string]$RunnerName = "inventario-runner-windows",

    [Parameter(Mandatory = $false)]
    [string]$RunnerPath = "C:\actions-runner"
)

Write-Host "================================================"
Write-Host "  Instalacion GitHub Actions Runner - Windows"
Write-Host "================================================"
Write-Host ""

# Verificar que se ejecuta como Administrador
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
    [Security.Principal.WindowsBuiltInRole]::Administrator
)
if (-not $isAdmin) {
    Write-Error "Debes ejecutar este script como ADMINISTRADOR."
    exit 1
}

# Verificar que Docker esta disponible
try {
    docker version | Out-Null
    Write-Host "Docker: OK"
} catch {
    Write-Error "Docker no esta instalado o no esta corriendo. Instala Docker Desktop primero."
    exit 1
}

# Verificar que Git esta disponible
try {
    git --version | Out-Null
    Write-Host "Git: OK"
} catch {
    Write-Error "Git no esta instalado. Instala Git para Windows primero."
    exit 1
}

# Crear directorio del runner
if (-not (Test-Path $RunnerPath)) {
    New-Item -ItemType Directory -Force -Path $RunnerPath | Out-Null
    Write-Host "Directorio del runner creado: $RunnerPath"
}

Set-Location $RunnerPath

# Descargar la ultima version del runner
Write-Host ""
Write-Host "Descargando GitHub Actions Runner..."
$runnerVersion = "2.321.0"
$runnerUrl = "https://github.com/actions/runner/releases/download/v${runnerVersion}/actions-runner-win-x64-${runnerVersion}.zip"
$runnerZip = "$RunnerPath\actions-runner.zip"

Invoke-WebRequest -Uri $runnerUrl -OutFile $runnerZip -UseBasicParsing
Write-Host "Descarga completada."

# Extraer
Write-Host "Extrayendo archivos..."
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::ExtractToDirectory($runnerZip, $RunnerPath)
Remove-Item $runnerZip

# Configurar el runner
Write-Host ""
Write-Host "Configurando runner..."
$repoUrl = "https://github.com/$GitHubRepo"

.\config.cmd `
    --url $repoUrl `
    --token $Token `
    --name $RunnerName `
    --labels "self-hosted,windows,inventario" `
    --runnergroup "Default" `
    --work "_work" `
    --unattended

if ($LASTEXITCODE -ne 0) {
    Write-Error "Error al configurar el runner. Verifica el token y la URL del repositorio."
    exit 1
}

# Instalar como servicio de Windows
Write-Host ""
Write-Host "Instalando como servicio de Windows..."
.\svc.cmd install
.\svc.cmd start

Write-Host ""
Write-Host "================================================"
Write-Host "  RUNNER INSTALADO EXITOSAMENTE"
Write-Host "================================================"
Write-Host "  Nombre:    $RunnerName"
Write-Host "  Repo:      $repoUrl"
Write-Host "  Directorio: $RunnerPath"
Write-Host "  Estado:    Corriendo como servicio de Windows"
Write-Host "================================================"
Write-Host ""
Write-Host "Verifica en GitHub > Settings > Actions > Runners"
Write-Host "El runner debe aparecer con estado 'Online'."

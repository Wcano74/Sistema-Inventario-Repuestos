# Script de Instalación Automática - Control Librería
# Para Windows Server / Windows 10/11 Pro
# Requiere ejecutar como Administrador

#Requires -RunAsAdministrator

Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "Control Librería - Instalación Automática en IIS" -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host ""

# Variables de configuración
$AppName = "ControlLibreria"
$AppPoolName = "ControlLibreriaPool"
$SitePath = "C:\inetpub\wwwroot\$AppName"
$Port = 80
$DatabaseName = "ControlLibreriaDb"

# Función para verificar si un comando existe
function Test-CommandExists {
    param($command)
    $null = Get-Command $command -ErrorAction SilentlyContinue
    return $?
}

# Paso 1: Verificar prerrequisitos
Write-Host "📋 Paso 1: Verificando prerrequisitos..." -ForegroundColor Yellow

# Verificar .NET Runtime
Write-Host "  Verificando .NET 9.0 Runtime..." -NoNewline
$dotnetVersion = dotnet --version 2>$null
if ($dotnetVersion -like "9.*") {
    Write-Host " ✅ Instalado ($dotnetVersion)" -ForegroundColor Green
} else {
    Write-Host " ❌ No encontrado" -ForegroundColor Red
    Write-Host ""
    Write-Host "Por favor instalar .NET 9.0 Runtime + ASP.NET Core Hosting Bundle desde:" -ForegroundColor Red
    Write-Host "https://dotnet.microsoft.com/download/dotnet/9.0" -ForegroundColor Yellow
    exit 1
}

# Verificar IIS
Write-Host "  Verificando IIS..." -NoNewline
$iisService = Get-Service W3SVC -ErrorAction SilentlyContinue
if ($iisService) {
    Write-Host " ✅ Instalado" -ForegroundColor Green
} else {
    Write-Host " ❌ No instalado" -ForegroundColor Red
    Write-Host ""
    Write-Host "Instalando IIS..." -ForegroundColor Yellow
    
    Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole -All -NoRestart
    Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServer -All -NoRestart
    Enable-WindowsOptionalFeature -Online -FeatureName IIS-CommonHttpFeatures -All -NoRestart
    Enable-WindowsOptionalFeature -Online -FeatureName IIS-ApplicationDevelopment -All -NoRestart
    Enable-WindowsOptionalFeature -Online -FeatureName IIS-ManagementConsole -All -NoRestart
    
    Write-Host "✅ IIS instalado" -ForegroundColor Green
}

Write-Host ""

# Paso 2: Solicitar información de SQL Server
Write-Host "📊 Paso 2: Configuración de SQL Server" -ForegroundColor Yellow
Write-Host ""

$sqlServer = Read-Host "Servidor SQL Server (presione Enter para 'localhost')"
if ([string]::IsNullOrWhiteSpace($sqlServer)) {
    $sqlServer = "localhost"
}

Write-Host ""
Write-Host "Tipo de autenticación:" -ForegroundColor Cyan
Write-Host "1. Windows (Integrated Security) - Recomendado"
Write-Host "2. SQL Server (Usuario y contraseña)"
$authChoice = Read-Host "Seleccione opción (1 o 2)"

if ($authChoice -eq "2") {
    $sqlUser = Read-Host "Usuario SQL Server"
    $sqlPassword = Read-Host "Contraseña SQL Server" -AsSecureString
    $sqlPasswordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($sqlPassword))
    $connectionString = "Server=$sqlServer;Database=$DatabaseName;User Id=$sqlUser;Password=$sqlPasswordPlain;TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=False"
} else {
    $connectionString = "Server=$sqlServer;Database=$DatabaseName;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}

Write-Host ""

# Paso 3: Copiar archivos de aplicación
Write-Host "📁 Paso 3: Copiando archivos de aplicación..." -ForegroundColor Yellow

$sourcePath = Read-Host "Ruta de la carpeta 'Aplicacion' (ej: C:\Temp\Distribucion_Cliente\Aplicacion)"

if (-not (Test-Path $sourcePath)) {
    Write-Host "❌ La ruta especificada no existe: $sourcePath" -ForegroundColor Red
    exit 1
}

# Crear directorio si no existe
if (-not (Test-Path $SitePath)) {
    New-Item -ItemType Directory -Path $SitePath -Force | Out-Null
}

# Copiar archivos
Write-Host "  Copiando archivos a $SitePath..." -NoNewline
Copy-Item -Path "$sourcePath\*" -Destination $SitePath -Recurse -Force
Write-Host " ✅" -ForegroundColor Green

# Actualizar appsettings.json
Write-Host "  Actualizando configuración..." -NoNewline
$appsettingsPath = Join-Path $SitePath "appsettings.json"
$appsettings = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
$appsettings.ConnectionStrings.DefaultConnection = $connectionString
$appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
Write-Host " ✅" -ForegroundColor Green

Write-Host ""

# Paso 4: Configurar IIS
Write-Host "🌐 Paso 4: Configurando IIS..." -ForegroundColor Yellow

# Importar módulo IIS
Import-Module WebAdministration -ErrorAction SilentlyContinue

# Crear Application Pool
Write-Host "  Creando Application Pool..." -NoNewline
if (Test-Path "IIS:\AppPools\$AppPoolName") {
    Remove-WebAppPool -Name $AppPoolName
}

New-WebAppPool -Name $AppPoolName | Out-Null
Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name managedRuntimeVersion -Value ""
Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name startMode -Value "AlwaysRunning"
Write-Host " ✅" -ForegroundColor Green

# Crear sitio web
Write-Host "  Creando sitio web..." -NoNewline
if (Test-Path "IIS:\Sites\$AppName") {
    Remove-Website -Name $AppName
}

New-Website -Name $AppName `
    -PhysicalPath $SitePath `
    -ApplicationPool $AppPoolName `
    -Port $Port | Out-Null
Write-Host " ✅" -ForegroundColor Green

# Dar permisos a la carpeta
Write-Host "  Configurando permisos..." -NoNewline
$acl = "IIS AppPool\$AppPoolName"
icacls $SitePath /grant "$($acl):(OI)(CI)F" /T | Out-Null
Write-Host " ✅" -ForegroundColor Green

# Iniciar sitio
Write-Host "  Iniciando sitio web..." -NoNewline
Start-Website -Name $AppName
Write-Host " ✅" -ForegroundColor Green

Write-Host ""

# Paso 5: Configurar Firewall
Write-Host "🔥 Paso 5: Configurando Firewall..." -ForegroundColor Yellow

$firewallRule = Get-NetFirewallRule -DisplayName "Control Libreria HTTP" -ErrorAction SilentlyContinue
if (-not $firewallRule) {
    Write-Host "  Creando regla de firewall..." -NoNewline
    New-NetFirewallRule -DisplayName "Control Libreria HTTP" `
        -Direction Inbound `
        -Protocol TCP `
        -LocalPort $Port `
        -Action Allow | Out-Null
    Write-Host " ✅" -ForegroundColor Green
} else {
    Write-Host "  Regla de firewall ya existe ✅" -ForegroundColor Green
}

Write-Host ""

# Paso 6: Información de base de datos
Write-Host "📊 Paso 6: Información de Base de Datos" -ForegroundColor Yellow
Write-Host ""
Write-Host "IMPORTANTE: Debe ejecutar el script de base de datos manualmente:" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Abrir SQL Server Management Studio (SSMS)" -ForegroundColor White
Write-Host "2. Conectarse a: $sqlServer" -ForegroundColor White
Write-Host "3. Crear base de datos:" -ForegroundColor White
Write-Host "   CREATE DATABASE $DatabaseName" -ForegroundColor Yellow
Write-Host "   GO" -ForegroundColor Yellow
Write-Host "4. Ejecutar script: Scripts\DatabaseScripts_Complete.sql" -ForegroundColor White
Write-Host ""

$executeDbScript = Read-Host "¿Ya ejecutó el script de base de datos? (S/N)"
if ($executeDbScript -ne "S" -and $executeDbScript -ne "s") {
    Write-Host ""
    Write-Host "⚠️  Por favor ejecute el script de base de datos antes de continuar." -ForegroundColor Yellow
    Write-Host "   La aplicación no funcionará sin la base de datos." -ForegroundColor Yellow
}

Write-Host ""

# Resumen final
Write-Host "========================================================" -ForegroundColor Green
Write-Host "✅ INSTALACIÓN COMPLETADA EXITOSAMENTE" -ForegroundColor Green
Write-Host "========================================================" -ForegroundColor Green
Write-Host ""
Write-Host "Información de acceso:" -ForegroundColor Cyan
Write-Host "  URL Local:    http://localhost:$Port" -ForegroundColor White
Write-Host "  URL Red:      http://$env:COMPUTERNAME:$Port" -ForegroundColor White

# Obtener IP
$ip = (Get-NetIPAddress -AddressFamily IPv4 | Where-Object {$_.InterfaceAlias -notlike "*Loopback*"} | Select-Object -First 1).IPAddress
if ($ip) {
    Write-Host "  URL IP:       http://$ip:$Port" -ForegroundColor White
}

Write-Host ""
Write-Host "Credenciales de acceso:" -ForegroundColor Cyan
Write-Host "  Código:       00001" -ForegroundColor White
Write-Host "  Contraseña:   Admin123!" -ForegroundColor White
Write-Host ""
Write-Host "⚠️  IMPORTANTE: Cambiar la contraseña después del primer inicio" -ForegroundColor Yellow
Write-Host ""
Write-Host "Ubicación de archivos:" -ForegroundColor Cyan
Write-Host "  Aplicación:   $SitePath" -ForegroundColor White
Write-Host "  Logs:         $SitePath\logs" -ForegroundColor White
Write-Host ""
Write-Host "Comandos útiles:" -ForegroundColor Cyan
Write-Host "  Reiniciar IIS:        iisreset" -ForegroundColor White
Write-Host "  Ver logs:             Get-Content '$SitePath\logs\*.log' -Tail 50" -ForegroundColor White
Write-Host "  Detener sitio:        Stop-Website -Name $AppName" -ForegroundColor White
Write-Host "  Iniciar sitio:        Start-Website -Name $AppName" -ForegroundColor White
Write-Host ""
Write-Host "========================================================" -ForegroundColor Green
Write-Host ""

# Preguntar si desea abrir el navegador
$openBrowser = Read-Host "¿Desea abrir la aplicación en el navegador? (S/N)"
if ($openBrowser -eq "S" -or $openBrowser -eq "s") {
    Start-Process "http://localhost:$Port"
}

Write-Host ""
Write-Host "Instalación finalizada. ¡Gracias por usar Control Librería!" -ForegroundColor Green
Write-Host ""

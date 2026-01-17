# Guía de Despliegue en Windows - Control Librería

## 📍 Ubicación del Script de Base de Datos

**Ruta completa**:
```
/Volumes/Mac NVME/Control Librería/ControlLibrería/DatabaseScripts_Complete.sql
```

Este archivo contiene **755 líneas** con todo lo necesario para crear la base de datos completa.

---

## 🎯 Opciones de Despliegue

Tienes **3 opciones** para instalar la aplicación en Windows:

| Opción | Ventajas | Desventajas | Recomendado Para |
|--------|----------|-------------|------------------|
| **1. IIS** | Profesional, escalable, fácil gestión | Requiere configuración IIS | Múltiples usuarios en red |
| **2. Servicio Windows** | Siempre activo, no requiere IIS | Configuración más técnica | Servidor dedicado |
| **3. Kestrel Standalone** | Simple, rápido | Requiere ejecutar manualmente | Desarrollo/Testing |

> **✅ RECOMENDADO**: **Opción 1 (IIS)** para instalación profesional en cliente.

---

## 📦 OPCIÓN 1: Despliegue con IIS (RECOMENDADO)

### Requisitos Previos

#### En la Máquina del Cliente (Windows)
- Windows 10/11 Pro o Windows Server 2016+
- SQL Server 2019+ instalado
- .NET 9.0 Runtime (ASP.NET Core) instalado
- IIS con ASP.NET Core Hosting Bundle

### Paso 1: Publicar el Proyecto

**En tu Mac (desarrollo)**:

```bash
cd "/Volumes/Mac NVME/Control Librería/ControlLibrería/ControlLibrería"

# Publicar para producción
dotnet publish -c Release -o ./publish --runtime win-x64 --self-contained false
```

Esto creará una carpeta `publish` con todos los archivos necesarios.

**Archivos generados en**: `/Volumes/Mac NVME/Control Librería/ControlLibrería/ControlLibrería/publish/`

### Paso 2: Preparar Archivos para el Cliente

**Crear paquete de instalación**:

```bash
# Crear carpeta de distribución
mkdir -p "/Volumes/Mac NVME/Control Librería/ControlLibrería/Distribucion_Cliente"

# Copiar archivos publicados
cp -r "./publish" "/Volumes/Mac NVME/Control Librería/ControlLibrería/Distribucion_Cliente/Aplicacion"

# Copiar script de base de datos
cp "DatabaseScripts_Complete.sql" "/Volumes/Mac NVME/Control Librería/ControlLibrería/Distribucion_Cliente/"

# Copiar guías
cp INSTALACION_BASE_DATOS.md "/Volumes/Mac NVME/Control Librería/ControlLibrería/Distribucion_Cliente/"
cp GUIA_DESPLIEGUE_WINDOWS.md "/Volumes/Mac NVME/Control Librería/ControlLibrería/Distribucion_Cliente/"
```

**Comprimir para transferencia**:
```bash
cd "/Volumes/Mac NVME/Control Librería/ControlLibrería"
zip -r "ControlLibreria_Instalacion.zip" "Distribucion_Cliente/"
```

### Paso 3: Instalación en Windows (Cliente)

#### 3.1. Instalar Prerrequisitos

**A. Instalar .NET 9.0 Runtime**

1. Descargar desde: https://dotnet.microsoft.com/download/dotnet/9.0
2. Seleccionar: **ASP.NET Core Runtime 9.0.x - Windows Hosting Bundle**
3. Ejecutar instalador
4. Reiniciar el servidor

**B. Habilitar IIS**

En Windows (PowerShell como Administrador):
```powershell
# Habilitar IIS y características necesarias
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServer
Enable-WindowsOptionalFeature -Online -FeatureName IIS-CommonHttpFeatures
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpErrors
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ApplicationDevelopment
Enable-WindowsOptionalFeature -Online -FeatureName IIS-NetFxExtensibility45
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HealthAndDiagnostics
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpLogging
Enable-WindowsOptionalFeature -Online -FeatureName IIS-Security
Enable-WindowsOptionalFeature -Online -FeatureName IIS-RequestFiltering
Enable-WindowsOptionalFeature -Online -FeatureName IIS-Performance
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerManagementTools
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ManagementConsole
Enable-WindowsOptionalFeature -Online -FeatureName IIS-StaticContent
Enable-WindowsOptionalFeature -Online -FeatureName IIS-DefaultDocument
Enable-WindowsOptionalFeature -Online -FeatureName IIS-DirectoryBrowsing
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ASPNET45

# Reiniciar IIS
iisreset
```

#### 3.2. Instalar Base de Datos

1. **Abrir SQL Server Management Studio (SSMS)**
2. **Conectarse al servidor local**
3. **Crear base de datos**:
   ```sql
   CREATE DATABASE ControlLibreriaDb
   GO
   ```
4. **Ejecutar script**:
   - File → Open → `DatabaseScripts_Complete.sql`
   - Asegurarse de estar en `ControlLibreriaDb`
   - Ejecutar (F5)

#### 3.3. Configurar la Aplicación

1. **Extraer archivos**:
   - Descomprimir `ControlLibreria_Instalacion.zip`
   - Copiar carpeta `Aplicacion` a: `C:\inetpub\wwwroot\ControlLibreria`

2. **Editar configuración**:
   - Abrir: `C:\inetpub\wwwroot\ControlLibreria\appsettings.json`
   - Actualizar cadena de conexión:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ControlLibreriaDb;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> **💡 NOTA**: Si usas autenticación SQL Server en lugar de Windows:
> ```json
> "DefaultConnection": "Server=localhost;Database=ControlLibreriaDb;User Id=sa;Password=TU_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=False"
> ```

#### 3.4. Configurar IIS

**A. Crear Application Pool**

1. Abrir **IIS Manager** (Administrador de IIS)
2. Clic derecho en **Application Pools** → **Add Application Pool**
3. Configurar:
   - **Name**: `ControlLibreriaPool`
   - **.NET CLR version**: `No Managed Code`
   - **Managed pipeline mode**: `Integrated`
   - Clic **OK**

4. Clic derecho en `ControlLibreriaPool` → **Advanced Settings**:
   - **Identity**: `ApplicationPoolIdentity` (o cuenta específica con acceso a SQL Server)
   - **Start Mode**: `AlwaysRunning`
   - Clic **OK**

**B. Crear Sitio Web**

1. En IIS Manager, clic derecho en **Sites** → **Add Website**
2. Configurar:
   - **Site name**: `ControlLibreria`
   - **Application pool**: `ControlLibreriaPool`
   - **Physical path**: `C:\inetpub\wwwroot\ControlLibreria`
   - **Binding**:
     - Type: `http`
     - IP address: `All Unassigned`
     - Port: `80` (o `8080` si 80 está ocupado)
     - Host name: (dejar vacío o poner nombre del servidor)
   - Clic **OK**

3. **Dar permisos a la carpeta**:

En PowerShell como Administrador:
```powershell
# Dar permisos al Application Pool
icacls "C:\inetpub\wwwroot\ControlLibreria" /grant "IIS AppPool\ControlLibreriaPool:(OI)(CI)F" /T
```

**C. Configurar Firewall (si es necesario)**

```powershell
# Permitir tráfico HTTP en puerto 80
New-NetFirewallRule -DisplayName "Control Libreria HTTP" -Direction Inbound -Protocol TCP -LocalPort 80 -Action Allow
```

#### 3.5. Verificar Instalación

1. **Abrir navegador** en la máquina cliente
2. **Navegar a**: `http://localhost` o `http://NOMBRE_SERVIDOR`
3. **Debería aparecer** la página de login
4. **Login con**:
   - Código: `00001`
   - Contraseña: `Admin123!`

---

## 🔧 OPCIÓN 2: Servicio de Windows

### Ventajas
- Aplicación siempre activa
- Inicia automáticamente con Windows
- No requiere IIS

### Instalación

#### Paso 1: Publicar como Servicio

```bash
cd "/Volumes/Mac NVME/Control Librería/ControlLibrería/ControlLibrería"

dotnet publish -c Release -o ./publish-service --runtime win-x64 --self-contained true
```

#### Paso 2: Instalar en Windows

1. **Copiar archivos** a: `C:\ControlLibreria`

2. **Crear servicio** (PowerShell como Administrador):

```powershell
# Instalar NSSM (Non-Sucking Service Manager)
# Descargar de: https://nssm.cc/download

# O usar sc.exe (nativo de Windows)
sc.exe create "ControlLibreriaService" binPath= "C:\ControlLibreria\ControlLibrería.exe" start= auto
sc.exe description "ControlLibreriaService" "Sistema de Control de Librería"
sc.exe start "ControlLibreriaService"
```

3. **Configurar URL** en `appsettings.json`:

```json
{
  "Urls": "http://localhost:5000;http://*:5000",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ControlLibreriaDb;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

4. **Acceder**: `http://localhost:5000`

---

## 🚀 OPCIÓN 3: Kestrel Standalone (Desarrollo/Testing)

### Uso Rápido

1. **Copiar carpeta publicada** a Windows
2. **Abrir PowerShell** en la carpeta
3. **Ejecutar**:
   ```powershell
   .\ControlLibrería.exe
   ```
4. **Acceder**: `http://localhost:5000`

> ⚠️ **NO recomendado para producción** - La aplicación se cierra al cerrar la ventana.

---

## 📋 Checklist de Instalación Completa

### Preparación (En Mac)
- [ ] Ejecutar `dotnet publish` para generar archivos
- [ ] Copiar archivos a carpeta `Distribucion_Cliente`
- [ ] Incluir `DatabaseScripts_Complete.sql`
- [ ] Incluir guías de instalación
- [ ] Comprimir todo en ZIP

### En Cliente Windows
- [ ] Instalar SQL Server
- [ ] Instalar .NET 9.0 Runtime + Hosting Bundle
- [ ] Habilitar IIS (si usa Opción 1)
- [ ] Crear base de datos `ControlLibreriaDb`
- [ ] Ejecutar `DatabaseScripts_Complete.sql`
- [ ] Verificar 24 tablas y 4 procedimientos creados
- [ ] Extraer archivos de aplicación
- [ ] Actualizar `appsettings.json` con cadena de conexión
- [ ] Configurar IIS / Servicio Windows
- [ ] Dar permisos a carpeta de aplicación
- [ ] Configurar firewall (si es necesario)
- [ ] Probar acceso desde navegador
- [ ] Login con usuario admin (00001 / Admin123!)
- [ ] Cambiar contraseña del administrador
- [ ] Crear usuarios para empleados

---

## 🔒 Configuración de Seguridad

### SQL Server

**Dar permisos al Application Pool de IIS**:

```sql
-- Crear login para IIS AppPool
CREATE LOGIN [IIS APPPOOL\ControlLibreriaPool] FROM WINDOWS;
GO

USE ControlLibreriaDb;
GO

-- Crear usuario y dar permisos
CREATE USER [IIS APPPOOL\ControlLibreriaPool] FOR LOGIN [IIS APPPOOL\ControlLibreriaPool];
GO

ALTER ROLE db_owner ADD MEMBER [IIS APPPOOL\ControlLibreriaPool];
GO
```

### Firewall

```powershell
# Permitir SQL Server (si está en otra máquina)
New-NetFirewallRule -DisplayName "SQL Server" -Direction Inbound -Protocol TCP -LocalPort 1433 -Action Allow
```

---

## 🆘 Solución de Problemas

### Error: "HTTP Error 500.31 - Failed to load ASP.NET Core runtime"
**Solución**: Instalar ASP.NET Core Hosting Bundle y reiniciar servidor.

### Error: "Cannot connect to database"
**Solución**: 
1. Verificar que SQL Server esté ejecutándose
2. Verificar cadena de conexión en `appsettings.json`
3. Verificar permisos del Application Pool en SQL Server

### Error: "Access Denied" al acceder a archivos
**Solución**: Ejecutar comando `icacls` para dar permisos al Application Pool.

### La aplicación no inicia
**Solución**:
1. Revisar Event Viewer de Windows
2. Revisar logs en: `C:\inetpub\wwwroot\ControlLibreria\logs`
3. Verificar que .NET 9.0 Runtime esté instalado

---

## 📊 Acceso desde Otros Equipos en Red Local

### Configurar para Acceso en Red

1. **Obtener IP del servidor**:
   ```powershell
   ipconfig
   ```
   Ejemplo: `192.168.1.100`

2. **Configurar binding en IIS**:
   - En IIS Manager → Sites → ControlLibreria → Bindings
   - Agregar binding con IP específica

3. **En otros equipos**, acceder a:
   ```
   http://192.168.1.100
   ```

---

## 🔄 Actualización de la Aplicación

Para actualizar a una nueva versión:

1. **Detener sitio en IIS**
2. **Hacer backup** de `appsettings.json`
3. **Reemplazar archivos** en `C:\inetpub\wwwroot\ControlLibreria`
4. **Restaurar** `appsettings.json`
5. **Iniciar sitio en IIS**

---

## 📞 Comandos Útiles

```powershell
# Ver estado de IIS
Get-Service W3SVC

# Reiniciar IIS
iisreset

# Ver Application Pools
Get-IISAppPool

# Ver sitios web
Get-IISSite

# Ver logs de eventos
Get-EventLog -LogName Application -Source "IIS*" -Newest 20
```

---

**Versión**: 1.0  
**Fecha**: 2026-01-17  
**Sistema**: Control Librería - ASP.NET Core 9.0

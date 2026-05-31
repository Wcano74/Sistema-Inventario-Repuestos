# CI/CD con GitHub Actions (Self-hosted Runner)

tags: #cicd #github #deploy #windows #automatizacion

---

## Arquitectura del Pipeline

```
Tu PC (dev)                GitHub                  PC Cliente (Windows)
───────────                ──────                  ────────────────────
git push master  ──────►  Detecta push  ──────►  Self-hosted Runner
                           Dispara job            ├── 1. Backup BD
                                                  ├── 2. git pull
                                                  ├── 3. docker build
                                                  ├── 4. docker up
                                                  └── 5. Health check
```

**Ventajas de Self-hosted Runner:**
- La PC del cliente no necesita IP pública ni abrir puertos
- El runner se conecta a GitHub (salida, no entrada)
- Funciona detrás de NAT/firewall normal
- Corre como servicio de Windows (arranca automáticamente)

---

## Archivos Creados en el Proyecto

```
.github/
└── workflows/
    └── deploy.yml          ← Workflow de CI/CD (ya en el repo)

scripts/
├── install-runner.ps1      ← Instala el runner en Windows (ejecutar 1 sola vez)
├── setup-initial.ps1       ← Primera instalación en PC cliente
└── deploy-manual.ps1       ← Deploy manual sin CI/CD (emergencia)
```

---

## GUÍA DE INSTALACIÓN COMPLETA

### Paso 1 — Subir el workflow a GitHub

Desde tu Mac de desarrollo:

```bash
cd "Venta de repuestos"
git add .github/workflows/deploy.yml scripts/
git commit -m "ci: agregar pipeline de deploy automático"
git push origin master
```

> En GitHub verás el workflow pero fallará porque aún no hay runner. Eso es normal.

---

### Paso 2 — Obtener el Token de registro del Runner

1. Ve a tu repo en GitHub
2. **Settings** → **Actions** → **Runners** → **New self-hosted runner**
3. Selecciona: `Windows` / `x64`
4. Copia el token de registro (empieza con `AART...`)
   - ⚠️ El token expira en **1 hora**, genéralo cuando estés listo para instalar

---

### Paso 3 — Primera instalación en la PC del cliente

En la PC Windows del cliente, abrir **PowerShell como Administrador** y ejecutar:

```powershell
# Opción A: Si el repositorio ya está clonado en C:\SistemaInventario
#   (Ya se hizo la instalación con Docker)
# Solo instalar el runner:

cd C:\SistemaInventario\scripts

powershell -ExecutionPolicy Bypass -File install-runner.ps1 `
  -GitHubRepo "TuUsuario/nombre-del-repo" `
  -Token "AART...token-copiado-de-github..."
```

```powershell
# Opción B: Instalación desde cero (PC nueva o reinstalación)
# Descargar y ejecutar el script de setup inicial:

powershell -ExecutionPolicy Bypass -Command "
  Invoke-WebRequest -Uri 'https://raw.githubusercontent.com/TuUsuario/nombre-repo/master/scripts/setup-initial.ps1' `
    -OutFile 'C:\setup-initial.ps1'; `
  & 'C:\setup-initial.ps1' `
    -GitHubRepo 'TuUsuario/nombre-del-repo' `
    -RutaDestino 'C:\SistemaInventario'
"
```

---

### Paso 4 — Configurar variable DEPLOY_PATH en GitHub

1. Ve a tu repo en GitHub
2. **Settings** → **Variables** → **Actions** → **New repository variable**
3. Nombre: `DEPLOY_PATH`
4. Valor: `C:\SistemaInventario` (la ruta donde está el proyecto en Windows)
5. Guardar

> **¿Por qué una variable y no un secret?** La ruta de instalación no es dato sensible. Los secrets se usan para contraseñas y tokens.

---

### Paso 5 — Verificar que el Runner está activo

1. GitHub → Settings → Actions → Runners
2. El runner `inventario-runner-windows` debe aparecer en **verde** con estado `Idle`
3. Si aparece `Offline`, revisar el servicio de Windows:

```powershell
# Ver estado del servicio
Get-Service -Name "actions.runner.*"

# Reiniciar si está detenido
Get-Service -Name "actions.runner.*" | Restart-Service
```

---

### Paso 6 — Probar el pipeline

Haz cualquier cambio en el código y:

```bash
git add .
git commit -m "test: probar pipeline CI/CD"
git push origin master
```

Luego en GitHub → **Actions** → verás el workflow ejecutándose. Cada paso muestra logs en tiempo real.

---

## Flujo de Pasos del Pipeline

| # | Paso | Qué hace | Falla si... |
|---|------|----------|-------------|
| 1 | Validar config | Verifica que DEPLOY_PATH existe | La variable no está configurada |
| 2 | Backup BD | Crea `.bak` pre-deploy en `backups/` | SQL Server no responde (el deploy se aborta) |
| 3 | Actualizar código | `git fetch` + `git reset --hard` | No hay conexión a GitHub |
| 4 | Build Docker | `docker compose build webapp` | Error de compilación del proyecto |
| 5 | Deploy | `docker compose up -d webapp` | Error de Docker |
| 6 | Health check | Llama a `http://localhost:8080` | La app no levanta (muestra los logs) |
| 7 | Resumen | Siempre se ejecuta | — |

> **SQL Server nunca se reinicia** en el pipeline. Solo se reconstruye el contenedor `webapp`. Los datos están seguros en el volumen Docker `inventario-sqlserver-data`.

---

## Variables y Secretos Necesarios

### Variables de repositorio (Settings → Variables → Actions)

| Variable | Ejemplo | Descripción |
|----------|---------|-------------|
| `DEPLOY_PATH` | `C:\SistemaInventario` | Ruta donde está el proyecto en Windows |

### Secretos (no se necesitan por ahora)

El sistema actual no requiere secretos en GitHub porque las credenciales de DB están en docker-compose.yml. Si en el futuro quieres externalizarlas, agrega:

| Secret | Descripción |
|--------|-------------|
| `DB_SA_PASSWORD` | Contraseña del SA de SQL Server |

---

## Comandos Útiles para el Runner (en la PC Windows)

```powershell
# Ver si el servicio del runner está corriendo
Get-Service -Name "actions.runner.*" | Select-Object Name, Status

# Iniciar el runner
Get-Service -Name "actions.runner.*" | Start-Service

# Detener el runner (detiene el CI/CD temporalmente)
Get-Service -Name "actions.runner.*" | Stop-Service

# Ver logs del runner
Get-Content "C:\actions-runner\_diag\*.log" -Tail 50

# Desinstalar el runner (si necesitas reinstalar)
cd C:\actions-runner
.\svc.cmd stop
.\svc.cmd uninstall
.\config.cmd remove --token TU_TOKEN
```

---

## Despliegue Manual de Emergencia

Si el CI/CD falla o necesitas forzar una actualización sin push:

```powershell
# En PowerShell en la PC del cliente
cd C:\SistemaInventario
.\scripts\deploy-manual.ps1
```

Este script hace exactamente los mismos pasos que el pipeline automático.

---

## Rollback (Revertir a Versión Anterior)

### Opción 1: Revertir el código y hacer push

```bash
# Ver commits recientes
git log --oneline -10

# Revertir al commit anterior (crea un nuevo commit de reversal)
git revert HEAD
git push origin master
# → GitHub Actions se dispara automáticamente con el código anterior
```

### Opción 2: Restaurar backup de BD (si los datos se corrompieron)

```powershell
# En la PC Windows del cliente
cd C:\SistemaInventario

# Ver backups disponibles
Get-ChildItem .\backups\*.bak | Sort-Object LastWriteTime -Descending

# Detener la app (no la BD)
docker compose stop webapp

# Restaurar backup específico
docker exec inventario-sqlserver /opt/mssql-tools18/bin/sqlcmd `
  -S localhost -U sa -P "SistemaInventario77!" -C `
  -Q "
    USE master;
    ALTER DATABASE SistemaInventarioDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    RESTORE DATABASE SistemaInventarioDB FROM DISK = N'/var/opt/mssql/backups/NOMBRE_BACKUP.bak' WITH REPLACE;
    ALTER DATABASE SistemaInventarioDB SET MULTI_USER;
  "

# Levantar la app
docker compose start webapp
```

---

## Solución de Problemas

### El runner aparece Offline en GitHub

```powershell
# Reiniciar el servicio de Windows
Get-Service -Name "actions.runner.*" | Restart-Service

# Si sigue offline, verificar conectividad a GitHub
Test-NetConnection github.com -Port 443
```

### El pipeline falla en "Build Docker"

```powershell
# Ver error completo en los logs de GitHub Actions
# O ejecutar manualmente en la PC:
cd C:\SistemaInventario
docker compose build webapp
# El error aparecerá en pantalla
```

### El pipeline falla en "Health check"

```powershell
# Ver logs de la aplicación
docker logs inventario-webapp --tail 50

# Reiniciar manualmente
docker compose restart webapp
Start-Sleep 30
Invoke-WebRequest http://localhost:8080 -UseBasicParsing
```

### Error "DEPLOY_PATH no está configurado"

- Ir a GitHub → Settings → Variables → Actions
- Verificar que existe la variable `DEPLOY_PATH`
- El valor debe ser exactamente: `C:\SistemaInventario` (sin barra al final)

---

## Ver también

- [[09 - Despliegue e Instalación]]
- [[11 - Respaldos]]
- [[10 - Mantenimiento y Queries]]

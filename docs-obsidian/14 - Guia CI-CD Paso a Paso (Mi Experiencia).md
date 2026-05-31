# Guía CI/CD con GitHub Actions — Paso a Paso

tags: #cicd #github #deploy #runner #automatizacion #guia-personal

> **Para qué sirve esto:** Cada vez que hago `git push` al repo, el sistema en la PC del cliente se actualiza automáticamente — hace backup, descarga el código nuevo, reconstruye Docker y verifica que todo funcione.

---

## La Idea en Una Imagen

```
Yo hago push        GitHub lo detecta       Runner en la PC
─────────────       ─────────────────       ───────────────
git push main  ──►  Dispara el pipeline ──► 1. Backup de BD
                                            2. git pull
                                            3. docker build
                                            4. docker up
                                            5. Verifica HTTP 200
```

El **runner** es un pequeño programa que corre en la PC (Mac o Windows), escucha GitHub y ejecuta los pasos cuando hay un push. No se necesita IP pública ni abrir puertos — el runner llama a GitHub, no al revés.

---

## Archivos que se crean en el proyecto

```
.github/
└── workflows/
    └── deploy.yml          ← El cerebro del CI/CD

scripts/
├── install-runner.ps1      ← Para instalar el runner en Windows
├── deploy-manual.ps1       ← Deploy de emergencia sin CI/CD
└── setup-initial.ps1       ← Primera instalación en PC nueva
```

---

## Lo que hace cada paso del pipeline (`deploy.yml`)

| Paso | Nombre | Qué hace | Si falla... |
|------|--------|----------|-------------|
| 1 | Validar config | Verifica que `DEPLOY_PATH` existe | Para todo |
| 2 | Backup BD | Crea `.bak` en `backups/` | Para todo (protege datos) |
| 3 | Actualizar código | `git fetch` + `git reset --hard` | Para todo |
| 4 | Build Docker | Reconstruye solo `webapp` | Para todo |
| 5 | Deploy | Reinicia el contenedor `webapp` | Para todo |
| 6 | Health check | Llama `http://localhost:8080` | Muestra logs y falla |
| 7 | Resumen | Siempre corre, muestra resultado final | — |

> **SQL Server nunca se reinicia.** Solo se reconstruye `webapp`. Los datos están seguros.

---

## INSTALACIÓN EN MAC (para desarrollo y pruebas)

### Requisitos previos

- [ ] Docker Desktop corriendo
- [ ] Git instalado
- [ ] PowerShell Core instalado

### 1. Instalar PowerShell Core en Mac (Apple Silicon)

```bash
# Opción A — Homebrew (puede que el nombre haya cambiado)
brew install --cask powershell@preview

# Si el comando pwsh no aparece, agregar al PATH manualmente
# Buscar dónde quedó instalado
find /usr/local /opt/homebrew /Applications -name "pwsh" 2>/dev/null

# Agregar la ruta encontrada al PATH (reemplazar con la ruta real)
echo 'export PATH="/usr/local/microsoft/powershell/7-preview:$PATH"' >> ~/.zshrc
source ~/.zshrc

# Verificar
pwsh --version
```

> **Nota personal:** En mi Mac Apple Silicon el cask se llama `powershell@preview` y quedó en `/usr/local/microsoft/powershell/7-preview/pwsh`

### 2. Instalar el runner de GitHub Actions en Mac

**Paso 1 — Obtener el token en GitHub:**
Repo → **Settings → Actions → Runners → New self-hosted runner** → seleccionar **macOS** → copiar el token (empieza con `AART...`, expira en 1 hora)

**Paso 2 — Descargar y configurar:**
```bash
# Crear carpeta
mkdir ~/actions-runner && cd ~/actions-runner

# Descargar (GitHub te da el comando exacto con la versión actual)
curl -o actions-runner-osx-arm64.tar.gz -L \
  https://github.com/actions/runner/releases/download/v2.X.X/actions-runner-osx-arm64-2.X.X.tar.gz

tar xzf ./actions-runner-osx-arm64.tar.gz

# Configurar — el --labels "inventario" es CLAVE
./config.sh \
  --url https://github.com/USUARIO/REPO \
  --token AART...TOKEN \
  --name "inventario-dev-mac" \
  --labels "inventario" \
  --unattended
```

> **⚠️ Lo más importante:** El flag `--labels "inventario"` conecta este runner con el workflow. Sin ese label, GitHub no manda los jobs aquí.

**Paso 3 — Iniciar el runner:**
```bash
# Buscar dónde quedó si no aparece run.sh
find ~ -name "run.sh" 2>/dev/null

# Iniciar (dejar esta terminal abierta)
cd /Users/wcano/actions-runner/actions-runner
./run.sh
# Debe decir: "Listening for Jobs"
```

### 3. Configurar la variable DEPLOY_PATH en GitHub

Repo → **Settings → Secrets and variables → Actions → pestaña "Variables" → New repository variable**

| Campo | Valor para Mac |
|-------|----------------|
| Name | `DEPLOY_PATH` |
| Value | `/Volumes/Mac NVME/Venta de repuestos` |

> **⚠️ Ojo:** La pestaña se llama "Variables" (no "Secrets"). Están en la misma sección pero son tabs diferentes.

### 4. Probar el pipeline

```bash
cd "/Volumes/Mac NVME/Venta de repuestos"
git add .
git commit -m "test: probar pipeline CI/CD"
git push origin main
```

Ir a GitHub → **Actions** y ver el pipeline en tiempo real. Debe completar en ~2-3 minutos con todos los pasos en verde.

---

## INSTALACIÓN EN WINDOWS (PC del cliente — producción)

### Requisitos previos en Windows

- [ ] Docker Desktop instalado con WSL2
- [ ] Git for Windows instalado
- [ ] PowerShell 5+ (ya viene en Windows 10/11)
- [ ] El proyecto clonado en `C:\SistemaInventario`

### 1. Clonar el proyecto (primera vez)

```powershell
git clone https://github.com/Wcano74/Sistema-Inventario-Repuestos C:\SistemaInventario
cd C:\SistemaInventario
docker compose up --build -d
```

### 2. Instalar el runner en Windows

**Obtener token:** GitHub → Settings → Actions → Runners → New self-hosted runner → Windows

```powershell
# Abrir PowerShell como ADMINISTRADOR
cd C:\SistemaInventario\scripts

powershell -ExecutionPolicy Bypass -File install-runner.ps1 `
  -GitHubRepo "Wcano74/Sistema-Inventario-Repuestos" `
  -Token "AART...TOKEN"
```

El script instala el runner como **servicio de Windows** (arranca automáticamente con el sistema).

### 3. Actualizar DEPLOY_PATH en GitHub

Cambiar el valor de la variable `DEPLOY_PATH`:
- Repo → Settings → Secrets and variables → Actions → Variables
- Editar `DEPLOY_PATH` → valor: `C:\SistemaInventario`

### 4. Verificar que el runner esté activo

GitHub → Settings → Actions → Runners → debe aparecer `inventario-prod-windows` en verde (Idle)

```powershell
# Si está offline, en Windows:
Get-Service -Name "actions.runner.*" | Start-Service
```

---

## Comandos útiles del runner

### Mac
```bash
# Iniciar runner
cd /Users/wcano/actions-runner/actions-runner && ./run.sh

# Ver si está corriendo (en otra terminal)
ps aux | grep run.sh
```

### Windows
```powershell
# Ver estado del servicio
Get-Service -Name "actions.runner.*"

# Iniciar / detener / reiniciar
Get-Service -Name "actions.runner.*" | Start-Service
Get-Service -Name "actions.runner.*" | Stop-Service
Get-Service -Name "actions.runner.*" | Restart-Service

# Ver logs del runner
Get-Content "C:\actions-runner\_diag\*.log" -Tail 50
```

---

## Cómo funciona el label "inventario"

El label es lo que conecta el workflow con el runner correcto:

```yaml
# En deploy.yml
runs-on: [self-hosted, inventario]
```

```
Runner Mac    → labels: self-hosted, macOS, X64, inventario  ✅ recibe el job
Runner Win    → labels: self-hosted, Windows, X64, inventario ✅ recibe el job
Otro runner   → labels: self-hosted, macOS, X64              ❌ no recibe el job
```

Si hay DOS runners activos con el label `inventario` al mismo tiempo, GitHub manda el job al que esté disponible (cualquiera de los dos). Para evitar esto: solo tener UN runner activo a la vez (Mac para dev, Windows para prod).

---

## Cambiar entre ambientes (Mac ↔ Windows)

No se cambia nada en el código. Solo:

1. **Detener el runner de Mac** (Ctrl+C en la terminal del run.sh)
2. **Iniciar el runner de Windows** (el servicio ya corre automático)
3. **Actualizar DEPLOY_PATH** en GitHub al valor de Windows (`C:\SistemaInventario`)

Para volver a Mac: al revés.

---

## Solución de problemas que me pasaron

### `pwsh: command not found` en Mac
```bash
# Buscar dónde está instalado
find /usr /opt /Applications -name "pwsh" 2>/dev/null

# Agregar al PATH con la ruta encontrada
echo 'export PATH="/ruta/encontrada:$PATH"' >> ~/.zshrc && source ~/.zshrc
```

### `./run.sh: No such file or directory`
```bash
# El runner quedó en una carpeta diferente
find ~ -name "run.sh" 2>/dev/null
# Usar la ruta completa que aparezca
```

### El pipeline no se dispara al hacer push
- Verificar que el runner esté corriendo (`Listening for Jobs`)
- Verificar que el label del runner sea exactamente `inventario`
- GitHub → Settings → Actions → Runners → ver el estado

### El runner aparece Offline en GitHub
- Significa que el proceso `run.sh` no está corriendo
- Mac: volver a ejecutar `./run.sh`
- Windows: `Get-Service "actions.runner.*" | Start-Service`

---

## Resultado final

Cuando funciona correctamente, GitHub Actions muestra:

```
✅ Validar configuracion          0s
✅ Backup de base de datos        15s
✅ Actualizar codigo              5s
✅ Build Docker                   1m 20s
✅ Deploy                         10s
✅ Verificar salud del sistema    40s
✅ Resumen del deploy             0s

Duración total: ~2 min 21s
Sistema respondiendo: HTTP 200
```

---

## Checklist para la próxima instalación

### Mac (dev/test)
- [ ] Instalar PowerShell Core y agregar al PATH
- [ ] Descargar runner de GitHub (macOS)
- [ ] Configurar con `--labels "inventario"` y `--name "inventario-dev-mac"`
- [ ] Ejecutar `./run.sh` — ver "Listening for Jobs"
- [ ] Crear variable `DEPLOY_PATH` en GitHub → Secrets and variables → Actions → Variables
- [ ] Hacer push de prueba y ver Actions en GitHub

### Windows (producción)
- [ ] Clonar repo en `C:\SistemaInventario`
- [ ] Levantar Docker: `docker compose up --build -d`
- [ ] Ejecutar `install-runner.ps1` como Administrador con el token de GitHub
- [ ] Actualizar `DEPLOY_PATH` en GitHub a `C:\SistemaInventario`
- [ ] Verificar runner Online en GitHub → Settings → Actions → Runners
- [ ] Hacer push de prueba

---

## Ver también

- [[13 - CI-CD GitHub Actions]] — Documentación técnica completa
- [[09 - Despliegue e Instalación]] — Cómo levantar el sistema con Docker
- [[11 - Respaldos]] — Sistema de backups

---
*Documentado: 2026-05-31*
*Tiempo de implementación: ~45 minutos (incluyendo troubleshooting)*
*Pipeline probado en: Mac Apple Silicon (M-series) + Docker Desktop*

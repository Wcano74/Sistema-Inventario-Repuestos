# Sistema de Inventario

Sistema de gestión de inventario, ventas, compras y caja registradora. Construido con **ASP.NET Core 9** y **SQL Server**. Diseñado para ser genérico y reutilizable en cualquier rubro de negocio (repuestos, librerías, ferreterías, etc.).

## Características

- 📦 **Inventario**: Productos, categorías, proveedores, stock mínimo, código de barras
- 🛒 **Punto de Venta (POS)**: Ventas rápidas con búsqueda de productos
- 💰 **Caja Registradora**: Apertura/cierre de caja, control de efectivo
- 📋 **Órdenes de Compra**: Gestión de pedidos a proveedores
- 🔄 **Devoluciones**: Sistema completo de devoluciones parciales y totales
- 👥 **Clientes**: Registro y seguimiento de clientes
- 📊 **Reportes**: Dashboard con ventas diarias, semanales, productos más vendidos
- 🔐 **Roles y Permisos**: Admin y Vendedor con permisos configurables
- 💸 **Egresos**: Control de gastos por categoría

## Requisitos Previos

- **Docker** y **Docker Compose** instalados
- Mínimo **2 GB de RAM** disponible (requerido por SQL Server)

## Instalación en una nueva máquina (Windows, Linux, Mac)

Una de las grandes ventajas de Docker es la **portabilidad**. Para instalar este sistema en otra computadora (por ejemplo, una con Windows), sigue estos pasos:

1. **Instalar Docker**: Descarga e instala **Docker Desktop** desde [docker.com](https://www.docker.com/products/docker-desktop/).
2. **Copiar archivos**: Copia la carpeta del proyecto (la que contiene el archivo `docker-compose.yml`) a la nueva máquina.
3. **Ejecutar**: Abre una terminal (PowerShell o CMD en Windows) en esa carpeta y ejecuta:
   ```bash
   docker compose up -d
   ```
4. **¡Listo!**: El sistema estará disponible en `http://localhost:8080`. Docker se encargará de descargar SQL Server y configurar la aplicación automáticamente.

## Despliegue con Docker

### 1. Clonar el repositorio

```bash
git clone <url-del-repositorio>
cd "Venta de repuestos"
```

### 2. Levantar los servicios

```bash
docker compose up --build -d
```

Esto levantará:
- **SQL Server 2022 Express** en el puerto `1433`
- **Aplicación Web** en el puerto `8080`

### 3. Acceder al sistema

Abrir en el navegador: **http://localhost:8080**

### Credenciales por defecto

| Campo | Valor |
|-------|-------|
| Código de empleado | `00001` |
| Contraseña | `Admin123!` |
| Rol | Administrador |

## Conexión a la Base de Datos

El sistema utiliza **SQL Server 2022** dentro de un contenedor Docker. Puedes conectarte desde tu máquina usando estas credenciales:

- **Servidor/Host**: `localhost` o `127.0.0.1`
- **Puerto**: `1433`
- **Usuario**: `sa`
- **Contraseña**: `SistemaInventario77!`
- **Base de Datos**: `SistemaInventarioDB`

### Herramientas Recomendadas (en Mac)

1. **Azure Data Studio**: (Recomendado) Es la versión ligera y moderna de Microsoft para Mac/Linux.
2. **VS Code**: Con la extensión **"SQL Server (mssql)"**.
3. **DBeaver**: Una herramienta universal excelente para múltiples bases de datos.

### Conexión desde VS Code

1. Instala la extensión **ms-mssql.mssql**.
2. Presiona `Cmd+Shift+P` y busca **SQL Server: Connect**.
3. Ingresa los datos de conexión mencionados arriba.

## Comandos Útiles

```bash
# Levantar servicios en segundo plano
docker compose up -d

# Ver logs en tiempo real
docker compose logs -f

# Ver solo logs de la aplicación
docker compose logs -f webapp

# Detener servicios
docker compose down

# Detener y eliminar todos los datos (¡cuidado!)
docker compose down -v

# Reconstruir después de cambios en el código
docker compose up --build -d
```

## Configuración

### Variables de Entorno

Puedes personalizar la conexión a la base de datos editando el archivo `docker-compose.yml`:

| Variable | Descripción | Valor por defecto |
|----------|-------------|-------------------|
| `ConnectionStrings__DefaultConnection` | Cadena de conexión a SQL Server | Configurada para el contenedor local |
| `MSSQL_SA_PASSWORD` | Contraseña del usuario SA de SQL Server | `SistemaInventario77!` |
| `ASPNETCORE_ENVIRONMENT` | Entorno de ejecución | `Production` |

### Personalizar el Nombre del Negocio

Después de iniciar sesión como administrador, ve a **Configuración** y cambia el valor de `NombreNegocio` al nombre de tu empresa.

## Estructura del Proyecto

```
SistemaInventario/
├── Controllers/       # Controladores MVC
├── Data/              # DbContext e inicializador de datos
├── Models/
│   ├── Entities/      # Modelos de la base de datos
│   └── ViewModels/    # Modelos para las vistas
├── Services/          # Servicios de la aplicación
├── Views/             # Vistas Razor (.cshtml)
├── wwwroot/           # Archivos estáticos (CSS, JS)
└── Program.cs         # Punto de entrada
```

## Desarrollo Local (sin Docker)

Si prefieres ejecutar sin Docker, necesitas:

1. **.NET 9 SDK** instalado
2. **SQL Server** accesible (local o remoto)
3. Actualizar la cadena de conexión en `appsettings.json`
4. Ejecutar:

```bash
cd SistemaInventario
dotnet run
```

## Licencia

© 2026 Sistema de Inventario. Todos los derechos reservados.

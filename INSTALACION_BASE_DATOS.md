# Guía de Instalación - Base de Datos Control Librería

## 📋 Requisitos Previos

### Software Necesario
- **SQL Server 2019 o superior** (Express, Standard o Enterprise)
- **SQL Server Management Studio (SSMS)** - Recomendado para ejecutar scripts
- **Usuario con permisos de administrador** en SQL Server

### Información Requerida
- Nombre del servidor SQL Server
- Credenciales de autenticación (Windows o SQL Server)
- Nombre de la base de datos a crear (sugerido: `ControlLibreriaDb`)

## 🚀 Proceso de Instalación

### Paso 1: Preparar el Entorno

1. **Verificar que SQL Server esté ejecutándose**
   ```powershell
   # En PowerShell (Windows)
   Get-Service -Name MSSQLSERVER
   ```

2. **Abrir SQL Server Management Studio (SSMS)**
   - Conectarse al servidor SQL Server del cliente
   - Usar autenticación Windows o SQL Server según configuración

### Paso 2: Crear la Base de Datos

**Opción A: Usando SSMS (Recomendado)**

1. En SSMS, clic derecho en **Databases** → **New Database**
2. Nombre: `ControlLibreriaDb`
3. Dejar configuraciones por defecto
4. Clic en **OK**

**Opción B: Usando Script SQL**

```sql
-- Ejecutar en SSMS (Query Window)
CREATE DATABASE ControlLibreriaDb
GO

USE ControlLibreriaDb
GO
```

### Paso 3: Ejecutar el Script de Instalación

> **⚠️ IMPORTANTE**: Ejecutar el script completo en una sola transacción.

1. **Abrir el archivo de script**
   - Ubicación: `DatabaseScripts_Complete.sql`
   - En SSMS: File → Open → File → Seleccionar `DatabaseScripts_Complete.sql`

2. **Verificar la base de datos activa**
   ```sql
   -- Asegurarse de estar en la base de datos correcta
   USE ControlLibreriaDb
   GO
   ```

3. **Ejecutar el script completo**
   - Clic en **Execute** (F5) o botón ▶️
   - **Tiempo estimado**: 30-60 segundos
   - Esperar mensaje: "Commands completed successfully"

### Paso 4: Verificar la Instalación

Ejecutar los siguientes comandos de verificación:

```sql
-- 1. Verificar que todas las tablas se crearon
SELECT COUNT(*) AS TotalTablas 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
-- Resultado esperado: 24 tablas

-- 2. Verificar procedimientos almacenados
SELECT name 
FROM sys.procedures 
WHERE type = 'P'
ORDER BY name
-- Resultado esperado: 4 procedimientos
-- sp_GetDailySales
-- sp_GetLowStockProducts
-- sp_GetTopSellingProducts
-- sp_GetWeeklySales

-- 3. Verificar tabla de migraciones
SELECT COUNT(*) AS MigracionesAplicadas 
FROM __EFMigrationsHistory
-- Resultado esperado: 17 migraciones

-- 4. Verificar datos iniciales
SELECT * FROM AspNetRoles
-- Resultado esperado: 2 roles (Admin, Vendedor)

SELECT * FROM Customers
-- Resultado esperado: 1 cliente (Cliente General)

SELECT * FROM SystemConfigurations
-- Resultado esperado: 13 configuraciones
```

### Paso 5: Crear Usuario Administrador

El script ya incluye la creación del usuario administrador, pero debe ejecutarse desde la aplicación en el primer inicio. Los datos por defecto son:

- **Código de Empleado**: `00001`
- **Contraseña**: `Admin123!`
- **Rol**: Administrador

> **📝 NOTA**: Se recomienda cambiar la contraseña después del primer inicio de sesión.

## 🔧 Configuración de la Aplicación

### Actualizar Cadena de Conexión

Editar el archivo `appsettings.json` en la aplicación:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=NOMBRE_SERVIDOR;Database=ControlLibreriaDb;User Id=USUARIO;Password=CONTRASEÑA;TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=False"
  }
}
```

**Reemplazar**:
- `NOMBRE_SERVIDOR`: Nombre o IP del servidor SQL Server
- `USUARIO`: Usuario de SQL Server
- `CONTRASEÑA`: Contraseña del usuario

**Ejemplos de cadenas de conexión**:

```json
// Autenticación Windows (Recomendado para red local)
"DefaultConnection": "Server=SERVIDOR_LOCAL;Database=ControlLibreriaDb;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true"

// Autenticación SQL Server
"DefaultConnection": "Server=192.168.1.100,1433;Database=ControlLibreriaDb;User Id=sa;Password=MiPassword123;TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=False"

// SQL Server Express (Instancia local)
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=ControlLibreriaDb;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

## 📊 Estructura de la Base de Datos

### Tablas Principales (24 tablas)

#### Autenticación y Usuarios (7 tablas)
- `AspNetUsers` - Usuarios del sistema
- `AspNetRoles` - Roles (Admin, Vendedor)
- `AspNetUserRoles` - Relación usuarios-roles
- `AspNetUserClaims`, `AspNetRoleClaims`, `AspNetUserLogins`, `AspNetUserTokens`

#### Gestión de Productos (4 tablas)
- `Products` - Productos
- `Categories` - Categorías
- `Suppliers` - Proveedores
- `ProductHistories` - Historial de movimientos

#### Ventas (4 tablas)
- `Sales` - Ventas
- `SaleDetails` - Detalles de ventas
- `Customers` - Clientes
- `CashRegisters` - Cajas registradoras

#### Devoluciones (2 tablas)
- `SaleRefunds` - Devoluciones
- `SaleRefundDetails` - Detalles de devoluciones

#### Compras (2 tablas)
- `PurchaseOrders` - Órdenes de compra
- `PurchaseOrderItems` - Items de órdenes

#### Inventario (3 tablas)
- `Adjustments` - Ajustes
- `InventoryCycles` - Ciclos de inventario
- `InventoryCounts` - Conteos

#### Egresos (2 tablas)
- `Expenses` - Gastos
- `ExpenseCategories` - Categorías de gastos

#### Sistema (1 tabla)
- `SystemConfigurations` - Configuraciones

### Procedimientos Almacenados (4)

1. **sp_GetDailySales** - Ventas del día
2. **sp_GetLowStockProducts** - Productos con stock bajo
3. **sp_GetTopSellingProducts** - Top 5 productos más vendidos
4. **sp_GetWeeklySales** - Ventas de últimos 7 días

## ✅ Checklist de Instalación

- [ ] SQL Server instalado y ejecutándose
- [ ] Base de datos `ControlLibreriaDb` creada
- [ ] Script `DatabaseScripts_Complete.sql` ejecutado exitosamente
- [ ] Verificación de tablas completada (24 tablas)
- [ ] Verificación de procedimientos completada (4 procedimientos)
- [ ] Verificación de migraciones completada (17 migraciones)
- [ ] Datos iniciales verificados (roles, cliente, configuraciones)
- [ ] Cadena de conexión actualizada en `appsettings.json`
- [ ] Aplicación iniciada correctamente
- [ ] Login con usuario admin exitoso (00001 / Admin123!)

## 🔒 Seguridad

### Recomendaciones

1. **Cambiar contraseña del administrador** después del primer inicio
2. **Crear usuarios específicos** para cada empleado
3. **Configurar backups automáticos** de la base de datos
4. **Restringir acceso** a la base de datos solo a la aplicación
5. **Usar autenticación Windows** cuando sea posible

### Backup de la Base de Datos

```sql
-- Crear backup completo
BACKUP DATABASE ControlLibreriaDb
TO DISK = 'C:\Backups\ControlLibreriaDb_Full.bak'
WITH FORMAT, INIT, NAME = 'Full Backup of ControlLibreriaDb';
GO
```

## 🆘 Solución de Problemas

### Error: "Cannot open database"
**Solución**: Verificar que la base de datos existe y el usuario tiene permisos.

### Error: "Login failed for user"
**Solución**: Verificar credenciales en la cadena de conexión.

### Error: "A network-related or instance-specific error"
**Solución**: 
- Verificar que SQL Server esté ejecutándose
- Verificar firewall de Windows
- Verificar que TCP/IP esté habilitado en SQL Server Configuration Manager

### Error: "Object already exists"
**Solución**: La base de datos ya tiene datos. Eliminar y volver a crear, o ejecutar solo las migraciones faltantes.

## 📞 Soporte

Para problemas durante la instalación:
1. Verificar logs de SQL Server
2. Verificar logs de la aplicación
3. Revisar este documento completamente
4. Contactar al equipo de desarrollo

---

**Versión del Script**: Compatible con todas las migraciones hasta 2026-01-17
**Última actualización**: 2026-01-17

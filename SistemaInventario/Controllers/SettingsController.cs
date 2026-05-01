using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using SistemaInventario.Data;
using SistemaInventario.Models.ViewModels;
using SistemaInventario.Models.Entities;
using SistemaInventario.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace SistemaInventario.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("configuracion")]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfigurationService _configService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHostApplicationLifetime _appLifetime;

        public SettingsController(ApplicationDbContext context, IConfigurationService configService, UserManager<ApplicationUser> userManager, IHostApplicationLifetime appLifetime)
        {
            _context = context;
            _configService = configService;
            _userManager = userManager;
            _appLifetime = appLifetime;
        }

        // GET: /configuracion/sistema
        [Route("sistema")]
        public async Task<IActionResult> Index()
        {
            var model = new SystemSettingsViewModel
            {
                // General
                NombreNegocio = await _configService.GetConfigurationAsync("NombreNegocio", "Sistema de Inventario"),
                DireccionNegocio = await _configService.GetConfigurationAsync("DireccionNegocio", ""),
                TelefonoNegocio = await _configService.GetConfigurationAsync("TelefonoNegocio", ""),
                NITNegocio = await _configService.GetConfigurationAsync("NITNegocio", "C/F"),
                ReceiptMessage = await _configService.GetConfigurationAsync("ReceiptMessage", "Gracias por su compra"),
                CurrencySymbol = await _configService.GetCurrencySymbolAsync(),

                // WhatsApp
                WhatsAppAlertsEnabled = (await _configService.GetConfigurationAsync("WhatsAppAlertsEnabled", "false")) == "true",
                WhatsAppNumber = await _configService.GetConfigurationAsync("WhatsAppNumber", ""),

                // Inventario
                Products_PageSize = int.Parse(await _configService.GetConfigurationAsync("Products_PageSize", "20")),
                Inventory_BlindCount = (await _configService.GetConfigurationAsync("Inventory_BlindCount", "false")) == "true",
                Permissions_CanManageInventory = (await _configService.GetConfigurationAsync("Permissions_CanManageInventory", "false")) == "true",

                // POS
                Customer_RequiredInPOS = (await _configService.GetConfigurationAsync("Customer_RequiredInPOS", "false")) == "true",
                Customer_DefaultId = await _configService.GetConfigurationAsync("Customer_DefaultId", "1"),
                Customer_AllowQuickCreate = (await _configService.GetConfigurationAsync("Customer_AllowQuickCreate", "true")) == "true",
                POSCart_AutoSave = (await _configService.GetConfigurationAsync("POSCart_AutoSave", "true")) == "true",
                POSCart_ExpirationMinutes = int.Parse(await _configService.GetConfigurationAsync("POSCart_ExpirationMinutes", "30")),
                POS_MaxDiscountPercent = decimal.Parse(await _configService.GetConfigurationAsync("POS_MaxDiscountPercent", "50")),

                // Caja
                CashRegister_RequireOpen = (await _configService.GetConfigurationAsync("CashRegister_RequireOpen", "true")) == "true",
                CashRegister_AllowMultipleSessions = (await _configService.GetConfigurationAsync("CashRegister_AllowMultipleSessions", "false")) == "true",
                CashRegister_DefaultPaymentMethod = await _configService.GetConfigurationAsync("CashRegister_DefaultPaymentMethod", "Cash"),

                // Devoluciones
                Refund_AllowPartialRefunds = (await _configService.GetConfigurationAsync("Refund_AllowPartialRefunds", "true")) == "true",
                Refund_MaxHoursAfterSale = int.Parse(await _configService.GetConfigurationAsync("Refund_MaxHoursAfterSale", "24")),
                Refund_RequireReason = (await _configService.GetConfigurationAsync("Refund_RequireReason", "true")) == "true",
                Refund_RequireAdminRole = (await _configService.GetConfigurationAsync("Refund_RequireAdminRole", "true")) == "true",
                Refund_AllowNegativeCashRegister = (await _configService.GetConfigurationAsync("Refund_AllowNegativeCashRegister", "false")) == "true",
                Refund_RegisterAsExpenseIfCashClosed = (await _configService.GetConfigurationAsync("Refund_RegisterAsExpenseIfCashClosed", "true")) == "true",

                // Permisos Globales Vendedor
                Vendedor_CanEditStock = (await _configService.GetConfigurationAsync("Vendedor_CanEditStock", "false")) == "true",
                Vendedor_SeeCostPrice = (await _configService.GetConfigurationAsync("Vendedor_SeeCostPrice", "false")) == "true",
                Vendedor_SeeInventoryTotal = (await _configService.GetConfigurationAsync("Vendedor_SeeInventoryTotal", "false")) == "true",
                Vendedor_DefaultPage = await _configService.GetConfigurationAsync("Vendedor_DefaultPage", "ventas/pos"),
                Vendedor_SeeDashboardCharts = (await _configService.GetConfigurationAsync("Vendedor_SeeDashboardCharts", "true")) == "true"
            };

            return View(model);
        }

        // POST: /configuracion/sistema
        [HttpPost]
        [Route("sistema")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SystemSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                // General
                await UpdateConfig("NombreNegocio", model.NombreNegocio);
                await UpdateConfig("DireccionNegocio", model.DireccionNegocio);
                await UpdateConfig("TelefonoNegocio", model.TelefonoNegocio);
                await UpdateConfig("NITNegocio", model.NITNegocio);
                await UpdateConfig("ReceiptMessage", model.ReceiptMessage);
                await UpdateConfig("CurrencySymbol", model.CurrencySymbol);

                // WhatsApp
                await UpdateConfig("WhatsAppAlertsEnabled", model.WhatsAppAlertsEnabled.ToString().ToLower());
                await UpdateConfig("WhatsAppNumber", model.WhatsAppNumber);

                // Inventario
                await UpdateConfig("Products_PageSize", model.Products_PageSize.ToString());
                await UpdateConfig("Inventory_BlindCount", model.Inventory_BlindCount.ToString().ToLower());
                await UpdateConfig("Permissions_CanManageInventory", model.Permissions_CanManageInventory.ToString().ToLower());

                // POS
                await UpdateConfig("Customer_RequiredInPOS", model.Customer_RequiredInPOS.ToString().ToLower());
                await UpdateConfig("Customer_DefaultId", model.Customer_DefaultId);
                await UpdateConfig("Customer_AllowQuickCreate", model.Customer_AllowQuickCreate.ToString().ToLower());
                await UpdateConfig("POSCart_AutoSave", model.POSCart_AutoSave.ToString().ToLower());
                await UpdateConfig("POSCart_ExpirationMinutes", model.POSCart_ExpirationMinutes.ToString());
                await UpdateConfig("POS_MaxDiscountPercent", model.POS_MaxDiscountPercent.ToString());

                // Caja
                await UpdateConfig("CashRegister_RequireOpen", model.CashRegister_RequireOpen.ToString().ToLower());
                await UpdateConfig("CashRegister_AllowMultipleSessions", model.CashRegister_AllowMultipleSessions.ToString().ToLower());
                await UpdateConfig("CashRegister_DefaultPaymentMethod", model.CashRegister_DefaultPaymentMethod);

                // Devoluciones
                await UpdateConfig("Refund_AllowPartialRefunds", model.Refund_AllowPartialRefunds.ToString().ToLower());
                await UpdateConfig("Refund_MaxHoursAfterSale", model.Refund_MaxHoursAfterSale.ToString());
                await UpdateConfig("Refund_RequireReason", model.Refund_RequireReason.ToString().ToLower());
                await UpdateConfig("Refund_RequireAdminRole", model.Refund_RequireAdminRole.ToString().ToLower());
                await UpdateConfig("Refund_AllowNegativeCashRegister", model.Refund_AllowNegativeCashRegister.ToString().ToLower());
                await UpdateConfig("Refund_RegisterAsExpenseIfCashClosed", model.Refund_RegisterAsExpenseIfCashClosed.ToString().ToLower());

                // Permisos Globales Vendedor
                await UpdateConfig("Vendedor_CanEditStock", model.Vendedor_CanEditStock.ToString().ToLower());
                await UpdateConfig("Vendedor_SeeCostPrice", model.Vendedor_SeeCostPrice.ToString().ToLower());
                await UpdateConfig("Vendedor_SeeInventoryTotal", model.Vendedor_SeeInventoryTotal.ToString().ToLower());
                await UpdateConfig("Vendedor_DefaultPage", model.Vendedor_DefaultPage);
                await UpdateConfig("Vendedor_SeeDashboardCharts", model.Vendedor_SeeDashboardCharts.ToString().ToLower());

                TempData["Success"] = "Configuración actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        private async Task UpdateConfig(string key, string value)
        {
            var config = await _context.SystemConfigurations.FirstOrDefaultAsync(c => c.Key == key);
            if (config != null)
            {
                config.Value = value;
                _context.Update(config);
            }
            else
            {
                _context.SystemConfigurations.Add(new SystemConfiguration { Key = key, Value = value });
            }
            await _context.SaveChangesAsync();
        }

        // GET: /configuracion/usuarios
        [Route("usuarios")]
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.Where(u => u.IsActive).ToListAsync();
            var model = new List<UserPermissionsViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new UserPermissionsViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName ?? "",
                    FullName = $"{user.FirstName} {user.LastName}",
                    Role = roles.FirstOrDefault() ?? "Sin Rol",
                    CanProcessRefunds = user.CanProcessRefunds,
                    CanViewAuditLog = user.CanViewAuditLog,
                    CanAccessPurchases = user.CanAccessPurchases
                });
            }

            return View(model);
        }

        // GET: /configuracion/usuarios/crear
        [Route("usuarios/crear")]
        public IActionResult CreateUser()
        {
            return View(new UserCreateViewModel());
        }

        // POST: /configuracion/usuarios/crear
        [HttpPost]
        [Route("usuarios/crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(UserCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.EmployeeCode,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmployeeCode = model.EmployeeCode,
                    DPI = "0000000000000" // Default value
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                    TempData["Success"] = "Usuario creado correctamente.";
                    return RedirectToAction(nameof(Users));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // POST: /configuracion/usuarios/eliminar
        [HttpPost]
        [Route("usuarios/eliminar")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction(nameof(Users));
            }

            if (user.UserName == User.Identity.Name)
            {
                TempData["Error"] = "No puedes eliminar tu propio usuario.";
                return RedirectToAction(nameof(Users));
            }

            // Perform Soft Delete to avoid FK issues with audit logs
            user.IsActive = false;
            _context.Update(user);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Usuario desactivado correctamente.";

            return RedirectToAction(nameof(Users));
        }

        // POST: /configuracion/usuarios/permisos
        [HttpPost]
        [Route("usuarios/permisos")]
        public async Task<IActionResult> UpdatePermission(string userId, string permission, bool isGranted)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            switch (permission)
            {
                case "CanProcessRefunds":
                    user.CanProcessRefunds = isGranted;
                    break;
                case "CanViewAuditLog":
                    user.CanViewAuditLog = isGranted;
                    break;
                case "CanAccessPurchases":
                    user.CanAccessPurchases = isGranted;
                    break;
                default:
                    return BadRequest("Permiso desconocido");
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // ==========================================
        // BACKUP & RESTORE
        // ==========================================

        private const string SqlBackupDir = "/var/opt/mssql/backups"; // Ruta dentro del contenedor SQL Server
        private const string LocalBackupDir = "/app/backups";       // Ruta dentro del contenedor webapp
        private const string DbName = "SistemaInventarioDB";

        private string GetMasterConnectionString()
        {
            var cs = _context.Database.GetConnectionString() ?? "";
            return cs.Replace(DbName, "master");
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes >= 1_073_741_824) return $"{bytes / 1_073_741_824.0:F1} GB";
            if (bytes >= 1_048_576) return $"{bytes / 1_048_576.0:F1} MB";
            if (bytes >= 1024) return $"{bytes / 1024.0:F1} KB";
            return $"{bytes} B";
        }

        private bool IsValidBackupFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return false;
            if (fileName.Contains("..") || fileName.Contains('/') || fileName.Contains('\\')) return false;
            if (!fileName.EndsWith(".bak", StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }

        // GET: /configuracion/respaldos
        [Route("respaldos")]
        public IActionResult Backups()
        {
            var backups = new List<BackupFileViewModel>();

            try
            {
                if (Directory.Exists(LocalBackupDir))
                {
                    var files = new DirectoryInfo(LocalBackupDir)
                        .GetFiles("*.bak")
                        .OrderByDescending(f => f.CreationTime);

                    foreach (var file in files)
                    {
                        backups.Add(new BackupFileViewModel
                        {
                            FileName = file.Name,
                            SizeInBytes = file.Length,
                            SizeFormatted = FormatFileSize(file.Length),
                            CreatedAt = file.CreationTime
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al leer backups: {ex.Message}";
            }

            return View(backups);
        }

        // POST: /configuracion/respaldos/crear
        [HttpPost]
        [Route("respaldos/crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBackup()
        {
            try
            {
                Directory.CreateDirectory(LocalBackupDir);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"{DbName}_{timestamp}.bak";
                var backupPath = Path.Combine(SqlBackupDir, fileName);

                using var connection = new SqlConnection(GetMasterConnectionString());
                await connection.OpenAsync();
                var sql = $"BACKUP DATABASE [{DbName}] TO DISK = @path WITH FORMAT, INIT, NAME = @name";
                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@path", backupPath);
                command.Parameters.AddWithValue("@name", $"{DbName}-Backup-{timestamp}");
                command.CommandTimeout = 120;
                await command.ExecuteNonQueryAsync();

                TempData["Success"] = $"Respaldo '{fileName}' creado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al crear respaldo: {ex.Message}";
            }

            return RedirectToAction(nameof(Backups));
        }

        // POST: /configuracion/respaldos/restaurar
        [HttpPost]
        [Route("respaldos/restaurar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreBackup(string fileName)
        {
            if (!IsValidBackupFileName(fileName))
            {
                TempData["Error"] = "Nombre de archivo no válido.";
                return RedirectToAction(nameof(Backups));
            }

            var localPath = Path.Combine(LocalBackupDir, fileName);
            if (!System.IO.File.Exists(localPath))
            {
                TempData["Error"] = "El archivo de respaldo no existe.";
                return RedirectToAction(nameof(Backups));
            }

            var sqlPath = Path.Combine(SqlBackupDir, fileName);

            try
            {
                using var connection = new SqlConnection(GetMasterConnectionString());
                await connection.OpenAsync();

                // Set single user to force disconnect all users
                var singleUserCmd = new SqlCommand(
                    $"IF DB_ID('{DbName}') IS NOT NULL ALTER DATABASE [{DbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE",
                    connection);
                singleUserCmd.CommandTimeout = 30;
                await singleUserCmd.ExecuteNonQueryAsync();

                // Restore the database
                var restoreCmd = new SqlCommand(
                    $"RESTORE DATABASE [{DbName}] FROM DISK = @path WITH REPLACE, RECOVERY",
                    connection);
                restoreCmd.Parameters.AddWithValue("@path", sqlPath);
                restoreCmd.CommandTimeout = 120;
                await restoreCmd.ExecuteNonQueryAsync();

                // Set multi user
                var multiUserCmd = new SqlCommand(
                    $"ALTER DATABASE [{DbName}] SET MULTI_USER",
                    connection);
                multiUserCmd.CommandTimeout = 30;
                await multiUserCmd.ExecuteNonQueryAsync();

                TempData["Success"] = $"Base de datos restaurada desde '{fileName}'. Por favor inicie sesión nuevamente.";
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                // Try to set multi user in case of error
                try
                {
                    using var recovery = new SqlConnection(GetMasterConnectionString());
                    await recovery.OpenAsync();
                    var cmd = new SqlCommand($"ALTER DATABASE [{DbName}] SET MULTI_USER", recovery);
                    await cmd.ExecuteNonQueryAsync();
                }
                catch { }

                TempData["Error"] = $"Error al restaurar: {ex.Message}";
                return RedirectToAction(nameof(Backups));
            }
        }

        // POST: /configuracion/respaldos/eliminar
        [HttpPost]
        [Route("respaldos/eliminar")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteBackup(string fileName)
        {
            if (!IsValidBackupFileName(fileName))
            {
                TempData["Error"] = "Nombre de archivo no válido.";
                return RedirectToAction(nameof(Backups));
            }

            var filePath = Path.Combine(LocalBackupDir, fileName);
            if (!System.IO.File.Exists(filePath))
            {
                TempData["Error"] = "El archivo no existe.";
                return RedirectToAction(nameof(Backups));
            }

            try
            {
                System.IO.File.Delete(filePath);
                TempData["Success"] = $"Respaldo '{fileName}' eliminado.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar: {ex.Message}";
            }

            return RedirectToAction(nameof(Backups));
        }

        // POST: /configuracion/respaldos/resetear
        [HttpPost]
        [Route("respaldos/resetear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetDatabase(string confirmText)
        {
            if (confirmText != "RESETEAR")
            {
                TempData["Error"] = "Texto de confirmación incorrecto.";
                return RedirectToAction(nameof(Backups));
            }

            try
            {
                using var connection = new SqlConnection(GetMasterConnectionString());
                await connection.OpenAsync();

                // Drop the database
                var dropCmd = new SqlCommand(
                    $"ALTER DATABASE [{DbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{DbName}]",
                    connection);
                dropCmd.CommandTimeout = 60;
                await dropCmd.ExecuteNonQueryAsync();

                // Stop the app so Docker restarts it and recreates the DB via migrations + DbInitializer
                _ = Task.Run(async () =>
                {
                    await Task.Delay(2000);
                    _appLifetime.StopApplication();
                });

                return Content(
                    "<html><body style='font-family:Inter,sans-serif;display:flex;align-items:center;justify-content:center;height:100vh;background:#f6f7f8'>" +
                    "<div style='text-align:center'>" +
                    "<h2>Base de datos reseteada</h2>" +
                    "<p>El sistema se esta reiniciando automaticamente...</p>" +
                    "<p style='color:#617589;margin-top:16px'>La pagina se recargara en unos segundos.</p>" +
                    "<script>setTimeout(()=>window.location.href='/',15000)</script>" +
                    "</div></body></html>",
                    "text/html");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al resetear: {ex.Message}";
                return RedirectToAction(nameof(Backups));
            }
        }
    }
}

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using SistemaInventario.Data;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SistemaInventario.Services
{
    public class BackupHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackupHostedService> _logger;
        private DateTime _lastRunDate = DateTime.MinValue;

        private const string SqlBackupDir = "/var/opt/mssql/backups";
        private const string LocalBackupDir = "/app/backups";
        private const string ImagesLocalDir = "/app/wwwroot/images/products";
        private const string DbName = "SistemaInventarioDB";

        public BackupHostedService(IServiceProvider serviceProvider, ILogger<BackupHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BackupHostedService iniciado.");

            // Revisar cada minuto
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await CheckAndRunBackupAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al ejecutar chequeo de auto backup.");
                }
            }
        }

        private async Task CheckAndRunBackupAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var configService = scope.ServiceProvider.GetRequiredService<IConfigurationService>();

            var enabled = (await configService.GetConfigurationAsync("AutoBackup_Enabled", "false")) == "true";
            if (!enabled) return;

            var timeStr = await configService.GetConfigurationAsync("AutoBackup_Time", "02:00");
            if (!TimeSpan.TryParse(timeStr, out var scheduleTime)) return;

            var now = DateTime.Now;
            
            // Verificamos si ya corrió hoy
            if (_lastRunDate.Date == now.Date) return;

            // Verificamos si ya es la hora configurada (o acaba de pasar dentro de este minuto)
            if (now.TimeOfDay < scheduleTime) return;

            var frequency = await configService.GetConfigurationAsync("AutoBackup_Frequency", "Daily");
            bool shouldRun = false;

            if (frequency == "Daily")
            {
                shouldRun = true;
            }
            else if (frequency == "Weekly")
            {
                var dayOfWeekStr = await configService.GetConfigurationAsync("AutoBackup_DayOfWeek", "0");
                if (int.TryParse(dayOfWeekStr, out var dayOfWeek) && (int)now.DayOfWeek == dayOfWeek)
                {
                    shouldRun = true;
                }
            }
            else if (frequency == "Monthly")
            {
                var dayOfMonthStr = await configService.GetConfigurationAsync("AutoBackup_DayOfMonth", "1");
                if (int.TryParse(dayOfMonthStr, out var dayOfMonth) && now.Day == dayOfMonth)
                {
                    shouldRun = true;
                }
            }

            if (shouldRun)
            {
                _logger.LogInformation("Iniciando tarea de Auto-Backup programada.");
                await RunBackupsAsync(scope.ServiceProvider);
                _lastRunDate = now.Date;

                // Limpieza de retención
                var retentionStr = await configService.GetConfigurationAsync("AutoBackup_RetentionDays", "10");
                if (int.TryParse(retentionStr, out var retention))
                {
                    CleanOldBackups(retention);
                }
            }
        }

        private async Task RunBackupsAsync(IServiceProvider scopedProvider)
        {
            var dbContext = scopedProvider.GetRequiredService<ApplicationDbContext>();
            var configService = scopedProvider.GetRequiredService<IConfigurationService>();
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            Directory.CreateDirectory(LocalBackupDir);

            bool dbSuccess = false;
            bool imgSuccess = false;
            string dbFileName = "";
            string imgFileName = "";
            string dbError = "";
            string imgError = "";

            // 1. Respaldo BD
            try
            {
                dbFileName = $"{DbName}_{timestamp}.bak";
                var backupPath = Path.Combine(SqlBackupDir, dbFileName);

                var cs = dbContext.Database.GetConnectionString() ?? "";
                var masterCs = cs.Replace(DbName, "master");

                using var connection = new SqlConnection(masterCs);
                await connection.OpenAsync();
                var sql = $"BACKUP DATABASE [{DbName}] TO DISK = @path WITH FORMAT, INIT, NAME = @name";
                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@path", backupPath);
                command.Parameters.AddWithValue("@name", $"{DbName}-Backup-{timestamp}");
                command.CommandTimeout = 300; // 5 min
                await command.ExecuteNonQueryAsync();

                dbSuccess = true;
                _logger.LogInformation("Auto-Backup: Base de datos respaldada correctamente en {FileName}", dbFileName);
            }
            catch (Exception ex)
            {
                dbError = ex.Message;
                _logger.LogError(ex, "Auto-Backup: Error al respaldar base de datos.");
            }

            // 2. Respaldo Imágenes
            try
            {
                imgFileName = $"Imagenes_Productos_{timestamp}.zip";
                var localPath = Path.Combine(LocalBackupDir, imgFileName);

                if (Directory.Exists(ImagesLocalDir))
                {
                    ZipFile.CreateFromDirectory(ImagesLocalDir, localPath);
                    imgSuccess = true;
                    _logger.LogInformation("Auto-Backup: Imágenes respaldadas correctamente en {FileName}", imgFileName);
                }
            }
            catch (Exception ex)
            {
                imgError = ex.Message;
                _logger.LogError(ex, "Auto-Backup: Error al respaldar imágenes.");
            }

            // 3. Enviar notificación por correo
            await SendBackupNotificationAsync(configService, dbSuccess, imgSuccess, dbFileName, imgFileName, dbError, imgError, timestamp);
        }

        private async Task SendBackupNotificationAsync(IConfigurationService configService,
            bool dbSuccess, bool imgSuccess, string dbFileName, string imgFileName,
            string dbError, string imgError, string timestamp)
        {
            try
            {
                var notifyEnabled = (await configService.GetConfigurationAsync("AutoBackup_NotifyEmail", "false")) == "true";
                if (!notifyEnabled) return;

                var notifyEmail = await configService.GetConfigurationAsync("AutoBackup_NotifyEmailAddress", "");
                if (string.IsNullOrWhiteSpace(notifyEmail)) return;

                var emailService = new EmailService(_serviceProvider, _serviceProvider.GetRequiredService<ILogger<EmailService>>());

                var allOk = dbSuccess && imgSuccess;
                var statusIcon = allOk ? "✅" : "⚠️";
                var statusText = allOk ? "Completado exitosamente" : "Completado con errores";

                var subject = $"{statusIcon} Auto-Backup Sistema de Inventario - {statusText}";

                var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background: {(allOk ? "#10b981" : "#f59e0b")}; color: white; padding: 20px; border-radius: 8px 8px 0 0;'>
                        <h2 style='margin: 0;'>{statusIcon} Respaldo Automático</h2>
                        <p style='margin: 5px 0 0 0; opacity: 0.9;'>{DateTime.Now:dddd, dd MMMM yyyy HH:mm}</p>
                    </div>
                    <div style='background: #f9fafb; padding: 20px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 8px 8px;'>
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr>
                                <td style='padding: 10px; border-bottom: 1px solid #e5e7eb;'>
                                    <strong>Base de Datos:</strong>
                                </td>
                                <td style='padding: 10px; border-bottom: 1px solid #e5e7eb; color: {(dbSuccess ? "green" : "red")};'>
                                    {(dbSuccess ? $"✅ {dbFileName}" : $"❌ Error: {dbError}")}
                                </td>
                            </tr>
                            <tr>
                                <td style='padding: 10px;'>
                                    <strong>Imágenes:</strong>
                                </td>
                                <td style='padding: 10px; color: {(imgSuccess ? "green" : "red")};'>
                                    {(imgSuccess ? $"✅ {imgFileName}" : $"❌ Error: {imgError}")}
                                </td>
                            </tr>
                        </table>
                        <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 15px 0;' />
                        <p style='font-size: 12px; color: #6b7280; margin: 0;'>
                            Este correo fue generado automáticamente por el Sistema de Inventario.
                        </p>
                    </div>
                </div>";

                await emailService.SendEmailAsync(notifyEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auto-Backup: Error al enviar notificación por correo.");
            }
        }

        private void CleanOldBackups(int retentionCount)
        {
            try
            {
                if (!Directory.Exists(LocalBackupDir)) return;

                var allFiles = new DirectoryInfo(LocalBackupDir).GetFiles("*.*")
                    .Where(f => f.Extension.Equals(".bak", StringComparison.OrdinalIgnoreCase) || 
                                f.Extension.Equals(".zip", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(f => f.CreationTime)
                    .ToList();

                var dbFiles = allFiles.Where(f => f.Extension.Equals(".bak", StringComparison.OrdinalIgnoreCase)).ToList();
                var imgFiles = allFiles.Where(f => f.Extension.Equals(".zip", StringComparison.OrdinalIgnoreCase)).ToList();

                // Eliminar BD antiguos
                if (dbFiles.Count > retentionCount)
                {
                    var toDelete = dbFiles.Skip(retentionCount);
                    foreach (var file in toDelete)
                    {
                        file.Delete();
                        _logger.LogInformation("Auto-Backup: Archivo antiguo eliminado {FileName}", file.Name);
                    }
                }

                // Eliminar Imágenes antiguos
                if (imgFiles.Count > retentionCount)
                {
                    var toDelete = imgFiles.Skip(retentionCount);
                    foreach (var file in toDelete)
                    {
                        file.Delete();
                        _logger.LogInformation("Auto-Backup: Archivo antiguo eliminado {FileName}", file.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auto-Backup: Error al limpiar respaldos antiguos.");
            }
        }
    }
}

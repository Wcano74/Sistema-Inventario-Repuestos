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
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            Directory.CreateDirectory(LocalBackupDir);

            // 1. Respaldo BD
            try
            {
                var dbFileName = $"{DbName}_{timestamp}.bak";
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

                _logger.LogInformation("Auto-Backup: Base de datos respaldada correctamente en {FileName}", dbFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auto-Backup: Error al respaldar base de datos.");
            }

            // 2. Respaldo Imágenes
            try
            {
                var imgFileName = $"Imagenes_Productos_{timestamp}.zip";
                var localPath = Path.Combine(LocalBackupDir, imgFileName);

                if (Directory.Exists(ImagesLocalDir))
                {
                    ZipFile.CreateFromDirectory(ImagesLocalDir, localPath);
                    _logger.LogInformation("Auto-Backup: Imágenes respaldadas correctamente en {FileName}", imgFileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auto-Backup: Error al respaldar imágenes.");
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

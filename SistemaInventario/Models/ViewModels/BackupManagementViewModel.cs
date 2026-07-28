using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaInventario.Models.ViewModels
{
    public class BackupManagementViewModel
    {
        public List<BackupFileViewModel> Backups { get; set; } = new List<BackupFileViewModel>();

        // Auto Backup Settings
        public bool AutoBackup_Enabled { get; set; }
        
        [Required(ErrorMessage = "La frecuencia es requerida.")]
        public string AutoBackup_Frequency { get; set; } = "Daily"; // "Daily", "Weekly", "Monthly"
        
        [Required(ErrorMessage = "La hora es requerida.")]
        public string AutoBackup_Time { get; set; } = "02:00";
        
        public int AutoBackup_DayOfWeek { get; set; } = 0; // 0 (Domingo) a 6 (Sábado)
        
        public int AutoBackup_DayOfMonth { get; set; } = 1; // 1-31
        
        [Range(1, 100, ErrorMessage = "La retención debe ser entre 1 y 100 días.")]
        public int AutoBackup_RetentionDays { get; set; } = 10;

        // Email Notification Settings
        public bool AutoBackup_NotifyEmail { get; set; }
        public string AutoBackup_NotifyEmailAddress { get; set; } = string.Empty;

        // SMTP Settings
        public string Smtp_Host { get; set; } = string.Empty;
        public int Smtp_Port { get; set; } = 587;
        public string Smtp_User { get; set; } = string.Empty;
        public string Smtp_Password { get; set; } = string.Empty;
        public string Smtp_FromEmail { get; set; } = string.Empty;
        public bool Smtp_UseSsl { get; set; } = true;
    }
}


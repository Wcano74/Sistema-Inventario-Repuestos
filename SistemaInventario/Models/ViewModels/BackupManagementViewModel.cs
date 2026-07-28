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
    }
}

namespace SistemaInventario.Models.ViewModels
{
    public class BackupFileViewModel
    {
        public string FileName { get; set; } = string.Empty;
        public long SizeInBytes { get; set; }
        public string SizeFormatted { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}

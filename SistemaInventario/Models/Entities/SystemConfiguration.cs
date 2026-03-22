using System.ComponentModel.DataAnnotations;

namespace SistemaInventario.Models.Entities
{
    public class SystemConfiguration
    {
        [Key]
        public string Key { get; set; } = string.Empty;

        [Required]
        public string Value { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}

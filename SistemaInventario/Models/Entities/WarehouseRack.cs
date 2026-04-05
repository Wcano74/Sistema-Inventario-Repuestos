using System.ComponentModel.DataAnnotations;

namespace SistemaInventario.Models.Entities
{
    public class WarehouseRack
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del estante es obligatorio")]
        [Display(Name = "Nombre del Estante")]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        public string? Description { get; set; }

        [Required]
        [Range(1, 20, ErrorMessage = "El número de filas debe estar entre 1 y 20")]
        [Display(Name = "Número de Filas")]
        public int NumberOfRows { get; set; } = 3;

        [Display(Name = "Activo")]
        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "La bodega es obligatoria.")]
        public int WarehouseId { get; set; }

        public Warehouse? Warehouse { get; set; }
        
        // Navigation property for rows/locations
        public ICollection<WarehouseLocation> Locations { get; set; } = new List<WarehouseLocation>();
    }
}

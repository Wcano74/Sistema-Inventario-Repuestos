using System.ComponentModel.DataAnnotations;

namespace SistemaInventario.Models.Entities
{
    public class Warehouse
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la bodega es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres.")]
        [Display(Name = "Nombre de Bodega")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Descripción o Dirección")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation property
        public ICollection<WarehouseRack> Racks { get; set; } = new List<WarehouseRack>();
    }
}

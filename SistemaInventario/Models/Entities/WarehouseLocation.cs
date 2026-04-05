using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaInventario.Models.Entities
{
    public class WarehouseLocation
    {
        public int Id { get; set; }

        [Display(Name = "Estante")]
        public int WarehouseRackId { get; set; }
        public WarehouseRack? WarehouseRack { get; set; }

        [Display(Name = "Fila")]
        public int Row { get; set; }

        [Display(Name = "Descripción")]
        public string? Description { get; set; }

        [NotMapped]
        [Display(Name = "Código de Ubicación")]
        public string LocationCode => WarehouseRack != null
            ? $"{WarehouseRack.Name}-F{Row}"
            : $"?-F{Row}";

        // Navigation property
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}

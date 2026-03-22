using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaInventario.Models.Entities
{
    public class ProductHistory
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        // User who performed the action
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Required]
        [Display(Name = "Acción")]
        public string Action { get; set; } = string.Empty; // "Creación", "Edición", "Venta", "Ajuste", "Estado"

        [Display(Name = "Cambio")]
        public int QuantityChange { get; set; } // +10, -5, 0 (if only price changed)

        [Display(Name = "Stock Final")]
        public int NewStock { get; set; } // Stock after action

        [Display(Name = "Detalle")]
        public string? Description { get; set; } // "Price changed from 10 to 12"

        public DateTime Date { get; set; } = DateTime.Now;
    }
}

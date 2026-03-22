using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaInventario.Models.Entities
{
    public class InventoryCount
    {
        public int Id { get; set; }

        public int InventoryCycleId { get; set; }
        public InventoryCycle InventoryCycle { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [Display(Name = "Conteo Físico")]
        public int PhysicalQuantity { get; set; }

        // Snapshot of system stock AT THE MOMENT OF CLOSING/VERIFYING
        [Display(Name = "Stock Sistema (Cierre)")]
        public int SystemQuantityAtClose { get; set; }

        [Display(Name = "Diferencia")]
        public int Difference { get; set; }
        
        [Display(Name = "Notas")]
        public string? Notes { get; set; }
        
        [Display(Name = "Verificado")]
        public bool IsVerified { get; set; } = false;
    }
}

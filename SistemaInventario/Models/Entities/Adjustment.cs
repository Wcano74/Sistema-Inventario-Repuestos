using System.ComponentModel.DataAnnotations;

namespace SistemaInventario.Models.Entities
{
    public class Adjustment
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Display(Name = "Cantidad")]
        public int Quantity { get; set; } // Positive for add, negative for remove

        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "Motivo")]
        public string? Reason { get; set; }
        
        // Could add User reference later
        public string? UserId { get; set; }
    }
}

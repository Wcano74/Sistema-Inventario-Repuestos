using System.ComponentModel.DataAnnotations;

namespace ControlLibrería.Models.Entities
{
    public class InventoryCycle
    {
        public int Id { get; set; }

        [Display(Name = "Fecha de Apertura")]
        public DateTime OpenedAt { get; set; } = DateTime.Now;

        [Display(Name = "Fecha de Cierre")]
        public DateTime? ClosedAt { get; set; }

        [Display(Name = "Estado")]
        public CycleStatus Status { get; set; } = CycleStatus.Open;

        // "Total" or "Category"
        [Display(Name = "Alcance")]
        public string Scope { get; set; } = "Total"; 

        [Display(Name = "Categoría")]
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        [Display(Name = "Notas")]
        public string? Notes { get; set; }

        // User who opened it
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public ICollection<InventoryCount> Counts { get; set; } = new List<InventoryCount>();
    }

    public enum CycleStatus
    {
        Open = 0,
        Closed = 1,
        Cancelled = 2
    }
}

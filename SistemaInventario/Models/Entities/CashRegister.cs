using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaInventario.Models.Entities
{
    public class CashRegister
    {
        public int Id { get; set; }

        [Display(Name = "Fecha de Apertura")]
        public DateTime OpenedAt { get; set; } = DateTime.Now;

        [Display(Name = "Fecha de Cierre")]
        public DateTime? ClosedAt { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Monto Inicial")]
        public decimal OpeningBalance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Balance Esperado")]
        public decimal ExpectedBalance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Balance Real")]
        public decimal ActualBalance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Diferencia")]
        public decimal Difference { get; set; }

        [Display(Name = "Notas de Apertura")]
        public string? OpeningNotes { get; set; }

        [Display(Name = "Notas de Cierre")]
        public string? ClosingNotes { get; set; }

        [Display(Name = "Estado")]
        public bool IsOpen { get; set; } = true;

        // Foreign Key
        [Required]
        [Display(Name = "Usuario")]
        public string UserId { get; set; } = string.Empty;
        
        public ApplicationUser? User { get; set; }

        // Navigation property
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}

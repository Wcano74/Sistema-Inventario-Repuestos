using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlLibrería.Models.Entities
{
    public class Expense
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [Display(Name = "Descripción")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Monto")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Amount { get; set; }

        [Display(Name = "Fecha")]
        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "Referencia / Recibo")]
        public string? Reference { get; set; }

        // Foreign Keys
        [Display(Name = "Categoría")]
        public int ExpenseCategoryId { get; set; }
        public ExpenseCategory? Category { get; set; }

        [Display(Name = "Usuario")]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
    }
}

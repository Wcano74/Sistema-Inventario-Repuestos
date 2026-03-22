using System.ComponentModel.DataAnnotations;

namespace SistemaInventario.Models.Entities
{
    public class ExpenseCategory
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre de la Categoría")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
}

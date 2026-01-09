using System.ComponentModel.DataAnnotations;

namespace ControlLibrería.Models.Entities
{
    public class Supplier
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Contacto")]
        public string? ContactName { get; set; }

        [Phone]
        [Display(Name = "Teléfono")]
        public string? Phone { get; set; }

        [EmailAddress]
        [Display(Name = "Correo Electrónico")]
        public string? Email { get; set; }

        // Navigation property
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}

using System.ComponentModel.DataAnnotations;

namespace ControlLibrería.Models.Entities
{
    public class Customer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "NIT/DPI")]
        public string? NitDpi { get; set; }

        [Phone]
        [Display(Name = "Teléfono")]
        public string? Phone { get; set; }

        [EmailAddress]
        [Display(Name = "Correo Electrónico")]
        public string? Email { get; set; }

        [Display(Name = "Dirección")]
        public string? Address { get; set; }

        [Display(Name = "Nombre Fiscal")]
        public string? FiscalName { get; set; }

        [Display(Name = "Dirección Fiscal")]
        public string? FiscalAddress { get; set; }

        [Display(Name = "Fecha de Registro")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Última Compra")]
        public DateTime? LastPurchaseDate { get; set; }

        [Display(Name = "Activo")]
        public bool IsActive { get; set; } = true;

        // Navigation property
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}

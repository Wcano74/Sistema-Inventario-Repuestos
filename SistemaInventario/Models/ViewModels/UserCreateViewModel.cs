using System.ComponentModel.DataAnnotations;

namespace SistemaInventario.Models.ViewModels
{
    public class UserCreateViewModel
    {
        [Required(ErrorMessage = "El código de empleado es obligatorio")]
        [Display(Name = "Código de Empleado")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "El código debe tener 5 dígitos")]
        public string EmployeeCode { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [Display(Name = "Apellido")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Display(Name = "Rol")]
        public string Role { get; set; } = "Vendedor";
    }
}

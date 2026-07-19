using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SistemaInventario.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        public string DisplayName => $"{FirstName} {LastName}";

        public int Age { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [Required]
        [MaxLength(20)]
        public string DPI { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? EducationLevel { get; set; }

        public decimal Salary { get; set; }

        [Required]
        [MaxLength(5)]
        public string EmployeeCode { get; set; } = string.Empty;

        public string? ProfilePictureUrl { get; set; }

        // Purchase Order Permissions
        public bool CanAccessPurchases { get; set; }
        public bool CanCreateOrders { get; set; } = false;
        public bool CanManageOrders { get; set; } = false; // Receive, Cancel
        public bool CanViewAuditLog { get; set; } = false; // View History/Bitácora

        // Refund Permissions
        public bool CanProcessRefunds { get; set; } = false; // Process sale refunds

        // Inventory Value Permissions
        public bool CanViewInventoryValue { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }
}

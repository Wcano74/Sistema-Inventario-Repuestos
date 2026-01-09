
namespace ControlLibrería.Models.ViewModels
{
    public class UserPermissionsViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public bool CanProcessRefunds { get; set; }
        public bool CanViewAuditLog { get; set; }
        public bool CanAccessPurchases { get; set; }
    }
}

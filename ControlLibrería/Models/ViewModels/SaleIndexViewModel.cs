using ControlLibrería.Models.Entities;

namespace ControlLibrería.Models.ViewModels
{
    public class SaleIndexViewModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Total { get; set; }
        public string? UserId { get; set; }
        public string SellerName { get; set; } = "Sistema";
        public string CustomerName { get; set; } = "Cliente General";
        public string PaymentMethodName { get; set; } = "Efectivo";
        public SaleStatus Status { get; set; } = SaleStatus.Active;
        public string StatusName { get; set; } = "Activa";
        public bool HasRefunds { get; set; }
        public decimal RefundedAmount { get; set; }
        public ICollection<SaleDetail>? SaleDetails { get; set; }

    }
}

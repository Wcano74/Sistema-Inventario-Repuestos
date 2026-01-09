using ControlLibrería.Models.Entities;

namespace ControlLibrería.Models.ViewModels
{
    public class RefundIndexViewModel
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public DateTime RefundDate { get; set; }
        public string RefundTypeName { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public bool IsRegisteredAsExpense { get; set; }
    }
}

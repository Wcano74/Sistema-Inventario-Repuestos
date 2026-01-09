using ControlLibrería.Models.Entities;

namespace ControlLibrería.Models.ViewModels
{
    public class RefundViewModel
    {
        public int SaleId { get; set; }
        public Sale? Sale { get; set; }
        public bool IsFullRefund { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public List<RefundItemViewModel> RefundItems { get; set; } = new();
    }

    public class RefundItemViewModel
    {
        public int SaleDetailId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public int QuantityAlreadyReturned { get; set; }
        public int QuantityAvailable => QuantitySold - QuantityAlreadyReturned;
        public int Quantity { get; set; } // Quantity to return
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => Quantity * UnitPrice;
    }
}

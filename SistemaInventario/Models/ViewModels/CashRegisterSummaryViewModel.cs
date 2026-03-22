using SistemaInventario.Models.Entities;

namespace SistemaInventario.Models.ViewModels
{
    public class CashRegisterSummaryViewModel
    {
        public CashRegister CashRegister { get; set; } = new CashRegister();
        public decimal TotalCash { get; set; }
        public decimal TotalCard { get; set; }
        public decimal TotalTransfer { get; set; }
        public decimal TotalMixed { get; set; }
        public int TransactionCount { get; set; }
        public List<Sale> Sales { get; set; } = new List<Sale>();
        public string CurrencySymbol { get; set; } = "Q";
    }
}

using SistemaInventario.Models.Entities;

namespace SistemaInventario.Models.ViewModels
{
    public class CustomerDetailsViewModel
    {
        public Customer Customer { get; set; } = new Customer();
        public List<Sale> RecentSales { get; set; } = new List<Sale>();
        public decimal TotalPurchased { get; set; }
        public int TotalTransactions { get; set; }
        public DateTime? LastPurchaseDate { get; set; }
    }
}

using ControlLibrería.Models.Entities;

namespace ControlLibrería.Models.ViewModels
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

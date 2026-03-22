using SistemaInventario.Models.Entities;

namespace SistemaInventario.Models.ViewModels
{
    public class DashboardViewModel
    {
        public DailySalesResult DailySales { get; set; } = new();
        public List<Product> LowStockProducts { get; set; } = new();
        public List<TopProductResult> TopProducts { get; set; } = new();
        public List<WeeklySalesResult> WeeklySales { get; set; } = new();
        public List<Sale> RecentSales { get; set; } = new();
        public List<CategoryDistributionResult> CategoryDistribution { get; set; } = new();
        public int TotalProducts { get; set; }
        public int LowStockCount { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public List<UserSalesReportResult> UserSales { get; set; } = new();
        public List<ProductHistory> RecentActivities { get; set; } = new();

        // Financial Stats
        public decimal TotalExpenses { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal NetProfit { get; set; }
    }

    public class UserSalesReportResult
    {
        public string? UserName { get; set; } = string.Empty;
        public int TransactionCount { get; set; }
        public decimal TotalSales { get; set; }
    }

    public class CategoryDistributionResult
    {
        public string CategoryName { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class WeeklySalesResult
    {
        public string DayName { get; set; } = string.Empty;
        public decimal TotalSales { get; set; }
        public decimal TotalProfit { get; set; }
    }

    public class DailySalesResult
    {
        public decimal TotalSales { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalProfit { get; set; }
        public int TransactionCount { get; set; }
    }

    public class TopProductResult
    {
        public string Name { get; set; } = string.Empty;
        public int TotalSold { get; set; }
        public decimal Revenue { get; set; }
    }
}

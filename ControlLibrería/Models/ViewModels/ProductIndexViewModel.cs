using ControlLibrería.Models.Entities;

namespace ControlLibrería.Models.ViewModels
{
    public class ProductIndexViewModel
    {
        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public int TotalProducts { get; set; }
        public decimal InventoryValue { get; set; }
        public int LowStockCount { get; set; }
        
        // Filter properties
        public string? SearchTerm { get; set; }
        public int? SelectedCategory { get; set; }

        // Pagination
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }
}

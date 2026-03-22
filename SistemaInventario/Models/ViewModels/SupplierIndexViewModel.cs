using SistemaInventario.Models.Entities;

namespace SistemaInventario.Models.ViewModels
{
    public class SupplierIndexViewModel
    {
        public List<Supplier> Suppliers { get; set; } = new();
        public int TotalSuppliers { get; set; }

        // Filter properties
        public string? SearchTerm { get; set; }

        // Pagination
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }
}

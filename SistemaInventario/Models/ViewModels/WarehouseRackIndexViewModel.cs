using SistemaInventario.Models.Entities;

namespace SistemaInventario.Models.ViewModels
{
    public class WarehouseRackIndexViewModel
    {
        public List<WarehouseRack> Racks { get; set; } = new();
        public int TotalRacks { get; set; }

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

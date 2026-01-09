using ControlLibrería.Models.Entities;

namespace ControlLibrería.Models.ViewModels
{
    public class ProductDetailsViewModel
    {
        public Product Product { get; set; } = null!;
        public List<MovementViewModel> RecentMovements { get; set; } = new();
    }

    public class MovementViewModel
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty; // "Venta", "Ajuste", "Compra"
        public int Quantity { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string User { get; set; } = "Sistema";
    }
}

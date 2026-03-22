using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaInventario.Models.Entities
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        public string? Description { get; set; }

        [Display(Name = "Código de Barras")]
        public string? Barcode { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Precio de Venta")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Costo")]
        public decimal Cost { get; set; }

        [Display(Name = "Stock Actual")]
        public int StockQuantity { get; set; }

        [Display(Name = "Stock Mínimo")]
        public int MinStock { get; set; }

        [Display(Name = "Imagen")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Marca / Fabricante")]
        public string? Brand { get; set; }

        [Display(Name = "Activo")]
        public bool IsActive { get; set; } = true;

        // Foreign Keys
        [Display(Name = "Categoría")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Display(Name = "Proveedor")]
        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        public ICollection<Adjustment> Adjustments { get; set; } = new List<Adjustment>();
        public ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();
        public ICollection<ProductHistory> HistoryLogs { get; set; } = new List<ProductHistory>();
    }
}

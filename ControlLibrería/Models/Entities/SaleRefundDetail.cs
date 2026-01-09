using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlLibrería.Models.Entities
{
    public class SaleRefundDetail
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Devolución")]
        public int SaleRefundId { get; set; }
        public SaleRefund? SaleRefund { get; set; }

        [Required]
        [Display(Name = "Detalle de Venta Original")]
        public int SaleDetailId { get; set; }
        public SaleDetail? SaleDetail { get; set; }

        [Required]
        [Display(Name = "Producto")]
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Required]
        [Display(Name = "Cantidad Devuelta")]
        public int QuantityReturned { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Precio Unitario")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Subtotal")]
        public decimal Subtotal { get; set; }
    }
}

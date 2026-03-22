using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaInventario.Models.Entities
{
    public class SaleRefund
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Venta")]
        public int SaleId { get; set; }
        public Sale? Sale { get; set; }

        [Required]
        [Display(Name = "Fecha de Devolución")]
        public DateTime RefundDate { get; set; } = DateTime.Now;

        [Display(Name = "Usuario")]
        public string? UserId { get; set; } // Usuario que procesó la devolución
        
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Required]
        [Display(Name = "Tipo de Devolución")]
        public RefundType RefundType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Monto Devuelto")]
        public decimal RefundAmount { get; set; }

        [Required]
        [Display(Name = "Motivo")]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        [Display(Name = "Notas Adicionales")]
        [StringLength(1000)]
        public string? Notes { get; set; }

        // Relación con caja registradora donde se procesó la devolución
        [Display(Name = "Caja Registradora")]
        public int? CashRegisterId { get; set; }
        public CashRegister? CashRegister { get; set; }

        // Indica si fue registrado como egreso (para cajas cerradas)
        [Display(Name = "Registrado como Egreso")]
        public bool IsRegisteredAsExpense { get; set; } = false;

        // Detalles de productos devueltos
        public ICollection<SaleRefundDetail> RefundDetails { get; set; } = new List<SaleRefundDetail>();
    }
}

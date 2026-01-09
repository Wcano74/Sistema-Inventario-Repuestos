using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlLibrería.Models.Entities
{
    public class Sale
    {
        public int Id { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total")]
        public decimal Total { get; set; }

        public string? UserId { get; set; } // Link to Identity User

        // Customer relationship
        [Display(Name = "Cliente")]
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        // Cash Register relationship
        [Display(Name = "Caja")]
        public int? CashRegisterId { get; set; }
        public CashRegister? CashRegister { get; set; }

        // Payment information
        [Display(Name = "Método de Pago")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Monto Pagado")]
        public decimal AmountPaid { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Cambio")]
        public decimal Change { get; set; }

        [Display(Name = "Detalles de Pago")]
        public string? PaymentDetails { get; set; } // JSON for mixed payments

        // Refund tracking fields
        [Display(Name = "Estado")]
        public SaleStatus Status { get; set; } = SaleStatus.Active;

        [Display(Name = "Tiene Devoluciones")]
        public bool HasRefunds { get; set; } = false;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Monto Devuelto")]
        public decimal RefundedAmount { get; set; } = 0;

        public ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();
        public ICollection<SaleRefund> Refunds { get; set; } = new List<SaleRefund>();

    }
}

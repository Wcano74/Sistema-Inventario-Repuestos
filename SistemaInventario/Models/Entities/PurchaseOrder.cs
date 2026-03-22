using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaInventario.Models.Entities
{
    public enum PurchaseOrderStatus
    {
        [Display(Name = "Pendiente")]
        Pending,
        [Display(Name = "Recibido")]
        Received,
        [Display(Name = "Cancelado")]
        Canceled
    }

    public class PurchaseOrder
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El proveedor es obligatorio")]
        [Display(Name = "Proveedor")]
        public int SupplierId { get; set; }
        
        public Supplier? Supplier { get; set; }

        [Required]
        [Display(Name = "Fecha de Pedido")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Display(Name = "Fecha Esperada")]
        public DateTime? ExpectedDate { get; set; }

        [Display(Name = "Estado")]
        public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Pending;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Notas")]
        public string? Notes { get; set; }

        // User Tracking
        [Display(Name = "Creado Por")]
        public string? CreatedByUserId { get; set; }
        [ForeignKey("CreatedByUserId")]
        public ApplicationUser? CreatedByUser { get; set; }

        [Display(Name = "Recibido Por")]
        public string? ReceivedByUserId { get; set; }
        [ForeignKey("ReceivedByUserId")]
        public ApplicationUser? ReceivedByUser { get; set; }

        [Display(Name = "Cancelado Por")]
        public string? CanceledByUserId { get; set; }
        [ForeignKey("CanceledByUserId")]
        public ApplicationUser? CanceledByUser { get; set; }

        public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    }
}

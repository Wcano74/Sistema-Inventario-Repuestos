
namespace SistemaInventario.Models.ViewModels
{
    public class SystemSettingsViewModel
    {
        // General - Datos del Negocio
        public string NombreNegocio { get; set; }
        public string DireccionNegocio { get; set; }
        public string TelefonoNegocio { get; set; }
        public string NITNegocio { get; set; }
        public string ReceiptMessage { get; set; }
        public string CurrencySymbol { get; set; }

        // WhatsApp / Notificaciones
        public bool WhatsAppAlertsEnabled { get; set; }
        public string WhatsAppNumber { get; set; }

        // Inventario y Productos
        public int Products_PageSize { get; set; }
        public bool Inventory_BlindCount { get; set; }
        public bool Permissions_CanManageInventory { get; set; }

        // Ventas / POS
        public bool Customer_RequiredInPOS { get; set; }
        public string Customer_DefaultId { get; set; }
        public bool Customer_AllowQuickCreate { get; set; }
        public bool POSCart_AutoSave { get; set; }
        public int POSCart_ExpirationMinutes { get; set; }
        public decimal POS_MaxDiscountPercent { get; set; } = 50;

        // Caja
        public bool CashRegister_RequireOpen { get; set; }
        public bool CashRegister_AllowMultipleSessions { get; set; }
        public string CashRegister_DefaultPaymentMethod { get; set; }

        // Devoluciones
        public bool Refund_AllowPartialRefunds { get; set; }
        public int Refund_MaxHoursAfterSale { get; set; }
        public bool Refund_RequireReason { get; set; }
        public bool Refund_RequireAdminRole { get; set; }
        public bool Refund_AllowNegativeCashRegister { get; set; }
        public bool Refund_RegisterAsExpenseIfCashClosed { get; set; }

        // Permisos Vendedor (Globales)
        public bool Vendedor_CanEditStock { get; set; }
        public bool Vendedor_SeeCostPrice { get; set; }
        public bool Vendedor_SeeInventoryTotal { get; set; }
        public string Vendedor_DefaultPage { get; set; }
        public bool Vendedor_SeeDashboardCharts { get; set; }
    }
}

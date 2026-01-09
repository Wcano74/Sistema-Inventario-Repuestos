using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ControlLibrería.Data;
using ControlLibrería.Models.Entities;

namespace ControlLibrería.Controllers
{
    [Route("ventas")]
    public class SalesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ControlLibrería.Services.IConfigurationService _configService;

        public SalesController(ApplicationDbContext context, ControlLibrería.Services.IConfigurationService configService)
        {
            _context = context;
            _configService = configService;
        }

        // GET: Sales (History)
        [Route("")]
        [Route("index")]
        [Route("historial")]
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, string period = "day")
        {
            // Calculate date ranges based on period
            var now = DateTime.Now;
            DateTime filterStartDate;
            DateTime filterEndDate = now;

            switch (period.ToLower())
            {
                case "week":
                    filterStartDate = now.Date.AddDays(-6);
                    break;
                case "month":
                    filterStartDate = new DateTime(now.Year, now.Month, 1);
                    break;
                case "custom":
                    filterStartDate = startDate ?? now.Date;
                    filterEndDate = endDate ?? now;
                    break;
                case "day":
                default:
                    filterStartDate = now.Date;
                    filterEndDate = now.Date.AddDays(1).AddSeconds(-1);
                    break;
            }

            // Override with custom dates if provided
            if (startDate.HasValue && period == "custom")
                filterStartDate = startDate.Value;
            if (endDate.HasValue && period == "custom")
                filterEndDate = endDate.Value;

            var sales = await (
                from sale in _context.Sales
                where sale.Date >= filterStartDate && sale.Date <= filterEndDate
                join user in _context.Users on sale.UserId equals user.EmployeeCode into userGroup
                from u in userGroup.DefaultIfEmpty()
                join customer in _context.Customers on sale.CustomerId equals customer.Id into customerGroup
                from c in customerGroup.DefaultIfEmpty()
                select new Models.ViewModels.SaleIndexViewModel
                {
                    Id = sale.Id,
                    Date = sale.Date,
                    Total = sale.Total,
                    UserId = sale.UserId,
                    SellerName = u != null ? u.FirstName + " " + u.LastName : "Sistema",
                    CustomerName = c != null ? c.Name : "Cliente General",
                    PaymentMethodName = sale.PaymentMethod == PaymentMethod.Cash ? "Efectivo" : 
                                       sale.PaymentMethod == PaymentMethod.Card ? "Tarjeta" : 
                                       sale.PaymentMethod == PaymentMethod.Transfer ? "Transferencia" : "Mixto",
                    Status = sale.Status,
                    StatusName = sale.Status == SaleStatus.Active ? "Activa" :
                                sale.Status == SaleStatus.PartiallyCancelled ? "Parcialmente Devuelta" : "Cancelada",
                    HasRefunds = sale.HasRefunds,
                    RefundedAmount = sale.RefundedAmount,
                    SaleDetails = sale.SaleDetails
                })

                .OrderByDescending(s => s.Date)
                .ToListAsync();

            // Pass filter state to view
            ViewBag.Period = period;
            ViewBag.StartDate = filterStartDate.ToString("yyyy-MM-dd");
            ViewBag.EndDate = filterEndDate.ToString("yyyy-MM-dd");

            return View(sales);
        }

        // GET: Sales/Create (The POS Screen)
        [Route("crear")]
        [Route("pos")]
        public async Task<IActionResult> Create()
        {
            // We pass the list of products to the view for the selector
            ViewBag.Products = _context.Products.Select(p => new {
                p.Id,
                p.Name,
                p.Price,
                p.StockQuantity,
                p.Barcode
            }).ToList();
            
            // Load customers for selector
            ViewBag.Customers = await _context.Customers
                .Where(c => c.IsActive)
                .Select(c => new { 
                    Id = c.Id, 
                    Name = c.Name, 
                    NitDpi = c.NitDpi, 
                    Phone = c.Phone 
                })
                .ToListAsync();
            
            // Get current user's open cash register
            var currentUserId = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name)?.Id;
            var openCashRegister = await _context.CashRegisters
                .Where(cr => cr.IsOpen && cr.UserId == currentUserId)
                .FirstOrDefaultAsync();
            
            ViewBag.HasOpenCashRegister = openCashRegister != null;
            ViewBag.CurrentCashRegisterId = openCashRegister?.Id;
            
            // Customer configurations
            ViewBag.CustomerRequired = (await _configService.GetConfigurationAsync("Customer_RequiredInPOS", "false")) == "true";
            ViewBag.DefaultCustomerId = int.Parse(await _configService.GetConfigurationAsync("Customer_DefaultId", "1"));
            ViewBag.AllowQuickCreate = (await _configService.GetConfigurationAsync("Customer_AllowQuickCreate", "true")) == "true";
            
            // Cash register configurations
            ViewBag.CashRegisterRequired = (await _configService.GetConfigurationAsync("CashRegister_RequireOpen", "true")) == "true";
            ViewBag.DefaultPaymentMethod = await _configService.GetConfigurationAsync("CashRegister_DefaultPaymentMethod", "Cash");
            
            // POS Cart Settings
            ViewBag.POSCartAutoSave = (await _configService.GetConfigurationAsync("POSCart_AutoSave", "true")) == "true";
            ViewBag.POSCartExpirationMinutes = int.Parse(await _configService.GetConfigurationAsync("POSCart_ExpirationMinutes", "30"));
            
            return View();
        }

        // POST: Sales/Create
        [HttpPost]
        [Route("crear")]
        public async Task<IActionResult> Create([FromBody] Sale saleData)
        {
            if (saleData == null || saleData.SaleDetails.Count == 0)
            {
                return BadRequest("No hay datos de venta o el carrito está vacío.");
            }

            // Validate cash register if required
            var requireCashRegister = (await _configService.GetConfigurationAsync("CashRegister_RequireOpen", "true")) == "true";
            if (requireCashRegister && saleData.PaymentMethod == PaymentMethod.Cash)
            {
                var currentUserId = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name)?.Id;
                var openCashRegister = await _context.CashRegisters
                    .Where(cr => cr.IsOpen && cr.UserId == currentUserId)
                    .FirstOrDefaultAsync();
                
                if (openCashRegister == null)
                {
                    return BadRequest("Debe abrir una caja antes de realizar ventas en efectivo.");
                }
                
                saleData.CashRegisterId = openCashRegister.Id;
            }

            // Validate customer if required
            var requireCustomer = (await _configService.GetConfigurationAsync("Customer_RequiredInPOS", "false")) == "true";
            if (requireCustomer && (saleData.CustomerId == null || saleData.CustomerId == 0))
            {
                return BadRequest("Debe seleccionar un cliente para completar la venta.");
            }
            
            // Set default customer if not provided
            if (saleData.CustomerId == null || saleData.CustomerId == 0)
            {
                saleData.CustomerId = int.Parse(await _configService.GetConfigurationAsync("Customer_DefaultId", "1"));
            }

            using var transaction = _context.Database.BeginTransaction();

            try
            {
                // 1. Prepare Sale Header
                var sale = new Sale
                {
                    Date = DateTime.Now,
                    UserId = User.Identity?.Name,
                    CustomerId = saleData.CustomerId,
                    CashRegisterId = saleData.CashRegisterId,
                    PaymentMethod = saleData.PaymentMethod,
                    AmountPaid = saleData.AmountPaid,
                    Change = saleData.Change,
                    PaymentDetails = saleData.PaymentDetails,
                    Total = 0
                };

                foreach (var item in saleData.SaleDetails)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null) throw new Exception($"Producto ID {item.ProductId} no encontrado");

                    // Validate Active Status
                    if (!product.IsActive)
                    {
                         return BadRequest($"El producto '{product.Name}' está inactivo y no se puede vender.");
                    }

                    // 2. Validate Stock
                    if (product.StockQuantity < item.Quantity)
                    {
                        return BadRequest($"Stock insuficiente para {product.Name}. Disponible: {product.StockQuantity}");
                    }

                    // 3. Update Inventory
                    product.StockQuantity -= item.Quantity;

                    // Log History
                    var userId = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name)?.Id;
                    var history = new ProductHistory
                    {
                        ProductId = product.Id,
                        UserId = userId,
                        Action = "Venta",
                        QuantityChange = -item.Quantity,
                        NewStock = product.StockQuantity,
                        Description = $"Venta POS",
                        Date = DateTime.Now
                    };
                    _context.Add(history);

                    // 4. Create Sale Detail
                    var detail = new SaleDetail
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        Subtotal = product.Price * item.Quantity
                    };
                    
                    sale.SaleDetails.Add(detail);
                    sale.Total += detail.Subtotal;
                }

                // Update customer's last purchase date
                if (sale.CustomerId.HasValue)
                {
                    var customer = await _context.Customers.FindAsync(sale.CustomerId.Value);
                    if (customer != null)
                    {
                        customer.LastPurchaseDate = DateTime.Now;
                    }
                }

                _context.Sales.Add(sale);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { id = sale.Id, message = "Venta registrada con éxito" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        // GET: Sales/Details/5
        [Route("detalles/{id}")]
        [Route("recibo/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var sale = await _context.Sales
                .Include(s => s.SaleDetails)
                .ThenInclude(sd => sd.Product)
                .Include(s => s.Customer)
                .Include(s => s.CashRegister)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (sale == null) return NotFound();

            return View(sale);
        }
    }
}

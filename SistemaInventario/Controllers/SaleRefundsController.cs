using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaInventario.Data;
using SistemaInventario.Models.Entities;
using SistemaInventario.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace SistemaInventario.Controllers
{
    [Route("devoluciones")]
    [Authorize]
    public class SaleRefundsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Services.IConfigurationService _configService;

        public SaleRefundsController(ApplicationDbContext context, Services.IConfigurationService configService)
        {
            _context = context;
            _configService = configService;
        }

        // GET: SaleRefunds
        [Route("")]
        [Route("index")]
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            // Check permission
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (!User.IsInRole("Admin") && (currentUser == null || !currentUser.CanProcessRefunds))
            {
                TempData["Error"] = "No tienes permisos para acceder a las devoluciones.";
                return RedirectToAction("Index", "Home");
            }

            var now = DateTime.Now;
            var filterStartDate = startDate ?? now.Date.AddDays(-30);
            var filterEndDate = endDate ?? now;

            var refunds = await (
                from refund in _context.SaleRefunds
                where refund.RefundDate >= filterStartDate && refund.RefundDate <= filterEndDate
                join sale in _context.Sales on refund.SaleId equals sale.Id
                join user in _context.Users on refund.UserId equals user.Id into userGroup
                from u in userGroup.DefaultIfEmpty()
                join customer in _context.Customers on sale.CustomerId equals customer.Id into customerGroup
                from c in customerGroup.DefaultIfEmpty()
                select new RefundIndexViewModel
                {
                    Id = refund.Id,
                    SaleId = refund.SaleId,
                    RefundDate = refund.RefundDate,
                    RefundTypeName = refund.RefundType == RefundType.Total ? "Total" : "Parcial",
                    RefundAmount = refund.RefundAmount,
                    Reason = refund.Reason,
                    UserName = u != null ? u.FirstName + " " + u.LastName : "Sistema",
                    CustomerName = c != null ? c.Name : "Cliente General",
                    IsRegisteredAsExpense = refund.IsRegisteredAsExpense
                })
                .OrderByDescending(r => r.RefundDate)
                .ToListAsync();

            ViewBag.StartDate = filterStartDate.ToString("yyyy-MM-dd");
            ViewBag.EndDate = filterEndDate.ToString("yyyy-MM-dd");

            return View(refunds);
        }

        // GET: SaleRefunds/Create/5
        [Route("crear/{saleId}")]
        public async Task<IActionResult> Create(int saleId)
        {
            // Check permission
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (!User.IsInRole("Admin") && (currentUser == null || !currentUser.CanProcessRefunds))
            {
                TempData["Error"] = "No tienes permisos para procesar devoluciones.";
                return RedirectToAction("Index", "Sales");
            }

            var sale = await _context.Sales
                .Include(s => s.SaleDetails)
                .ThenInclude(sd => sd.Product)
                .Include(s => s.Customer)
                .FirstOrDefaultAsync(s => s.Id == saleId);

            if (sale == null)
            {
                TempData["Error"] = "Venta no encontrada.";
                return RedirectToAction("Index", "Sales");
            }

            // Validate sale status
            if (sale.Status == SaleStatus.Cancelled)
            {
                TempData["Error"] = "Esta venta ya está completamente cancelada.";
                return RedirectToAction("Details", "Sales", new { id = saleId });
            }

            // Validate time limit
            var maxHours = int.Parse(await _configService.GetConfigurationAsync("Refund_MaxHoursAfterSale", "24"));
            var hoursSinceSale = (DateTime.Now - sale.Date).TotalHours;
            if (hoursSinceSale > maxHours)
            {
                TempData["Error"] = $"El tiempo límite para devoluciones es de {maxHours} horas. Esta venta fue realizada hace {hoursSinceSale:F1} horas.";
                return RedirectToAction("Details", "Sales", new { id = saleId });
            }

            // Prepare view model
            var model = new RefundViewModel
            {
                SaleId = sale.Id,
                Sale = sale,
                RefundItems = sale.SaleDetails.Select(sd => new RefundItemViewModel
                {
                    SaleDetailId = sd.Id,
                    ProductId = sd.ProductId,
                    ProductName = sd.Product?.Name ?? "Producto",
                    QuantitySold = sd.Quantity,
                    QuantityAlreadyReturned = sd.QuantityReturned,
                    UnitPrice = sd.UnitPrice,
                    Quantity = 0 // User will select
                }).ToList()
            };

            // Get configuration
            ViewBag.RequireReason = (await _configService.GetConfigurationAsync("Refund_RequireReason", "true")) == "true";
            ViewBag.AllowPartialRefunds = (await _configService.GetConfigurationAsync("Refund_AllowPartialRefunds", "true")) == "true";

            return View(model);
        }

        // POST: SaleRefunds/Create
        [HttpPost]
        [Route("crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RefundViewModel model)
        {
            // Check permission
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (!User.IsInRole("Admin") && (currentUser == null || !currentUser.CanProcessRefunds))
            {
                TempData["Error"] = "No tienes permisos para procesar devoluciones.";
                return RedirectToAction("Index", "Sales");
            }

            // Validate reason is required
            var requireReason = (await _configService.GetConfigurationAsync("Refund_RequireReason", "true")) == "true";
            if (requireReason && string.IsNullOrWhiteSpace(model.Reason))
            {
                TempData["Error"] = "El motivo de la devolución es obligatorio.";
                return RedirectToAction("Create", new { saleId = model.SaleId });
            }

            // Validate at least one item selected
            if (!model.RefundItems.Any(i => i.Quantity > 0))
            {
                TempData["Error"] = "Debe seleccionar al menos un producto para devolver.";
                return RedirectToAction("Create", new { saleId = model.SaleId });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Load sale with details
                var sale = await _context.Sales
                    .Include(s => s.SaleDetails)
                    .ThenInclude(sd => sd.Product)
                    .FirstOrDefaultAsync(s => s.Id == model.SaleId);

                if (sale == null)
                {
                    throw new InvalidOperationException("Venta no encontrada.");
                }

                // 2. Validate sale status
                if (sale.Status == SaleStatus.Cancelled)
                {
                    throw new InvalidOperationException("Esta venta ya está completamente cancelada.");
                }

                // 3. Validate time limit
                var maxHours = int.Parse(await _configService.GetConfigurationAsync("Refund_MaxHoursAfterSale", "24"));
                var hoursSinceSale = (DateTime.Now - sale.Date).TotalHours;
                if (hoursSinceSale > maxHours)
                {
                    throw new InvalidOperationException($"El tiempo límite para devoluciones es de {maxHours} horas.");
                }

                // 4. Validate and prepare cash register
                CashRegister? cashRegister = null;
                bool registerAsExpense = false;

                if (sale.PaymentMethod == PaymentMethod.Cash)
                {
                    var currentUserId = currentUser.Id;
                    cashRegister = await _context.CashRegisters
                        .Where(cr => cr.IsOpen && cr.UserId == currentUserId)
                        .FirstOrDefaultAsync();

                    // If original cash register is closed, register as expense
                    if (cashRegister == null)
                    {
                        var allowExpenseRegistration = (await _configService.GetConfigurationAsync("Refund_RegisterAsExpenseIfCashClosed", "true")) == "true";
                        if (!allowExpenseRegistration)
                        {
                            throw new InvalidOperationException("Debe abrir una caja registradora para procesar devoluciones en efectivo.");
                        }
                        
                        // Try to find any open cash register
                        cashRegister = await _context.CashRegisters
                            .Where(cr => cr.IsOpen)
                            .FirstOrDefaultAsync();

                        if (cashRegister == null)
                        {
                            throw new InvalidOperationException("No hay ninguna caja abierta. Debe abrir una caja para procesar la devolución.");
                        }

                        registerAsExpense = true;
                    }
                }

                // 5. Create refund record
                var refund = new SaleRefund
                {
                    SaleId = sale.Id,
                    RefundDate = DateTime.Now,
                    UserId = currentUser.Id,
                    RefundType = model.IsFullRefund ? RefundType.Total : RefundType.Partial,
                    Reason = model.Reason,
                    Notes = model.Notes,
                    CashRegisterId = cashRegister?.Id,
                    IsRegisteredAsExpense = registerAsExpense
                };

                decimal totalRefund = 0;

                // 6. Process each product to return
                foreach (var item in model.RefundItems.Where(i => i.Quantity > 0))
                {
                    var saleDetail = sale.SaleDetails.FirstOrDefault(sd => sd.Id == item.SaleDetailId);
                    if (saleDetail == null)
                    {
                        throw new InvalidOperationException($"Detalle de venta {item.SaleDetailId} no encontrado.");
                    }

                    // Validate quantity
                    var availableToReturn = saleDetail.Quantity - saleDetail.QuantityReturned;
                    if (item.Quantity > availableToReturn)
                    {
                        throw new InvalidOperationException($"La cantidad a devolver de '{item.ProductName}' excede la cantidad disponible ({availableToReturn}).");
                    }

                    // Create refund detail
                    var refundDetail = new SaleRefundDetail
                    {
                        SaleDetailId = saleDetail.Id,
                        ProductId = saleDetail.ProductId,
                        QuantityReturned = item.Quantity,
                        UnitPrice = saleDetail.UnitPrice,
                        Subtotal = saleDetail.UnitPrice * item.Quantity
                    };

                    refund.RefundDetails.Add(refundDetail);
                    totalRefund += refundDetail.Subtotal;

                    // 7. Restore inventory
                    var product = saleDetail.Product;
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;

                        // 8. Log in product history
                        var history = new ProductHistory
                        {
                            ProductId = product.Id,
                            UserId = currentUser.Id,
                            Action = "Devolución",
                            QuantityChange = item.Quantity,
                            NewStock = product.StockQuantity,
                            Description = $"Devolución de venta #{sale.Id} - {refund.Reason}",
                            Date = DateTime.Now
                        };
                        _context.ProductHistories.Add(history);
                    }

                    // 9. Update sale detail
                    saleDetail.QuantityReturned += item.Quantity;
                }

                refund.RefundAmount = totalRefund;

                // 10. Update sale status
                sale.RefundedAmount += totalRefund;
                sale.HasRefunds = true;

                if (sale.RefundedAmount >= sale.Total)
                {
                    sale.Status = SaleStatus.Cancelled;
                }
                else
                {
                    sale.Status = SaleStatus.PartiallyCancelled;
                }

                // 11. Adjust cash register if applicable
                if (cashRegister != null)
                {
                    if (registerAsExpense)
                    {
                        // Register as expense in current cash register
                        var expense = new Expense
                        {
                            ExpenseCategoryId = 1, // Should be a "Devoluciones" category
                            Description = $"Devolución de venta #{sale.Id} - {refund.Reason}",
                            Amount = totalRefund,
                            Date = DateTime.Now,
                            UserId = currentUser.Id
                        };
                        _context.Expenses.Add(expense);
                    }
                    
                    // Reduce expected balance
                    cashRegister.ExpectedBalance -= totalRefund;
                }

                _context.SaleRefunds.Add(refund);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = $"Devolución procesada correctamente. Monto devuelto: Q {totalRefund:N2}";
                return RedirectToAction("Details", new { id = refund.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"Error al procesar la devolución: {ex.Message}";
                return RedirectToAction("Create", new { saleId = model.SaleId });
            }
        }

        // GET: SaleRefunds/Details/5
        [Route("detalles/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            // Check permission
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (!User.IsInRole("Admin") && (currentUser == null || !currentUser.CanProcessRefunds))
            {
                TempData["Error"] = "No tienes permisos para ver los detalles de devoluciones.";
                return RedirectToAction("Index", "Home");
            }

            var refund = await _context.SaleRefunds
                .Include(r => r.Sale)
                    .ThenInclude(s => s.Customer)
                .Include(r => r.RefundDetails)
                    .ThenInclude(rd => rd.Product)
                .Include(r => r.User)
                .Include(r => r.CashRegister)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (refund == null)
            {
                TempData["Error"] = "Devolución no encontrada.";
                return RedirectToAction("Index");
            }

            return View(refund);
        }
    }
}

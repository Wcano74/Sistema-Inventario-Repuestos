using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ControlLibrería.Data;
using ControlLibrería.Models.Entities;
using ControlLibrería.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace ControlLibrería.Controllers
{
    [Route("caja")]
    [Authorize]
    public class CashRegisterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ControlLibrería.Services.IConfigurationService _configService;

        public CashRegisterController(ApplicationDbContext context, ControlLibrería.Services.IConfigurationService configService)
        {
            _context = context;
            _configService = configService;
        }

        // GET: CashRegister
        [Route("")]
        [Route("index")]
        public async Task<IActionResult> Index()
        {
            var cashRegisters = await _context.CashRegisters
                .Include(cr => cr.User)
                .OrderByDescending(cr => cr.OpenedAt)
                .Take(50)
                .ToListAsync();

            // Get current open cash register for this user
            var currentUserId = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name)?.Id;
            ViewBag.HasOpenCashRegister = await _context.CashRegisters
                .AnyAsync(cr => cr.IsOpen && cr.UserId == currentUserId);

            return View(cashRegisters);
        }

        // GET: CashRegister/Open
        [Route("abrir")]
        public async Task<IActionResult> Open()
        {
            var currentUserId = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name)?.Id;
            
            // Check if user already has an open cash register
            var hasOpenCashRegister = await _context.CashRegisters
                .AnyAsync(cr => cr.IsOpen && cr.UserId == currentUserId);

            if (hasOpenCashRegister)
            {
                TempData["Error"] = "Ya tienes una caja abierta. Debes cerrarla antes de abrir una nueva.";
                return RedirectToAction(nameof(Index));
            }

            // Check if multiple sessions are allowed
            var allowMultiple = await _configService.GetConfigurationAsync("CashRegister_AllowMultipleSessions", "false");
            if (allowMultiple == "false")
            {
                var anyOpenCashRegister = await _context.CashRegisters.AnyAsync(cr => cr.IsOpen);
                if (anyOpenCashRegister)
                {
                    TempData["Error"] = "Ya existe una caja abierta. Solo se permite una caja abierta a la vez.";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View();
        }

        // POST: CashRegister/Open
        [HttpPost]
        [Route("abrir")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Open([Bind("OpeningBalance,OpeningNotes")] CashRegister cashRegister)
        {
            var currentUserId = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name)?.Id;
            
            if (currentUserId == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            // Validate no open cash register for this user
            var hasOpenCashRegister = await _context.CashRegisters
                .AnyAsync(cr => cr.IsOpen && cr.UserId == currentUserId);

            if (hasOpenCashRegister)
            {
                TempData["Error"] = "Ya tienes una caja abierta.";
                return RedirectToAction(nameof(Index));
            }

            // Check if multiple sessions are allowed
            var allowMultiple = await _configService.GetConfigurationAsync("CashRegister_AllowMultipleSessions", "false");
            if (allowMultiple == "false")
            {
                var anyOpenCashRegister = await _context.CashRegisters.AnyAsync(cr => cr.IsOpen);
                if (anyOpenCashRegister)
                {
                    TempData["Error"] = "Ya existe una caja abierta. Solo se permite una caja abierta a la vez.";
                    return RedirectToAction(nameof(Index));
                }
            }

            cashRegister.UserId = currentUserId;
            cashRegister.OpenedAt = DateTime.Now;
            cashRegister.IsOpen = true;
            cashRegister.ExpectedBalance = cashRegister.OpeningBalance;
            cashRegister.ActualBalance = 0;
            cashRegister.Difference = 0;

            _context.Add(cashRegister);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Caja abierta correctamente con Q {cashRegister.OpeningBalance:N2}";
            return RedirectToAction(nameof(Index));
        }

        // GET: CashRegister/Close/5
        [Route("cerrar/{id}")]
        public async Task<IActionResult> Close(int? id)
        {
            if (id == null) return NotFound();

            var cashRegister = await _context.CashRegisters
                .Include(cr => cr.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cashRegister == null) return NotFound();

            if (!cashRegister.IsOpen)
            {
                TempData["Error"] = "Esta caja ya está cerrada.";
                return RedirectToAction(nameof(Index));
            }

            // Get summary of sales
            var summary = await GetCashRegisterSummary(id.Value);
            return View(summary);
        }

        // POST: CashRegister/Close/5
        [HttpPost]
        [Route("cerrar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseConfirmed(int id, decimal actualBalance, string? closingNotes)
        {
            var cashRegister = await _context.CashRegisters.FindAsync(id);
            if (cashRegister == null) return NotFound();

            if (!cashRegister.IsOpen)
            {
                TempData["Error"] = "Esta caja ya está cerrada.";
                return RedirectToAction(nameof(Index));
            }

            // Calculate expected balance from sales minus refunds
            var cashSales = await _context.Sales
                .Where(s => s.CashRegisterId == id && 
                            s.PaymentMethod == PaymentMethod.Cash &&
                            s.Status != SaleStatus.Cancelled)
                .SumAsync(s => s.Total);

            var cashRefunds = await _context.SaleRefunds
                .Where(r => r.CashRegisterId == id && !r.IsRegisteredAsExpense)
                .SumAsync(r => r.RefundAmount);

            cashRegister.ExpectedBalance = cashRegister.OpeningBalance + cashSales - cashRefunds;

            cashRegister.ActualBalance = actualBalance;
            cashRegister.Difference = actualBalance - cashRegister.ExpectedBalance;
            cashRegister.ClosingNotes = closingNotes;
            cashRegister.ClosedAt = DateTime.Now;
            cashRegister.IsOpen = false;

            _context.Update(cashRegister);
            await _context.SaveChangesAsync();

            if (cashRegister.Difference != 0)
            {
                TempData["Warning"] = $"Caja cerrada con diferencia de Q {cashRegister.Difference:N2}";
            }
            else
            {
                TempData["Success"] = "Caja cerrada correctamente. Arqueo cuadrado.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: CashRegister/Movements/5
        [Route("movimientos/{id}")]
        public async Task<IActionResult> Movements(int? id)
        {
            if (id == null) return NotFound();

            var summary = await GetCashRegisterSummary(id.Value);
            if (summary.CashRegister == null) return NotFound();

            return View(summary);
        }

        // API: Get current open cash register
        [HttpGet]
        [Route("api/actual")]
        public async Task<IActionResult> Current()
        {
            var currentUserId = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name)?.Id;
            
            var cashRegister = await _context.CashRegisters
                .Where(cr => cr.IsOpen && cr.UserId == currentUserId)
                .OrderByDescending(cr => cr.OpenedAt)
                .FirstOrDefaultAsync();

            if (cashRegister == null)
            {
                return Json(new { hasOpenCashRegister = false });
            }

            return Json(new
            {
                hasOpenCashRegister = true,
                id = cashRegister.Id,
                openedAt = cashRegister.OpenedAt,
                openingBalance = cashRegister.OpeningBalance
            });
        }

        // Helper method to get cash register summary
        private async Task<CashRegisterSummaryViewModel> GetCashRegisterSummary(int cashRegisterId)
        {
            var cashRegister = await _context.CashRegisters
                .Include(cr => cr.User)
                .FirstOrDefaultAsync(cr => cr.Id == cashRegisterId);

            if (cashRegister == null)
            {
                return new CashRegisterSummaryViewModel();
            }

            var sales = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleDetails)
                .ThenInclude(sd => sd.Product)
                .Where(s => s.CashRegisterId == cashRegisterId)
                .OrderBy(s => s.Date)
                .ToListAsync();

            var summary = new CashRegisterSummaryViewModel
            {
                CashRegister = cashRegister,
                Sales = sales,
                TransactionCount = sales.Count,
                TotalCash = sales.Where(s => s.PaymentMethod == PaymentMethod.Cash && s.Status != SaleStatus.Cancelled).Sum(s => s.Total),
                TotalCard = sales.Where(s => s.PaymentMethod == PaymentMethod.Card && s.Status != SaleStatus.Cancelled).Sum(s => s.Total),
                TotalTransfer = sales.Where(s => s.PaymentMethod == PaymentMethod.Transfer && s.Status != SaleStatus.Cancelled).Sum(s => s.Total),
                TotalMixed = sales.Where(s => s.PaymentMethod == PaymentMethod.Mixed && s.Status != SaleStatus.Cancelled).Sum(s => s.Total)
            };

            return summary;
        }
    }
}

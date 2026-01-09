using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ControlLibrería.Data;
using ControlLibrería.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace ControlLibrería.Controllers
{
    [Authorize]
    [Route("egresos")]
    public class ExpensesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ControlLibrería.Services.IConfigurationService _configService;

        public ExpensesController(ApplicationDbContext context, ControlLibrería.Services.IConfigurationService configService)
        {
            _context = context;
            _configService = configService;
        }

        private async Task<bool> IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        // GET: Expenses
        [Route("")]
        [Route("index")]
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, int? categoryId)
        {
            if (!await IsAdmin()) return Forbid();

            var query = _context.Expenses
                .Include(e => e.Category)
                .Include(e => e.User)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(e => e.Date >= startDate.Value);
            
            if (endDate.HasValue)
                query = query.Where(e => e.Date <= endDate.Value.AddDays(1).AddTicks(-1));

            if (categoryId.HasValue)
                query = query.Where(e => e.ExpenseCategoryId == categoryId.Value);

            var expenses = await query.OrderByDescending(e => e.Date).ToListAsync();
            
            ViewBag.Categories = new SelectList(await _context.ExpenseCategories.ToListAsync(), "Id", "Name", categoryId);
            ViewBag.TotalAmount = expenses.Sum(e => e.Amount);
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(expenses);
        }

        // GET: Expenses/Create
        [Route("crear")]
        public async Task<IActionResult> Create()
        {
            if (!await IsAdmin()) return Forbid();
            ViewBag.ExpenseCategoryId = new SelectList(await _context.ExpenseCategories.ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: Expenses/Create
        [HttpPost]
        [Route("crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Description,Amount,Date,Reference,ExpenseCategoryId")] Expense expense)
        {
            if (!await IsAdmin()) return Forbid();
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (ModelState.IsValid)
            {
                expense.UserId = userId;
                _context.Add(expense);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ExpenseCategoryId = new SelectList(await _context.ExpenseCategories.ToListAsync(), "Id", "Name", expense.ExpenseCategoryId);
            return View(expense);
        }

        // GET: Expenses/Edit/5
        [Route("editar/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!await IsAdmin()) return Forbid();
            if (id == null) return NotFound();

            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return NotFound();

            ViewBag.ExpenseCategoryId = new SelectList(await _context.ExpenseCategories.ToListAsync(), "Id", "Name", expense.ExpenseCategoryId);
            return View(expense);
        }

        // POST: Expenses/Edit/5
        [HttpPost]
        [Route("editar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,Amount,Date,Reference,ExpenseCategoryId,UserId")] Expense expense)
        {
            if (!await IsAdmin()) return Forbid();
            if (id != expense.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(expense);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpenseExists(expense.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ExpenseCategoryId = new SelectList(await _context.ExpenseCategories.ToListAsync(), "Id", "Name", expense.ExpenseCategoryId);
            return View(expense);
        }

        // GET: Expenses/Delete/5
        [Route("eliminar/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!await IsAdmin()) return Forbid();
            if (id == null) return NotFound();

            var expense = await _context.Expenses
                .Include(e => e.Category)
                .Include(e => e.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (expense == null) return NotFound();

            return View(expense);
        }

        // POST: Expenses/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("eliminar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!await IsAdmin()) return Forbid();
            var expense = await _context.Expenses.FindAsync(id);
            if (expense != null)
            {
                _context.Expenses.Remove(expense);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ExpenseExists(int id)
        {
            return _context.Expenses.Any(e => e.Id == id);
        }
    }
}

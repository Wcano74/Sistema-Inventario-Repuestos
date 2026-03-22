using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaInventario.Data;
using SistemaInventario.Models.Entities;
using Microsoft.AspNetCore.Authorization;

namespace SistemaInventario.Controllers
{
    [Authorize]
    [Route("egresos/categorias")]
    public class ExpenseCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SistemaInventario.Services.IConfigurationService _configService;

        public ExpenseCategoriesController(ApplicationDbContext context, SistemaInventario.Services.IConfigurationService configService)
        {
            _context = context;
            _configService = configService;
        }

        private async Task<bool> IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        // GET: ExpenseCategories
        [Route("")]
        [Route("index")]
        public async Task<IActionResult> Index()
        {
            if (!await IsAdmin()) return Forbid();
            return View(await _context.ExpenseCategories.ToListAsync());
        }

        // GET: ExpenseCategories/Create
        [Route("crear")]
        public async Task<IActionResult> Create()
        {
            if (!await IsAdmin()) return Forbid();
            return View();
        }

        // POST: ExpenseCategories/Create
        [HttpPost]
        [Route("crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] ExpenseCategory expenseCategory)
        {
            if (!await IsAdmin()) return Forbid();
            if (ModelState.IsValid)
            {
                _context.Add(expenseCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(expenseCategory);
        }

        // GET: ExpenseCategories/Edit/5
        [Route("editar/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!await IsAdmin()) return Forbid();
            if (id == null) return NotFound();

            var expenseCategory = await _context.ExpenseCategories.FindAsync(id);
            if (expenseCategory == null) return NotFound();
            return View(expenseCategory);
        }

        // POST: ExpenseCategories/Edit/5
        [HttpPost]
        [Route("editar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] ExpenseCategory expenseCategory)
        {
            if (!await IsAdmin()) return Forbid();
            if (id != expenseCategory.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(expenseCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpenseCategoryExists(expenseCategory.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(expenseCategory);
        }

        // GET: ExpenseCategories/Delete/5
        [Route("eliminar/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!await IsAdmin()) return Forbid();
            if (id == null) return NotFound();

            var expenseCategory = await _context.ExpenseCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (expenseCategory == null) return NotFound();

            return View(expenseCategory);
        }

        // POST: ExpenseCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("eliminar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!await IsAdmin()) return Forbid();
            var expenseCategory = await _context.ExpenseCategories.FindAsync(id);
            if (expenseCategory != null)
            {
                _context.ExpenseCategories.Remove(expenseCategory);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ExpenseCategoryExists(int id)
        {
            return _context.ExpenseCategories.Any(e => e.Id == id);
        }
    }
}

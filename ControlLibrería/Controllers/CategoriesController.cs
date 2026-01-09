using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ControlLibrería.Data;
using ControlLibrería.Models.Entities;

namespace ControlLibrería.Controllers
{
    [Route("categorias")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ControlLibrería.Services.IConfigurationService _configService;

        public CategoriesController(ApplicationDbContext context, ControlLibrería.Services.IConfigurationService configService)
        {
            _context = context;
            _configService = configService;
        }

        private async Task<bool> CanPerformVendedorAction()
        {
            if (User.IsInRole("Admin")) return true;
            if (User.IsInRole("Vendedor"))
            {
                var allowed = await _configService.GetConfigurationAsync("Vendedor_CanEditStock", "false");
                return allowed == "true";
            }
            return false;
        }

        // GET: Categories
        [Route("")]
        [Route("index")]
        public async Task<IActionResult> Index()
        {
            ViewBag.CanEditStock = await CanPerformVendedorAction();
            return View(await _context.Categories.Include(c => c.Products).ToListAsync());
        }

        // GET: Categories/Details/5
        [Route("detalle/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (category == null) return NotFound();

            return View(category);
        }

        // GET: Categories/Create
        [Route("crear")]
        public async Task<IActionResult> Create()
        {
            if (!await CanPerformVendedorAction()) return Forbid();
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [Route("crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] Category category)
        {
            if (!await CanPerformVendedorAction()) return Forbid();
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        [Route("editar/{id?}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!await CanPerformVendedorAction()) return Forbid();
            if (id == null) return NotFound();

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [Route("editar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Category category)
        {
            if (!await CanPerformVendedorAction()) return Forbid();
            if (id != category.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Delete/5
        [Route("eliminar/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!await CanPerformVendedorAction()) return Forbid();
            if (id == null) return NotFound();

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("eliminar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!await CanPerformVendedorAction()) return Forbid();

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            // Check for associated products
            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
            {
                TempData["Error"] = "No se puede eliminar la categoría porque tiene productos asociados.";
                return RedirectToAction(nameof(Index));
            }

            // Check for associated inventory cycles
            var hasInventoryCycles = await _context.InventoryCycles.AnyAsync(ic => ic.CategoryId == id);
            if (hasInventoryCycles)
            {
                TempData["Error"] = "No se puede eliminar la categoría porque está referenciada en ciclos de inventario.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Categoría eliminada correctamente.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Ocurrió un error al intentar eliminar la categoría.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}

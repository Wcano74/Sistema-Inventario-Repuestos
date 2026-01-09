using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ControlLibrería.Data;
using ControlLibrería.Models.Entities;

namespace ControlLibrería.Controllers
{
    [Route("proveedores")]
    public class SuppliersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ControlLibrería.Services.IConfigurationService _configService;

        public SuppliersController(ApplicationDbContext context, ControlLibrería.Services.IConfigurationService configService)
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

        // GET: Suppliers
        [Route("")]
        [Route("index")]
        public async Task<IActionResult> Index()
        {
            ViewBag.CanEditStock = await CanPerformVendedorAction();
            return View(await _context.Suppliers.ToListAsync());
        }

        // GET: Suppliers/Details/5
        [Route("detalle/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (supplier == null) return NotFound();

            return View(supplier);
        }

        // GET: Suppliers/Create
        [Route("crear")]
        public async Task<IActionResult> Create()
        {
            if (!await CanPerformVendedorAction()) return Forbid();
            return View();
        }

        // POST: Suppliers/Create
        [HttpPost]
        [Route("crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ContactName,Phone,Email")] Supplier supplier)
        {
            if (!await CanPerformVendedorAction()) return Forbid();
            if (ModelState.IsValid)
            {
                _context.Add(supplier);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
        }

        // GET: Suppliers/Edit/5
        [Route("editar/{id?}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!await CanPerformVendedorAction()) return Forbid();
            if (id == null) return NotFound();

            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return NotFound();
            return View(supplier);
        }

        // POST: Suppliers/Edit/5
        [HttpPost]
        [Route("editar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ContactName,Phone,Email")] Supplier supplier)
        {
            if (!await CanPerformVendedorAction()) return Forbid();
            if (id != supplier.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(supplier);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SupplierExists(supplier.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
        }

        // GET: Suppliers/Delete/5
        [Route("eliminar/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!await CanPerformVendedorAction()) return Forbid();
            if (id == null) return NotFound();

            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (supplier == null) return NotFound();

            return View(supplier);
        }

        // POST: Suppliers/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("eliminar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!await CanPerformVendedorAction()) return Forbid();
            
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return NotFound();

            // Check for linked products
            var hasProducts = await _context.Products.AnyAsync(p => p.SupplierId == id);
            if (hasProducts)
            {
                TempData["Error"] = "No se puede eliminar el proveedor porque tiene productos asociados.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Suppliers.Remove(supplier);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Proveedor eliminado correctamente.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Error al intentar eliminar el proveedor.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool SupplierExists(int id)
        {
            return _context.Suppliers.Any(e => e.Id == id);
        }
    }
}

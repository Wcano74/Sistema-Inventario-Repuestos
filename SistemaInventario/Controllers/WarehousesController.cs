using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaInventario.Data;
using SistemaInventario.Models.Entities;

namespace SistemaInventario.Controllers
{
    [Authorize]
    [Route("bodegas")]
    public class WarehousesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WarehousesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("")]
        public async Task<IActionResult> Index(string? searchTerm)
        {
            ViewBag.SearchTerm = searchTerm;
            var query = _context.Warehouses
                .Include(w => w.Racks)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                // In EF Core, Contains on strings automatically translates nicely inside WHERE.
                query = query.Where(w => w.Name.Contains(searchTerm) ||
                                         (w.Description != null && w.Description.Contains(searchTerm)) ||
                                         w.Racks.Any(r => r.Name.Contains(searchTerm) || 
                                                          r.Locations.Any(l => l.Products.Any(p => 
                                                              p.Name.Contains(searchTerm) || 
                                                              p.Barcode.Contains(searchTerm) || 
                                                              (p.Description != null && p.Description.Contains(searchTerm))
                                                          ))));
            }

            var warehouses = await query.OrderBy(w => w.Name).ToListAsync();
            return View(warehouses);
        }

        [Route("crear")]
        public IActionResult Create()
        {
            return View(new Warehouse());
        }

        [HttpPost]
        [Route("crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,IsActive")] Warehouse warehouse)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Warehouses.AnyAsync(w => w.Name == warehouse.Name))
                {
                    ModelState.AddModelError("Name", "Ya existe una bodega con este nombre.");
                    return View(warehouse);
                }

                _context.Add(warehouse);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(warehouse);
        }

        [Route("editar/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var warehouse = await _context.Warehouses.FindAsync(id);
            if (warehouse == null) return NotFound();

            return View(warehouse);
        }

        [HttpPost]
        [Route("editar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,IsActive")] Warehouse warehouse)
        {
            if (id != warehouse.Id) return NotFound();

            if (ModelState.IsValid)
            {
                if (await _context.Warehouses.AnyAsync(w => w.Name == warehouse.Name && w.Id != warehouse.Id))
                {
                    ModelState.AddModelError("Name", "Ya existe otra bodega con este nombre.");
                    return View(warehouse);
                }

                try
                {
                    _context.Update(warehouse);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WarehouseExists(warehouse.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(warehouse);
        }

        [Route("eliminar/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var warehouse = await _context.Warehouses
                .Include(w => w.Racks)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (warehouse == null) return NotFound();

            return View(warehouse);
        }

        [HttpPost, ActionName("Delete")]
        [Route("eliminar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var warehouse = await _context.Warehouses
                .Include(w => w.Racks)
                    .ThenInclude(r => r.Locations)
                        .ThenInclude(l => l.Products)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (warehouse != null)
            {
                // Prevent deleting if it has products assigned
                bool hasProducts = warehouse.Racks.Any(r => r.Locations.Any(l => l.Products.Any()));
                if (hasProducts)
                {
                    ModelState.AddModelError("", "No se puede eliminar la bodega porque contiene productos almacenados.");
                    return View(warehouse);
                }

                _context.Warehouses.Remove(warehouse);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Route("toggle-status/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var warehouse = await _context.Warehouses.FindAsync(id);
            if (warehouse != null)
            {
                warehouse.IsActive = !warehouse.IsActive;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool WarehouseExists(int id)
        {
            return _context.Warehouses.Any(e => e.Id == id);
        }
    }
}

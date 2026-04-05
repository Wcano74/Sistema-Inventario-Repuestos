using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaInventario.Data;
using SistemaInventario.Models.Entities;

namespace SistemaInventario.Controllers
{
    [Route("ubicaciones")]
    public class WarehouseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SistemaInventario.Services.IConfigurationService _configService;

        public WarehouseController(ApplicationDbContext context, SistemaInventario.Services.IConfigurationService configService)
        {
            _context = context;
            _configService = configService;
        }

        private async Task<bool> CanPerformVendedorAction()
        {
            if (User.IsInRole("Admin")) return true;
            if (User.IsInRole("Bodeguero")) return true;
            if (User.IsInRole("Vendedor"))
            {
                var allowed = await _configService.GetConfigurationAsync("Vendedor_CanEditStock", "false");
                return allowed == "true";
            }
            return false;
        }

        // GET: Warehouse
        [Route("")]
        [Route("index")]
        public async Task<IActionResult> Index(string? searchTerm, int page = 1)
        {
            ViewBag.CanEditStock = await CanPerformVendedorAction();

            var query = _context.WarehouseRacks
                .Include(r => r.Warehouse)
                .Include(r => r.Locations)
                    .ThenInclude(l => l.Products)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(r => r.Name.Contains(searchTerm) || 
                                         (r.Description != null && r.Description.Contains(searchTerm)) ||
                                         (r.Warehouse != null && r.Warehouse.Name.Contains(searchTerm)) ||
                                         r.Locations.Any(l => l.Products.Any(p => 
                                             p.Name.Contains(searchTerm) || 
                                             p.Barcode.Contains(searchTerm) || 
                                             (p.Description != null && p.Description.Contains(searchTerm))
                                         )));
            }

            int pageSize = 10;
            var totalItems = await query.CountAsync();
            var racks = await query
                .OrderBy(r => r.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new SistemaInventario.Models.ViewModels.WarehouseRackIndexViewModel
            {
                Racks = racks,
                TotalRacks = await _context.WarehouseRacks.CountAsync(),
                SearchTerm = searchTerm,
                PageIndex = page,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                PageSize = pageSize
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_RackTablePartial", viewModel);
            }

            return View(viewModel);
        }

        // GET: Warehouse/Create
        [Route("crear")]
        public async Task<IActionResult> Create(int? warehouseId = null)
        {
            if (!await CanPerformVendedorAction()) return Forbid();
            ViewData["WarehouseId"] = new SelectList(await _context.Warehouses.Where(w => w.IsActive).OrderBy(w => w.Name).ToListAsync(), "Id", "Name", warehouseId);
            return View(new WarehouseRack { WarehouseId = warehouseId ?? 0 });
        }

        // POST: Warehouse/Create
        [HttpPost]
        [Route("crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,NumberOfRows,WarehouseId")] WarehouseRack rack)
        {
            if (!await CanPerformVendedorAction()) return Forbid();

            // Validar nombre duplicado
            var exists = await _context.WarehouseRacks.AnyAsync(r => r.Name == rack.Name);
            if (exists)
            {
                ModelState.AddModelError("Name", "Ya existe un estante con ese nombre.");
                ViewData["WarehouseId"] = new SelectList(await _context.Warehouses.Where(w => w.IsActive).OrderBy(w => w.Name).ToListAsync(), "Id", "Name", rack.WarehouseId);
                return View(rack);
            }

            if (ModelState.IsValid)
            {
                _context.Add(rack);
                await _context.SaveChangesAsync();

                // Auto-generar ubicaciones (filas)
                for (int i = 1; i <= rack.NumberOfRows; i++)
                {
                    _context.WarehouseLocations.Add(new WarehouseLocation
                    {
                        WarehouseRackId = rack.Id,
                        Row = i
                    });
                }
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Estante \"{rack.Name}\" creado con {rack.NumberOfRows} filas.";
                return RedirectToAction("Index", "Warehouses");
            }
            ViewData["WarehouseId"] = new SelectList(await _context.Warehouses.Where(w => w.IsActive).OrderBy(w => w.Name).ToListAsync(), "Id", "Name", rack.WarehouseId);
            return View(rack);
        }

        // GET: Warehouse/Edit/5
        [Route("editar/{id?}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!await CanPerformVendedorAction()) return Forbid();
            if (id == null) return NotFound();

            var rack = await _context.WarehouseRacks
                .Include(r => r.Locations)
                    .ThenInclude(l => l.Products)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rack == null) return NotFound();
            ViewData["WarehouseId"] = new SelectList(await _context.Warehouses.Where(w => w.IsActive).OrderBy(w => w.Name).ToListAsync(), "Id", "Name", rack.WarehouseId);
            return View(rack);
        }

        // POST: Warehouse/Edit/5
        [HttpPost]
        [Route("editar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,NumberOfRows,IsActive,WarehouseId")] WarehouseRack rack)
        {
            if (!await CanPerformVendedorAction()) return Forbid();
            if (id != rack.Id) return NotFound();

            // Validar nombre duplicado
            var nameExists = await _context.WarehouseRacks.AnyAsync(r => r.Name == rack.Name && r.Id != rack.Id);
            if (nameExists)
            {
                ModelState.AddModelError("Name", "Ya existe un estante con ese nombre.");
                var rackReload = await _context.WarehouseRacks
                    .Include(r => r.Locations).ThenInclude(l => l.Products)
                    .FirstOrDefaultAsync(r => r.Id == id);
                ViewData["WarehouseId"] = new SelectList(await _context.Warehouses.Where(w => w.IsActive).OrderBy(w => w.Name).ToListAsync(), "Id", "Name", rack.WarehouseId);
                return View(rackReload);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingRack = await _context.WarehouseRacks
                        .Include(r => r.Locations)
                            .ThenInclude(l => l.Products)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(r => r.Id == id);

                    if (existingRack == null) return NotFound();

                    int oldRows = existingRack.Locations.Count;
                    int newRows = rack.NumberOfRows;

                    if (newRows < oldRows)
                    {
                        // Verificar si las filas a eliminar tienen productos
                        var rowsWithProducts = existingRack.Locations
                            .Where(l => l.Row > newRows && l.Products.Any())
                            .Select(l => $"{existingRack.Name}-F{l.Row}")
                            .ToList();

                        if (rowsWithProducts.Any())
                        {
                            TempData["Error"] = $"No se puede reducir el número de filas porque las ubicaciones {string.Join(", ", rowsWithProducts)} tienen productos asignados. Reasigne los productos primero.";
                            return RedirectToAction(nameof(Edit), new { id });
                        }

                        // Eliminar filas sobrantes
                        var locationsToRemove = await _context.WarehouseLocations
                            .Where(l => l.WarehouseRackId == id && l.Row > newRows)
                            .ToListAsync();
                        _context.WarehouseLocations.RemoveRange(locationsToRemove);
                    }
                    else if (newRows > oldRows)
                    {
                        // Agregar filas nuevas
                        for (int i = oldRows + 1; i <= newRows; i++)
                        {
                            _context.WarehouseLocations.Add(new WarehouseLocation
                            {
                                WarehouseRackId = id,
                                Row = i
                            });
                        }
                    }

                    _context.Update(rack);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Estante actualizado correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RackExists(rack.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction("Index", "Warehouses");
            }

            var rackWithLocations = await _context.WarehouseRacks
                .Include(r => r.Locations).ThenInclude(l => l.Products)
                .FirstOrDefaultAsync(r => r.Id == id);
            return View(rackWithLocations);
        }

        // GET: Warehouse/Delete/5
        [Route("eliminar/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!await CanPerformVendedorAction()) return Forbid();
            if (id == null) return NotFound();

            var rack = await _context.WarehouseRacks
                .Include(r => r.Locations)
                    .ThenInclude(l => l.Products)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rack == null) return NotFound();

            return View(rack);
        }

        // POST: Warehouse/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("eliminar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!await CanPerformVendedorAction()) return Forbid();

            var rack = await _context.WarehouseRacks
                .Include(r => r.Locations)
                    .ThenInclude(l => l.Products)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rack == null) return NotFound();

            // Verificar que ninguna ubicación tenga productos
            var hasProducts = rack.Locations.Any(l => l.Products.Any());
            if (hasProducts)
            {
                TempData["Error"] = "No se puede eliminar el estante porque tiene productos asignados. Reasígnelos primero.";
                return RedirectToAction("Index", "Warehouses");
            }

            try
            {
                _context.WarehouseRacks.Remove(rack);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Estante eliminado correctamente.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Ocurrió un error al intentar eliminar el estante.";
            }

            return RedirectToAction("Index", "Warehouses");
        }

        private bool RackExists(int id)
        {
            return _context.WarehouseRacks.Any(e => e.Id == id);
        }
    }
}

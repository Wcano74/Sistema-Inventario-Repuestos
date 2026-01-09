using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ControlLibrería.Data;
using ControlLibrería.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ControlLibrería.Controllers
{
    [Authorize]
    public class InventoryCyclesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ControlLibrería.Services.IConfigurationService _configService;

        public InventoryCyclesController(ApplicationDbContext context, ControlLibrería.Services.IConfigurationService configService)
        {
            _context = context;
            _configService = configService;
        }

        private async Task<bool> CanManageInventory()
        {
            if (User.IsInRole("Admin")) return true;
            // Configurable permission for non-admins (Vendedores)
            var allowed = await _configService.GetConfigurationAsync("Permissions_CanManageInventory", "false");
            return allowed == "true";
        }

        // GET: InventoryCycles
        public async Task<IActionResult> Index()
        {
            if (!await CanManageInventory()) return Forbid();

            var cycles = await _context.InventoryCycles
                .Include(i => i.User)
                .OrderByDescending(i => i.OpenedAt)
                .ToListAsync();
            return View(cycles);
        }

        // GET: InventoryCycles/Create
        public async Task<IActionResult> Create()
        {
            if (!await CanManageInventory()) return Forbid();

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: InventoryCycles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Scope,CategoryId,Notes")] InventoryCycle inventoryCycle)
        {
            if (!await CanManageInventory()) return Forbid();

            var currentUserId = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name)?.Id;
            if (currentUserId == null) return NotFound();

            if (ModelState.IsValid)
            {
                inventoryCycle.UserId = currentUserId;
                inventoryCycle.OpenedAt = DateTime.Now;
                inventoryCycle.Status = CycleStatus.Open;

                _context.Add(inventoryCycle);
                await _context.SaveChangesAsync();
                
                var productsQuery = _context.Products.Where(p => p.IsActive);
                if (inventoryCycle.Scope == "Category" && inventoryCycle.CategoryId.HasValue)
                {
                    productsQuery = productsQuery.Where(p => p.CategoryId == inventoryCycle.CategoryId);
                }

                var products = await productsQuery.ToListAsync();
                var counts = new List<InventoryCount>();

                foreach (var product in products)
                {
                    counts.Add(new InventoryCount
                    {
                        InventoryCycleId = inventoryCycle.Id,
                        ProductId = product.Id,
                        PhysicalQuantity = 0,
                        SystemQuantityAtClose = 0,
                        IsVerified = false
                    });
                }

                _context.InventoryCounts.AddRange(counts);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Counting), new { id = inventoryCycle.Id });
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", inventoryCycle.CategoryId);
            return View(inventoryCycle);
        }

        // GET: InventoryCycles/Counting/5
        public async Task<IActionResult> Counting(int? id)
        {
            if (!await CanManageInventory()) return Forbid();
            if (id == null) return NotFound();

            var cycle = await _context.InventoryCycles
                .Include(i => i.Counts)
                .ThenInclude(c => c.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cycle == null) return NotFound();
            if (cycle.Status == CycleStatus.Closed) return RedirectToAction(nameof(Details), new { id = id });

            var blindCount = await _configService.GetConfigurationAsync("Inventory_BlindCount", "false") == "true";
            ViewBag.BlindCount = blindCount;

            return View(cycle);
        }

        // POST: InventoryCycles/UpdateCount
        [HttpPost]
        public async Task<IActionResult> UpdateCount(int countId, int physicalQuantity)
        {
            if (!await CanManageInventory()) return Forbid();

            var count = await _context.InventoryCounts.FindAsync(countId);
            if (count == null) return NotFound();
            
            var cycle = await _context.InventoryCycles.FindAsync(count.InventoryCycleId);
            if (cycle == null || cycle.Status != CycleStatus.Open) return BadRequest("Ciclo cerrado");

            count.PhysicalQuantity = physicalQuantity;
            count.IsVerified = true;
            _context.Update(count);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // GET: InventoryCycles/Close/5
        public async Task<IActionResult> Close(int? id)
        {
            if (!await CanManageInventory()) return Forbid();
            if (id == null) return NotFound();

            var cycle = await _context.InventoryCycles
                .Include(i => i.Counts)
                .ThenInclude(c => c.Product)
                .Include(i => i.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cycle == null) return NotFound();

            foreach(var count in cycle.Counts)
            {
                count.SystemQuantityAtClose = count.Product.StockQuantity;
                count.Difference = count.PhysicalQuantity - count.SystemQuantityAtClose;
            }

            return View(cycle);
        }

        // POST: InventoryCycles/CloseConfirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseConfirmed(int id, string? notes)
        {
            if (!await CanManageInventory()) return Forbid();

            using var transaction = _context.Database.BeginTransaction();
            try 
            {
                var cycle = await _context.InventoryCycles
                    .Include(i => i.Counts)
                    .ThenInclude(c => c.Product)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (cycle == null || cycle.Status != CycleStatus.Open) return NotFound();

                var currentUserId = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name)?.Id;
                
                cycle.Notes = notes;
                cycle.ClosedAt = DateTime.Now;
                cycle.Status = CycleStatus.Closed;

                foreach (var count in cycle.Counts)
                {
                    var currentSystemStock = count.Product.StockQuantity;
                    count.SystemQuantityAtClose = currentSystemStock;
                    count.Difference = count.PhysicalQuantity - currentSystemStock;

                    if (count.Difference != 0)
                    {
                        count.Product.StockQuantity = count.PhysicalQuantity;
                        _context.Update(count.Product);

                        var history = new ProductHistory
                        {
                            ProductId = count.ProductId,
                            UserId = currentUserId,
                            Action = "Ajuste Inventario",
                            QuantityChange = count.Difference,
                            NewStock = count.PhysicalQuantity,
                            Description = $"Ajuste por Cierre de Inventario #{cycle.Id}",
                            Date = DateTime.Now
                        };
                        _context.Add(history);
                    }
                }

                _context.Update(cycle);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            catch(Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // GET: InventoryCycles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
             if (!await CanManageInventory()) return Forbid();
             if (id == null) return NotFound();

            var cycle = await _context.InventoryCycles
                .Include(i => i.Counts)
                .ThenInclude(c => c.Product)
                .Include(i => i.Category)
                .Include(i => i.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cycle == null) return NotFound();

            return View(cycle);
        }

        // GET: InventoryCycles/PrintSheet/5
        public async Task<IActionResult> PrintSheet(int? id)
        {
            if (!await CanManageInventory()) return Forbid();
            if (id == null) return NotFound();

            var cycle = await _context.InventoryCycles
                .Include(i => i.Counts)
                .ThenInclude(c => c.Product)
                .ThenInclude(p => p.Category)
                .Include(i => i.Category)
                .Include(i => i.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cycle == null) return NotFound();

            return View(cycle);
        }
    }
}

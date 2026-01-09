using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ControlLibrería.Data;
using ControlLibrería.Models.Entities;

namespace ControlLibrería.Controllers
{
    [Route("productos")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ControlLibrería.Services.IConfigurationService _configService;

        public ProductsController(ApplicationDbContext context, ControlLibrería.Services.IConfigurationService configService)
        {
            _context = context;
            _configService = configService;
        }

        private async Task<bool> CanPerformVendedorAction(string permissionKey)
        {
            if (User.IsInRole("Admin")) return true;
            if (User.IsInRole("Vendedor"))
            {
                var allowed = await _configService.GetConfigurationAsync(permissionKey, "false");
                return allowed == "true";
            }
            return false;
        }

        [HttpGet("buscar")]
        public async Task<IActionResult> Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Content("");

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Name.Contains(term) || p.Barcode.Contains(term))
                .OrderBy(p => p.Name)
                .Take(5)
                .ToListAsync();

            return PartialView("_SearchResultsPartial", products);
        }

        // GET: Products
        [Route("")]
        [Route("index")]
        public async Task<IActionResult> Index(string? searchTerm, int? categoryId, int page = 1)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || p.Barcode.Contains(searchTerm));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            // Pagination Config
            int pageSize = 20; // Default
            if (int.TryParse(await _configService.GetConfigurationAsync("Products_PageSize", "20"), out int configSize))
            {
                pageSize = configSize > 0 ? configSize : 20;
            }

            // Metrics (Calculated on filtered set or full set?) Usually stats are for full inventory unless specified
            // Keeping metrics for FULL inventory as per original design, or filtered? 
            // Original code calculated metrics on the fly. Let's filter metrics based on current view or keep global?
            // "filtered" metrics are better for users.
            
            var totalItems = await query.CountAsync();
            var products = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Calculate stats for the VIEW MODEL (Global or filtered? Let's do Global for the top cards as standard)
            var allProductsQuery = _context.Products.AsQueryable();
            
            var viewModel = new ControlLibrería.Models.ViewModels.ProductIndexViewModel
            {
                Products = products,
                Categories = await _context.Categories.ToListAsync(),
                TotalProducts = await allProductsQuery.CountAsync(), // Total global
                InventoryValue = await allProductsQuery.SumAsync(p => p.Price * p.StockQuantity),
                LowStockCount = await allProductsQuery.CountAsync(p => p.StockQuantity <= p.MinStock),
                SearchTerm = searchTerm,
                SelectedCategory = categoryId,
                
                // Pagination
                PageIndex = page,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                PageSize = pageSize
            };

            ViewBag.CanEditStock = await CanPerformVendedorAction("Vendedor_CanEditStock");
            ViewBag.SeeCostPrice = await CanPerformVendedorAction("Vendedor_SeeCostPrice");
            ViewBag.SeeInventoryTotal = await CanPerformVendedorAction("Vendedor_SeeInventoryTotal");
            // Load WhatsApp Config
            ViewBag.WhatsAppNumber = await _configService.GetConfigurationAsync("WhatsAppNumber", "");
            var alertsEnabled = await _configService.GetConfigurationAsync("WhatsAppAlertsEnabled", "false");
            ViewBag.WhatsAppAlertsEnabled = string.Equals(alertsEnabled, "true", StringComparison.OrdinalIgnoreCase);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ProductTablePartial", viewModel);
            }

            return View(viewModel);
        }

        // GET: Products/Details/5
        [Route("detalle/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.HistoryLogs).ThenInclude(h => h.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (product == null) return NotFound();

            var viewModel = new ControlLibrería.Models.ViewModels.ProductDetailsViewModel
            {
                Product = product
            };

            // Map HistoryLogs to MovementViewModel
            viewModel.RecentMovements = product.HistoryLogs
                .OrderByDescending(h => h.Date)
                .Take(20)
                .Select(h => new ControlLibrería.Models.ViewModels.MovementViewModel
                {
                    Date = h.Date,
                    Type = h.Action,
                    Quantity = h.QuantityChange,
                    Reference = h.Description ?? "-",
                    User = h.User != null ? (h.User.FirstName ?? h.User.LastName) : "Sistema"
                })
                .ToList();
            
            ViewBag.SeeCostPrice = await CanPerformVendedorAction("Vendedor_SeeCostPrice");

            return View(viewModel);
        }

        // GET: Products/Create
        [Route("crear")]
        public async Task<IActionResult> Create()
        {
            if (!await CanPerformVendedorAction("Vendedor_CanEditStock")) return Forbid();
            
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [Route("crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Barcode,Price,Cost,StockQuantity,MinStock,ImageUrl,CategoryId,SupplierId")] Product product)
        {
            if (!await CanPerformVendedorAction("Vendedor_CanEditStock")) return Forbid();
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();

                // Log Creation History
                var userId = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name)?.Id;
                var history = new ProductHistory
                {
                    ProductId = product.Id,
                    UserId = userId,
                    Action = "Creación",
                    QuantityChange = product.StockQuantity,
                    NewStock = product.StockQuantity,
                    Description = "Producto creado inicialmente",
                    Date = DateTime.Now
                };
                _context.Add(history);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", product.SupplierId);
            return View(product);
        }

        // GET: Products/Edit/5
        [Route("editar/{id?}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!await CanPerformVendedorAction("Vendedor_CanEditStock")) return Forbid();
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", product.SupplierId);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [Route("editar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Barcode,Price,Cost,StockQuantity,MinStock,ImageUrl,CategoryId,SupplierId,Brand,IsActive")] Product product)
        {
            if (!await CanPerformVendedorAction("Vendedor_CanEditStock")) return Forbid();
            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var originalProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                    if (originalProduct == null) return NotFound();

                    _context.Update(product);
                    await _context.SaveChangesAsync();

                    // Detect Changes and Log
                    var changes = new List<string>();
                    if (originalProduct.Price != product.Price) changes.Add($"Precio: {originalProduct.Price:C} -> {product.Price:C}");
                    if (originalProduct.Cost != product.Cost) changes.Add($"Costo: {originalProduct.Cost:C} -> {product.Cost:C}");
                    if (originalProduct.StockQuantity != product.StockQuantity) changes.Add($"Stock Manual: {originalProduct.StockQuantity} -> {product.StockQuantity}");
                    if (originalProduct.Name != product.Name) changes.Add("Nombre modificado");
                    if (originalProduct.Brand != product.Brand) changes.Add($"Marca: {originalProduct.Brand ?? "N/A"} -> {product.Brand ?? "N/A"}");
                    if (originalProduct.IsActive != product.IsActive) changes.Add(product.IsActive ? "Reactivado" : "Desactivado");

                    if (changes.Any())
                    {
                        var userId = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name)?.Id;
                        var history = new ProductHistory
                        {
                            ProductId = product.Id,
                            UserId = userId,
                            Action = "Edición",
                            QuantityChange = product.StockQuantity - originalProduct.StockQuantity,
                            NewStock = product.StockQuantity,
                            Description = string.Join(", ", changes),
                            Date = DateTime.Now
                        };
                        _context.Add(history);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", product.SupplierId);
            return View(product);
        }

        // GET: Products/Delete/5
        [Route("eliminar/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!await CanPerformVendedorAction("Vendedor_CanEditStock")) return Forbid();
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("eliminar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!await CanPerformVendedorAction("Vendedor_CanEditStock")) return Forbid();
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // SOFT DELETE
                product.IsActive = false;
                _context.Update(product);

                // Log Deactivation
                var userId = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name)?.Id;
                var history = new ProductHistory
                {
                    ProductId = product.Id,
                    UserId = userId,
                    Action = "Estado",
                    QuantityChange = 0,
                    NewStock = product.StockQuantity,
                    Description = "Producto eliminado (desactivado)",
                    Date = DateTime.Now
                };
                _context.Add(history);

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Route("toggle-status/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            if (!await CanPerformVendedorAction("Vendedor_CanEditStock")) return Forbid();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.IsActive = !product.IsActive;
            
            // Log Status Change
            var userId = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name)?.Id;
            var history = new ProductHistory
            {
                ProductId = product.Id,
                UserId = userId,
                Action = "Estado",
                QuantityChange = 0,
                NewStock = product.StockQuantity,
                Description = product.IsActive ? "Producto reactivado manualmente" : "Producto desactivado manualmente",
                Date = DateTime.Now
            };
            _context.Add(history);
            _context.Update(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}

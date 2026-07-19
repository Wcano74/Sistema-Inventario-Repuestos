using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaInventario.Data;
using SistemaInventario.Models.Entities;

namespace SistemaInventario.Controllers
{
    [Route("productos")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SistemaInventario.Services.IConfigurationService _configService;
        private readonly IWebHostEnvironment _env;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        public ProductsController(ApplicationDbContext context, SistemaInventario.Services.IConfigurationService configService, IWebHostEnvironment env)
        {
            _context = context;
            _configService = configService;
            _env = env;
        }

        private async Task<string?> SaveImageAsync(IFormFile imageFile)
        {
            var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                return null;
            if (imageFile.Length > MaxFileSize)
                return null;

            var fileName = $"{Guid.NewGuid()}{ext}";
            var uploadsDir = Path.Combine(_env.WebRootPath, "images", "products");
            Directory.CreateDirectory(uploadsDir);

            var filePath = Path.Combine(uploadsDir, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }
            return $"/images/products/{fileName}";
        }

        private void DeleteOldImage(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl) || !imageUrl.StartsWith("/images/products/"))
                return;
            var filePath = Path.Combine(_env.WebRootPath, imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }

        private async Task<bool> CanPerformVendedorAction(string permissionKey)
        {
            if (User.IsInRole("Admin")) return true;
            if (User.IsInRole("Bodeguero")) return true;
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
                .Include(p => p.WarehouseLocation)
                    .ThenInclude(wl => wl.WarehouseRack)
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
                .Include(p => p.WarehouseLocation)
                    .ThenInclude(wl => wl.WarehouseRack)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm)
                    || p.Barcode.Contains(searchTerm)
                    || (p.Brand != null && p.Brand.Contains(searchTerm))
                    || (p.Description != null && p.Description.Contains(searchTerm))
                    || (p.Category != null && p.Category.Name.Contains(searchTerm)));
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
            
            var viewModel = new SistemaInventario.Models.ViewModels.ProductIndexViewModel
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
            
            // Check individual user permission for Inventory Value or Admin role
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            ViewBag.SeeInventoryTotal = User.IsInRole("Admin") || (currentUser != null && currentUser.CanViewInventoryValue);

            ViewBag.CanAddToPOS = !User.IsInRole("Bodeguero");
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
                .Include(p => p.WarehouseLocation)
                    .ThenInclude(wl => wl.WarehouseRack)
                        .ThenInclude(r => r.Warehouse)
                .Include(p => p.HistoryLogs).ThenInclude(h => h.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (product == null) return NotFound();

            var viewModel = new SistemaInventario.Models.ViewModels.ProductDetailsViewModel
            {
                Product = product
            };

            // Map HistoryLogs to MovementViewModel
            viewModel.RecentMovements = product.HistoryLogs
                .OrderByDescending(h => h.Date)
                .Take(20)
                .Select(h => new SistemaInventario.Models.ViewModels.MovementViewModel
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
            ViewData["WarehouseLocationId"] = await GetLocationSelectList();
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [Route("crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Barcode,Price,Cost,StockQuantity,MinStock,ImageUrl,CategoryId,SupplierId,WarehouseLocationId")] Product product, IFormFile? imageFile)
        {
            if (!await CanPerformVendedorAction("Vendedor_CanEditStock")) return Forbid();
            if (ModelState.IsValid)
            {
                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    var savedPath = await SaveImageAsync(imageFile);
                    if (savedPath != null)
                        product.ImageUrl = savedPath;
                    else
                    {
                        ModelState.AddModelError("ImageUrl", "Archivo no válido. Solo se permiten imágenes (jpg, png, webp, gif) de hasta 5MB.");
                        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
                        ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", product.SupplierId);
                        ViewData["WarehouseLocationId"] = await GetLocationSelectList(product.WarehouseLocationId);
                        return View(product);
                    }
                }

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
            ViewData["WarehouseLocationId"] = await GetLocationSelectList(product.WarehouseLocationId);
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
            
            if (product.WarehouseLocationId.HasValue)
            {
                var loc = await _context.WarehouseLocations.Include(l => l.WarehouseRack).FirstOrDefaultAsync(l => l.Id == product.WarehouseLocationId);
                if (loc != null)
                {
                    ViewBag.SelectedRackId = loc.WarehouseRackId;
                    ViewBag.SelectedWarehouseId = loc.WarehouseRack.WarehouseId;
                }
            }
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [Route("editar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Barcode,Price,Cost,StockQuantity,MinStock,ImageUrl,CategoryId,SupplierId,Brand,IsActive,WarehouseLocationId")] Product product, IFormFile? imageFile)
        {
            if (!await CanPerformVendedorAction("Vendedor_CanEditStock")) return Forbid();
            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var originalProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                    if (originalProduct == null) return NotFound();

                    // Handle image upload
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var savedPath = await SaveImageAsync(imageFile);
                        if (savedPath != null)
                        {
                            DeleteOldImage(originalProduct.ImageUrl);
                            product.ImageUrl = savedPath;
                        }
                        else
                        {
                            ModelState.AddModelError("ImageUrl", "Archivo no válido. Solo se permiten imágenes (jpg, png, webp, gif) de hasta 5MB.");
                            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
                            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", product.SupplierId);
                            ViewData["WarehouseLocationId"] = await GetLocationSelectList(product.WarehouseLocationId);
                            return View(product);
                        }
                    }
                    else if (string.IsNullOrEmpty(product.ImageUrl))
                    {
                        // If user cleared the image, delete the old local file
                        DeleteOldImage(originalProduct.ImageUrl);
                    }

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
                    if (originalProduct.ImageUrl != product.ImageUrl) changes.Add("Imagen actualizada");
                    if (originalProduct.WarehouseLocationId != product.WarehouseLocationId) changes.Add("Ubicación cambiada");

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
            if (product.WarehouseLocationId.HasValue)
            {
                var loc = await _context.WarehouseLocations.Include(l => l.WarehouseRack).FirstOrDefaultAsync(l => l.Id == product.WarehouseLocationId);
                if (loc != null)
                {
                    ViewBag.SelectedRackId = loc.WarehouseRackId;
                    ViewBag.SelectedWarehouseId = loc.WarehouseRack.WarehouseId;
                }
            }
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

        [HttpGet("~/api/warehouses")]
        public async Task<IActionResult> GetWarehouses()
        {
            var warehouses = await _context.Warehouses
                .Where(w => w.IsActive)
                .OrderBy(w => w.Name)
                .Select(w => new { id = w.Id, name = w.Name })
                .ToListAsync();
            return Json(warehouses);
        }

        [HttpGet("~/api/racks/{warehouseId}")]
        public async Task<IActionResult> GetRacksByWarehouse(int warehouseId)
        {
            var racks = await _context.WarehouseRacks
                .Where(r => r.WarehouseId == warehouseId && r.IsActive)
                .OrderBy(r => r.Name)
                .Select(r => new { id = r.Id, name = r.Name })
                .ToListAsync();
            return Json(racks);
        }

        [HttpGet("~/api/locations/{rackId}")]
        public async Task<IActionResult> GetLocationsByRack(int rackId)
        {
            var locations = await _context.WarehouseLocations
                .Where(l => l.WarehouseRackId == rackId)
                .OrderBy(l => l.Row)
                .Select(l => new { id = l.Id, name = "Fila " + l.Row })
                .ToListAsync();
            return Json(locations);
        }

        private async Task<SelectList> GetLocationSelectList(int? selectedId = null)
        {
            var locations = await _context.WarehouseLocations
                .Include(l => l.WarehouseRack)
                .Where(l => l.WarehouseRack != null && l.WarehouseRack.IsActive)
                .OrderBy(l => l.WarehouseRack!.Name)
                .ThenBy(l => l.Row)
                .Select(l => new {
                    l.Id,
                    DisplayName = l.WarehouseRack!.Name + "-F" + l.Row +
                        (l.Description != null ? " (" + l.Description + ")" : "")
                })
                .ToListAsync();

            return new SelectList(locations, "Id", "DisplayName", selectedId);
        }
    }
}

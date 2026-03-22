using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaInventario.Data;
using SistemaInventario.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SistemaInventario.Controllers
{
    [Route("pedidos")]
    public class PurchaseOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;

        public PurchaseOrdersController(ApplicationDbContext context, Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager, SistemaInventario.Services.IConfigurationService configService)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: PurchaseOrders
        [Route("")]
        [Route("index")]
        public async Task<IActionResult> Index(int? supplierId, PurchaseOrderStatus? status)
        {
            // Permission Check
            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && !User.IsInRole("Bodeguero") && user != null)
            {
                if (!user.CanAccessPurchases) return RedirectToAction("Index", "Home");
            }

            var query = _context.PurchaseOrders
                .Include(p => p.Supplier)
                .AsQueryable();

            if (supplierId.HasValue)
            {
                query = query.Where(p => p.SupplierId == supplierId);
            }

            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status);
            }

            ViewBag.Suppliers = new SelectList(await _context.Suppliers.OrderBy(s => s.Name).ToListAsync(), "Id", "Name");
            return View(await query.OrderByDescending(p => p.OrderDate).ToListAsync());
        }

        // GET: PurchaseOrders/Details/5
        [Route("detalle/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var purchaseOrder = await _context.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (purchaseOrder == null) return NotFound();

            return View(purchaseOrder);
        }

        // GET: PurchaseOrders/Create
        // GET: PurchaseOrders/Create
        [Route("crear")]
        public async Task<IActionResult> Create()
        {
            // Permission Check
            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && !User.IsInRole("Bodeguero") && user != null)
            {
                if (!user.CanCreateOrders) 
                {
                    TempData["Error"] = "No tienes permisos para crear pedidos.";
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewData["SupplierId"] = new SelectList(_context.Suppliers.OrderBy(s => s.Name), "Id", "Name");
            ViewData["ProductId"] = new SelectList(_context.Products.Where(p => p.IsActive).OrderBy(p => p.Name), "Id", "Name");
            return View();
        }

        // POST: PurchaseOrders/Create
        [HttpPost]
        [Route("crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SupplierId,ExpectedDate,Notes")] PurchaseOrder purchaseOrder, string[] productIds, string[] quantities, string[] costs)
        {
            if (ModelState.IsValid)
            {
                // Validate Items presence
                if (productIds == null || productIds.Length == 0)
                {
                    TempData["Error"] = "El pedido debe tener al menos un producto.";
                    ViewData["SupplierId"] = new SelectList(_context.Suppliers.OrderBy(s => s.Name), "Id", "Name", purchaseOrder.SupplierId);
                    ViewData["ProductId"] = new SelectList(_context.Products.Where(p => p.IsActive).OrderBy(p => p.Name), "Id", "Name");
                    return View(purchaseOrder);
                }

                // Permission Check
                var user = await _userManager.GetUserAsync(User);
                if (!User.IsInRole("Admin") && !User.IsInRole("Bodeguero") && user != null)
                {
                    if (!user.CanCreateOrders) return Forbid();
                }

                purchaseOrder.OrderDate = DateTime.Now;
                purchaseOrder.Status = PurchaseOrderStatus.Pending;
                purchaseOrder.TotalAmount = 0;
                purchaseOrder.CreatedByUserId = user?.Id;

                // Process Items
                if (productIds != null)
                {
                    for (int i = 0; i < productIds.Length; i++)
                    {
                        if (int.TryParse(productIds[i], out int pid) && 
                            int.TryParse(quantities[i], out int qty) && qty > 0 &&
                            decimal.TryParse(costs[i], out decimal cost))
                        {
                            var item = new PurchaseOrderItem
                            {
                                ProductId = pid,
                                Quantity = qty,
                                UnitCost = cost,
                                TotalCost = qty * cost
                            };
                            purchaseOrder.Items.Add(item);
                            purchaseOrder.TotalAmount += item.TotalCost;
                        }
                    }
                }

                _context.Add(purchaseOrder);
                await _context.SaveChangesAsync();

                // Log Audit for Items
                foreach(var item in purchaseOrder.Items)
                {
                    var pHistory = new ProductHistory {
                        ProductId = item.ProductId,
                        Action = "Pedido Solicitado",
                        QuantityChange = 0, 
                        NewStock = (await _context.Products.Where(p => p.Id == item.ProductId).Select(p => p.StockQuantity).FirstOrDefaultAsync()), // Stock doesn't change yet
                        Description = $"Solicitado en Pedido #{purchaseOrder.Id} (Cant: {item.Quantity})",
                        Date = DateTime.Now,
                        UserId = user?.Id
                    };
                    _context.ProductHistories.Add(pHistory);
                }
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            ViewData["SupplierId"] = new SelectList(_context.Suppliers.OrderBy(s => s.Name), "Id", "Name", purchaseOrder.SupplierId);
            return View(purchaseOrder);
        }

        // POST: PurchaseOrders/Receive/5
        [HttpPost]
        [Route("recibir/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Receive(int id)
        {
            // Permission Check
            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && !User.IsInRole("Bodeguero") && user != null)    
            {
                if (!user.CanManageOrders) 
                {
                    TempData["Error"] = "No tienes permisos para recibir pedidos.";
                    return RedirectToAction(nameof(Details), new { id = id });
                }
            }

            var order = await _context.PurchaseOrders
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (order == null) return NotFound();
            if (order.Status != PurchaseOrderStatus.Pending)
            {
                TempData["Error"] = "Solo se pueden recibir pedidos pendientes.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            // var user = await _userManager.GetUserAsync(User); // Already fetched above

            // Update Status
            order.Status = PurchaseOrderStatus.Received;
            order.ReceivedByUserId = user?.Id;

            // Update Stock
            foreach (var item in order.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    // Create Product History Log
                    var history = new ProductHistory
                    {
                        ProductId = product.Id,
                        Action = "Compra Recibida",
                        QuantityChange = item.Quantity,
                        NewStock = product.StockQuantity + item.Quantity,
                        Description = $"Pedido #{order.Id} recibido.",
                        Date = DateTime.Now,
                        UserId = user?.Id
                    };
                    _context.ProductHistories.Add(history);

                    product.StockQuantity += item.Quantity;
                    _context.Update(product);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Pedido marcado como recibido y stock actualizado.";
            return RedirectToAction(nameof(Details), new { id = id });
        }

        // POST: PurchaseOrders/Cancel/5
        [HttpPost]
        [Route("cancelar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            // Permission Check
            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && !User.IsInRole("Bodeguero") && user != null)
            {
                if (!user.CanManageOrders) 
                {
                    TempData["Error"] = "No tienes permisos para cancelar pedidos.";
                    return RedirectToAction(nameof(Details), new { id = id });
                }
            }

            var order = await _context.PurchaseOrders
                .Include(p => p.Items) // Include items to log history
                .FirstOrDefaultAsync(p => p.Id == id);

            if (order == null) return NotFound();

            if (order.Status != PurchaseOrderStatus.Pending)
            {
                TempData["Error"] = "Solo se pueden cancelar pedidos pendientes.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            // var user = await _userManager.GetUserAsync(User); // Already fetched above

            order.Status = PurchaseOrderStatus.Canceled;
            order.CanceledByUserId = user?.Id;

            // Audit Log for Cancel
            foreach(var item in order.Items)
            {
                var pHistory = new ProductHistory {
                    ProductId = item.ProductId,
                    Action = "Pedido Cancelado",
                    QuantityChange = 0, 
                    NewStock = (await _context.Products.Where(p => p.Id == item.ProductId).Select(p => p.StockQuantity).FirstOrDefaultAsync()),
                    Description = $"Pedido #{order.Id} fue anulado.",
                    Date = DateTime.Now,
                    UserId = user?.Id
                };
                _context.ProductHistories.Add(pHistory);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Pedido cancelado.";
            return RedirectToAction(nameof(Details), new { id = id });
        }
    
        // API to get products for a supplier (optional/nice to have for filtering in Create view)
         [HttpGet]
         [Route("api/products")]
         public async Task<IActionResult> GetProducts(int? supplierId)
         {
             var query = _context.Products.Where(p => p.IsActive);
             if (supplierId.HasValue)
             {
                 query = query.Where(p => p.SupplierId == supplierId || p.SupplierId == null);
             }
             
             var products = await query.Select(p => new {
                 id = p.Id,
                 name = p.Name,
                 cost = p.Cost,
                 stock = p.StockQuantity,
                 minStock = p.MinStock
             }).ToListAsync();

             return Json(products);
         }
    }
}

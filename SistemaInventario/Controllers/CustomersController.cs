using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaInventario.Data;
using SistemaInventario.Models.Entities;
using SistemaInventario.Models.ViewModels;

namespace SistemaInventario.Controllers
{
    [Route("clientes")]
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SistemaInventario.Services.IConfigurationService _configService;

        public CustomersController(ApplicationDbContext context, SistemaInventario.Services.IConfigurationService configService)
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

        // GET: Customers
        [Route("")]
        [Route("index")]
        public async Task<IActionResult> Index()
        {
            ViewBag.CanEditStock = await CanPerformVendedorAction();
            var customers = await _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
            return View(customers);
        }

        // GET: Customers/Details/5
        [Route("detalle/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (customer == null) return NotFound();

            // Get customer's purchase history
            var sales = await _context.Sales
                .Include(s => s.SaleDetails)
                .ThenInclude(sd => sd.Product)
                .Where(s => s.CustomerId == id)
                .OrderByDescending(s => s.Date)
                .Take(20)
                .ToListAsync();

            var viewModel = new CustomerDetailsViewModel
            {
                Customer = customer,
                RecentSales = sales,
                TotalPurchased = sales.Sum(s => s.Total),
                TotalTransactions = sales.Count,
                LastPurchaseDate = sales.FirstOrDefault()?.Date
            };

            return View(viewModel);
        }

        // GET: Customers/Create
        [Route("crear")]
        public async Task<IActionResult> Create()
        {
            if (!await CanPerformVendedorAction()) return Forbid();
            return View();
        }

        // POST: Customers/Create
        [HttpPost]
        [Route("crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,NitDpi,Phone,Email,Address,FiscalName,FiscalAddress")] Customer customer)
        {
            if (!await CanPerformVendedorAction()) return Forbid();
            
            if (ModelState.IsValid)
            {
                customer.CreatedAt = DateTime.Now;
                customer.IsActive = true;
                _context.Add(customer);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cliente registrado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Customers/Edit/5
        [Route("editar/{id?}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!await CanPerformVendedorAction()) return Forbid();
            if (id == null) return NotFound();

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        // POST: Customers/Edit/5
        [HttpPost]
        [Route("editar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,NitDpi,Phone,Email,Address,FiscalName,FiscalAddress,IsActive,CreatedAt,LastPurchaseDate")] Customer customer)
        {
            if (!await CanPerformVendedorAction()) return Forbid();
            if (id != customer.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cliente actualizado correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Customers/Delete/5
        [Route("eliminar/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!await CanPerformVendedorAction()) return Forbid();
            if (id == null) return NotFound();

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (customer == null) return NotFound();

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("eliminar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!await CanPerformVendedorAction()) return Forbid();
            
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            // Check for linked sales
            var hasSales = await _context.Sales.AnyAsync(s => s.CustomerId == id);
            if (hasSales)
            {
                TempData["Error"] = "No se puede eliminar el cliente porque tiene ventas asociadas. Puede desactivarlo en su lugar.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cliente eliminado correctamente.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Error al intentar eliminar el cliente.";
            }

            return RedirectToAction(nameof(Index));
        }

        // API: Search customers (for POS)
        [HttpGet]
        [Route("api/buscar")]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new List<object>());
            }

            var customers = await _context.Customers
                .Where(c => c.IsActive && 
                       (c.Name.Contains(query) || 
                        (c.NitDpi != null && c.NitDpi.Contains(query)) ||
                        (c.Phone != null && c.Phone.Contains(query))))
                .Take(10)
                .Select(c => new {
                    id = c.Id,
                    name = c.Name,
                    nitDpi = c.NitDpi,
                    phone = c.Phone,
                    email = c.Email
                })
                .ToListAsync();

            return Json(customers);
        }

        // API: Quick create customer (from POS)
        [HttpPost]
        [Route("api/crear-rapido")]
        public async Task<IActionResult> QuickCreate([FromBody] Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer.Name))
            {
                return BadRequest("El nombre es obligatorio");
            }

            try
            {
                customer.CreatedAt = DateTime.Now;
                customer.IsActive = true;
                _context.Add(customer);
                await _context.SaveChangesAsync();

                return Json(new {
                    success = true,
                    customer = new {
                        id = customer.Id,
                        name = customer.Name,
                        nitDpi = customer.NitDpi,
                        phone = customer.Phone,
                        email = customer.Email
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al crear cliente: {ex.Message}");
            }
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.Id == id);
        }
    }
}

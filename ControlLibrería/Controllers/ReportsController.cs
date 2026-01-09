using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ControlLibrería.Data;
using ControlLibrería.Models.ViewModels;
using ControlLibrería.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace ControlLibrería.Controllers
{
    [Route("reportes")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Route("")]
        [Route("index")]
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, string period = "week")
        {
            if (!User.IsInRole("Admin")) return Forbid();

            var viewModel = new DashboardViewModel();
            
            // Handle predefined periods
            DateTime now = DateTime.Now;
            if (!startDate.HasValue || !endDate.HasValue)
            {
                switch (period.ToLower())
                {
                    case "day":
                        startDate = now.Date;
                        endDate = now.Date.AddDays(1).AddTicks(-1);
                        break;
                    case "month":
                        startDate = new DateTime(now.Year, now.Month, 1);
                        endDate = startDate.Value.AddMonths(1).AddTicks(-1);
                        break;
                    case "week":
                        startDate = now.AddDays(-6).Date;
                        endDate = now.Date.AddDays(1).AddTicks(-1);
                        break;
                    default:
                        startDate = now.AddDays(-(int)now.DayOfWeek).Date;
                        endDate = startDate.Value.AddDays(7).AddTicks(-1);
                        break;
                }
            }

            // Final fallback if custom period was requested without dates
            if (!startDate.HasValue) startDate = now.AddDays(-7).Date;
            if (!endDate.HasValue) endDate = now.Date.AddDays(1).AddTicks(-1);

            // 1. Sales Summary and COGS in Period (excluding cancelled sales)
            var stats = await _context.Sales
                .Where(s => s.Date >= startDate && s.Date <= endDate && s.Status != SaleStatus.Cancelled)
                .SelectMany(s => s.SaleDetails)
                .GroupBy(sd => 1)
                .Select(g => new { 
                    Total = g.Sum(sd => sd.Subtotal), 
                    Cost = g.Sum(sd => sd.Quantity * sd.Product.Cost),
                    Count = g.Select(sd => sd.SaleId).Distinct().Count() 
                })
                .FirstOrDefaultAsync();

            if (stats != null)
            {
                viewModel.DailySales.TotalSales = stats.Total;
                viewModel.DailySales.TotalCost = stats.Cost;
                viewModel.DailySales.TotalProfit = stats.Total - stats.Cost;
                viewModel.DailySales.TransactionCount = stats.Count;
            }

            // 2. Expenses in Period
            viewModel.TotalExpenses = await _context.Expenses
                .Where(e => e.Date >= startDate && e.Date <= endDate)
                .SumAsync(e => e.Amount);

            viewModel.GrossProfit = viewModel.DailySales.TotalProfit;
            viewModel.NetProfit = viewModel.GrossProfit - viewModel.TotalExpenses;

            // 2. Dynamic Performance Chart Data
            var diff = (endDate.Value - startDate.Value).TotalDays;
            if (diff > 90) // Group by Month
            {
                var monthlyData = await _context.SaleDetails
                    .Where(sd => sd.Sale.Date >= startDate && sd.Sale.Date <= endDate && sd.Sale.Status != SaleStatus.Cancelled)
                    .GroupBy(sd => new { Year = sd.Sale.Date.Year, Month = sd.Sale.Date.Month })
                    .Select(g => new { 
                        Total = g.Sum(sd => sd.Subtotal),
                        Profit = g.Sum(sd => sd.Subtotal - (sd.Quantity * sd.Product.Cost)),
                        Date = new DateTime(g.Key.Year, g.Key.Month, 1)
                    })
                    .OrderBy(g => g.Date)
                    .ToListAsync();
                
                foreach (var item in monthlyData)
                {
                    viewModel.WeeklySales.Add(new WeeklySalesResult { 
                        DayName = item.Date.ToString("MMM yyyy", new System.Globalization.CultureInfo("es-ES")), 
                        TotalSales = item.Total,
                        TotalProfit = item.Profit
                    });
                }
            }
            else if (diff > 31) // Group by Week
            {
                // We fetch the data and group in memory to ensure ISOWeek compatibility across different SQL providers
                var salesData = await _context.SaleDetails
                    .Where(sd => sd.Sale.Date >= startDate && sd.Sale.Date <= endDate && sd.Sale.Status != SaleStatus.Cancelled)
                    .Select(sd => new {
                        Date = sd.Sale.Date,
                        Subtotal = sd.Subtotal,
                        Cost = sd.Quantity * sd.Product.Cost
                    })
                    .ToListAsync();

                var weeklyGroups = salesData
                    .GroupBy(s => System.Globalization.ISOWeek.GetYear(s.Date) + "-" + System.Globalization.ISOWeek.GetWeekOfYear(s.Date))
                    .Select(g => new {
                        Total = g.Sum(s => s.Subtotal),
                        Profit = g.Sum(s => s.Subtotal - s.Cost),
                        StartDate = g.Min(s => s.Date)
                    })
                    .OrderBy(g => g.StartDate);

                foreach (var item in weeklyGroups)
                {
                    viewModel.WeeklySales.Add(new WeeklySalesResult {
                        DayName = "Sem " + System.Globalization.ISOWeek.GetWeekOfYear(item.StartDate) + " (" + item.StartDate.ToString("dd/MM") + ")",
                        TotalSales = item.Total,
                        TotalProfit = item.Profit
                    });
                }
            }
            else // Group by Day
            {
                var dailyData = await _context.SaleDetails
                    .Where(sd => sd.Sale.Date >= startDate && sd.Sale.Date <= endDate && sd.Sale.Status != SaleStatus.Cancelled)
                    .GroupBy(sd => sd.Sale.Date.Date)
                    .Select(g => new { 
                        Date = g.Key, 
                        Total = g.Sum(sd => sd.Subtotal),
                        Profit = g.Sum(sd => sd.Subtotal - (sd.Quantity * sd.Product.Cost))
                    })
                    .OrderBy(g => g.Date)
                    .ToListAsync();

                foreach (var item in dailyData)
                {
                    viewModel.WeeklySales.Add(new WeeklySalesResult { 
                        DayName = item.Date.ToString("dd/MM"), 
                        TotalSales = item.Total,
                        TotalProfit = item.Profit
                    });
                }
            }

            // 3. Top Products in Period
            viewModel.TopProducts = await _context.SaleDetails
                .Include(sd => sd.Sale)
                .Include(sd => sd.Product)
                .Where(sd => sd.Sale.Date >= startDate && sd.Sale.Date <= endDate && sd.Sale.Status != SaleStatus.Cancelled)
                .GroupBy(sd => sd.Product.Name)
                .Select(g => new TopProductResult {
                    Name = g.Key,
                    TotalSold = g.Sum(sd => sd.Quantity),
                    Revenue = g.Sum(sd => sd.Subtotal)
                })
                .OrderByDescending(p => p.Revenue)
                .Take(5)
                .ToListAsync();

            // 4. Sales by User
            viewModel.UserSales = await (
                from sale in _context.Sales
                where sale.Date >= startDate && sale.Date <= endDate && sale.Status != SaleStatus.Cancelled
                join user in _context.Users on sale.UserId equals user.EmployeeCode into userGroup
                from u in userGroup.DefaultIfEmpty()
                select new { 
                    Sale = sale,
                    FirstName = u != null ? u.FirstName : null,
                    LastName = u != null ? u.LastName : null,
                    UserId = sale.UserId
                })
                .GroupBy(x => new { x.UserId, x.FirstName, x.LastName })
                .Select(g => new UserSalesReportResult {
                    UserName = g.Key.FirstName != null && g.Key.LastName != null 
                        ? g.Key.FirstName + " " + g.Key.LastName 
                        : "Sistema",
                    TransactionCount = g.Count(),
                    TotalSales = g.Sum(x => x.Sale.Total)
                })
                .OrderByDescending(u => u.TotalSales)
                .ToListAsync();
            


            // Global stats
            viewModel.LowStockProducts = await _context.Products.Include(p => p.Category).Where(p => p.StockQuantity <= p.MinStock).ToListAsync();
            viewModel.TotalInventoryValue = await _context.Products.SumAsync(p => p.Price * p.StockQuantity);
            viewModel.TotalProducts = await _context.Products.CountAsync();

            // Category distribution (stays the same)
            var distribution = await _context.Products.GroupBy(p => p.Category != null ? p.Category.Name : "Sin Categoría").Select(g => new { Name = g.Key, Count = g.Count() }).ToListAsync();
            string[] colors = { "#137fec", "#ec4899", "#f59e0b", "#10b981", "#8b5cf6" };
            int i = 0;
            foreach (var item in distribution)
            {
                viewModel.CategoryDistribution.Add(new CategoryDistributionResult {
                    CategoryName = item.Name, Count = item.Count,
                    Percentage = viewModel.TotalProducts > 0 ? (double)item.Count / viewModel.TotalProducts * 100 : 0,
                    Color = colors[i % colors.Length]
                });
                i++;
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(viewModel);
            }

            return View(viewModel);
        }

        [Route("historial")]
        public async Task<IActionResult> History(int page = 1, int pageSize = 10)
        {
            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && (user == null || !user.CanViewAuditLog))
            {
                TempData["Error"] = "No tienes permiso para ver la bitácora.";
                return RedirectToAction("Index", "Home");
            }

            var query = _context.ProductHistories
                .Include(h => h.Product)
                .Include(h => h.User)
                .OrderByDescending(h => h.Date);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;

            return View(items);
        }
    }
}

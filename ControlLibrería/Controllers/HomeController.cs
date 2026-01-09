using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ControlLibrería.Models;
using Microsoft.EntityFrameworkCore;
using ControlLibrería.Models.Entities;

namespace ControlLibrería.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ControlLibrería.Data.ApplicationDbContext _context;
    private readonly ControlLibrería.Services.IConfigurationService _configService;
    private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;

    public HomeController(ILogger<HomeController> logger, ControlLibrería.Data.ApplicationDbContext context, ControlLibrería.Services.IConfigurationService configService, Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _configService = configService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        if (User.IsInRole("Vendedor"))
        {
            var defaultPage = await _configService.GetConfigurationAsync("Vendedor_DefaultPage", "ventas/pos");
            if (defaultPage != "Home")
            {
                return Redirect("/" + defaultPage);
            }
        }

        var viewModel = new ControlLibrería.Models.ViewModels.DashboardViewModel();

        try 
        {
            // 1. Daily Sales and Profits (Today)
            var today = DateTime.Today;
            var nextDay = today.AddDays(1);
            
            var dailyData = await _context.Sales
                .Where(s => s.Date >= today && s.Date < nextDay)
                .SelectMany(s => s.SaleDetails)
                .GroupBy(sd => 1)
                .Select(g => new { 
                    Total = g.Sum(sd => sd.Subtotal), 
                    Cost = g.Sum(sd => sd.Quantity * sd.Product.Cost),
                    Count = g.Select(sd => sd.SaleId).Distinct().Count() 
                })
                .FirstOrDefaultAsync();

            if (dailyData != null)
            {
                viewModel.DailySales.TotalSales = dailyData.Total;
                viewModel.DailySales.TotalCost = dailyData.Cost;
                viewModel.DailySales.TotalProfit = dailyData.Total - dailyData.Cost;
                viewModel.DailySales.TransactionCount = dailyData.Count;
            }

            // Today's Expenses
            viewModel.TotalExpenses = await _context.Expenses
                .Where(e => e.Date >= today && e.Date < nextDay)
                .SumAsync(e => e.Amount);
            
            viewModel.NetProfit = viewModel.DailySales.TotalProfit - viewModel.TotalExpenses;

            // 2. Weekly Sales & Profit for Chart (Last 7 days)
            var weekStart = DateTime.Today.AddDays(-6);
            var weeklyChartData = await _context.Sales
                .Where(s => s.Date >= weekStart)
                .SelectMany(s => s.SaleDetails)
                .GroupBy(sd => sd.Sale.Date.Date)
                .Select(g => new { 
                    Date = g.Key, 
                    Total = g.Sum(sd => sd.Subtotal),
                    Profit = g.Sum(sd => sd.Subtotal - (sd.Quantity * sd.Product.Cost))
                })
                .OrderBy(g => g.Date)
                .ToListAsync();

            foreach (var item in weeklyChartData)
            {
                viewModel.WeeklySales.Add(new ControlLibrería.Models.ViewModels.WeeklySalesResult
                {
                    DayName = item.Date.ToString("ddd", new System.Globalization.CultureInfo("es-ES")),
                    TotalSales = item.Total,
                    TotalProfit = item.Profit
                });
            }
            
            // 3. Counts & Alerts
            viewModel.TotalProducts = await _context.Products.CountAsync();
            viewModel.LowStockCount = await _context.Products.CountAsync(p => p.StockQuantity <= p.MinStock);
            viewModel.LowStockProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.StockQuantity <= p.MinStock)
                .OrderBy(p => p.StockQuantity)
                .Take(5)
                .ToListAsync();

            // 4. Recent Sales
            viewModel.RecentSales = await _context.Sales
                .OrderByDescending(s => s.Date)
                .Take(5)
                .ToListAsync();

            // 5. Category Distribution
            var distribution = await _context.Products
                .GroupBy(p => p.Category != null ? p.Category.Name : "Sin Categoría")
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .ToListAsync();

            // 6. Recent Activities (Movements)
            viewModel.RecentActivities = await _context.ProductHistories
                .Include(h => h.Product)
                .Include(h => h.User)
                .OrderByDescending(h => h.Date)
                .Take(10)
                .ToListAsync();

            int totalItems = distribution.Sum(d => d.Count);
            string[] colors = { "#137fec", "#ec4899", "#f59e0b", "#10b981", "#8b5cf6" };
            int colorIndex = 0;

            foreach (var item in distribution)
            {
                viewModel.CategoryDistribution.Add(new ControlLibrería.Models.ViewModels.CategoryDistributionResult
                {
                    CategoryName = item.Name,
                    Count = item.Count,
                    Percentage = totalItems > 0 ? (double)item.Count / totalItems * 100 : 0,
                    Color = colors[colorIndex % colors.Length]
                });
                colorIndex++;
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard data");
            // Fail gracefully if DB issue
        }

        // Load WhatsApp and Permission configurations
        ViewBag.WhatsAppNumber = await _configService.GetConfigurationAsync("WhatsAppNumber", "");
        var alertsEnabled = await _configService.GetConfigurationAsync("WhatsAppAlertsEnabled", "false");
        ViewBag.WhatsAppAlertsEnabled = string.Equals(alertsEnabled, "true", StringComparison.OrdinalIgnoreCase);
        ViewBag.VendedorSeeDashboardCharts = (await _configService.GetConfigurationAsync("Vendedor_SeeDashboardCharts", "true")) == "true";
        
        var user = await _userManager.GetUserAsync(User);
        ViewBag.CanViewAuditLog = user?.CanViewAuditLog ?? false;

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
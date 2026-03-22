using Microsoft.AspNetCore.Identity;
using SistemaInventario.Data;
using Microsoft.Extensions.DependencyInjection;
using SistemaInventario.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace SistemaInventario.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // 1. Create Roles
            string[] roleNames = { "Admin", "Vendedor", "Bodeguero" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Create Admin User
            string adminCode = "00001";
            var adminUser = await userManager.Users.FirstOrDefaultAsync(u => u.EmployeeCode == adminCode);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminCode,
                    Email = "admin@sistema.local",
                    EmailConfirmed = true,
                    FirstName = "Administrador",
                    LastName = "Sistema",
                    DPI = "0000000000000",
                    EmployeeCode = adminCode,
                    Age = 30,
                    Salary = 0,
                    CanProcessRefunds = true
                };
                await userManager.CreateAsync(adminUser, "Admin123!");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // 3. Create Default Customer
            if (!context.Customers.Any())
            {
                context.Customers.Add(new Customer
                {
                    Name = "Cliente General",
                    NitDpi = "C/F",
                    Phone = "",
                    Email = "",
                    Address = "",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                });
            }

            // 4. Seed Default Permissions
            if (!context.SystemConfigurations.Any(c => c.Key == "Vendedor_CanEditStock"))
            {
                context.SystemConfigurations.Add(new SystemConfiguration { Key = "Vendedor_CanEditStock", Value = "false", Description = "Vendedor puede editar stock" });
            }
            if (!context.SystemConfigurations.Any(c => c.Key == "Vendedor_SeeCostPrice"))
            {
                context.SystemConfigurations.Add(new SystemConfiguration { Key = "Vendedor_SeeCostPrice", Value = "false", Description = "Vendedor puede ver precios de costo" });
            }
            if (!context.SystemConfigurations.Any(c => c.Key == "Vendedor_DefaultPage"))
            {
                context.SystemConfigurations.Add(new SystemConfiguration { Key = "Vendedor_DefaultPage", Value = "ventas/pos", Description = "Página de inicio para vendedores" });
            }
            if (!context.SystemConfigurations.Any(c => c.Key == "Vendedor_SeeInventoryTotal"))
            {
                context.SystemConfigurations.Add(new SystemConfiguration { Key = "Vendedor_SeeInventoryTotal", Value = "false", Description = "Vendedor puede ver el valor total del inventario" });
            }

            // Bodeguero default page
            if (!context.SystemConfigurations.Any(c => c.Key == "Bodeguero_DefaultPage"))
            {
                context.SystemConfigurations.Add(new SystemConfiguration { Key = "Bodeguero_DefaultPage", Value = "productos", Description = "Página de inicio para bodegueros" });
            }

            // 5. Seed Customer Configurations
            if (!context.SystemConfigurations.Any(c => c.Key == "Customer_RequiredInPOS"))
            {
                context.SystemConfigurations.Add(new SystemConfiguration { Key = "Customer_RequiredInPOS", Value = "false", Description = "Requerir cliente en cada venta" });
            }
            if (!context.SystemConfigurations.Any(c => c.Key == "Customer_DefaultId"))
            {
                context.SystemConfigurations.Add(new SystemConfiguration { Key = "Customer_DefaultId", Value = "1", Description = "ID del cliente por defecto" });
            }
            if (!context.SystemConfigurations.Any(c => c.Key == "Customer_AllowQuickCreate"))
            {
                context.SystemConfigurations.Add(new SystemConfiguration { Key = "Customer_AllowQuickCreate", Value = "true", Description = "Permitir creación rápida desde POS" });
            }

            // 6. Seed Cash Register Configurations
            if (!context.SystemConfigurations.Any(c => c.Key == "CashRegister_RequireOpen"))
            {
                context.SystemConfigurations.Add(new SystemConfiguration { Key = "CashRegister_RequireOpen", Value = "true", Description = "Requerir caja abierta para ventas" });
            }
            if (!context.SystemConfigurations.Any(c => c.Key == "CashRegister_AllowMultipleSessions"))
            {
                context.SystemConfigurations.Add(new SystemConfiguration { Key = "CashRegister_AllowMultipleSessions", Value = "false", Description = "Permitir múltiples cajas abiertas" });
            }
            if (!context.SystemConfigurations.Any(c => c.Key == "CashRegister_DefaultPaymentMethod"))
            {
                context.SystemConfigurations.Add(new SystemConfiguration { Key = "CashRegister_DefaultPaymentMethod", Value = "Cash", Description = "Método de pago por defecto" });
            }

            // 7. Seed Refund Configurations
            if (!context.SystemConfigurations.Any(c => c.Key == "Refund_AllowPartialRefunds"))
            {
                context.SystemConfigurations.Add(new SystemConfiguration { Key = "Refund_AllowPartialRefunds", Value = "true", Description = "Permitir devoluciones parciales" });
            }
            if (!context.SystemConfigurations.Any(c => c.Key == "Refund_MaxHoursAfterSale"))
            {
                context.SystemConfigurations.Add(new SystemConfiguration { Key = "Refund_MaxHoursAfterSale", Value = "24", Description = "Horas máximas después de venta para devolución" });
            }
            if (!context.SystemConfigurations.Any(c => c.Key == "Refund_RequireReason"))
            {
                context.SystemConfigurations.Add(new SystemConfiguration { Key = "Refund_RequireReason", Value = "true", Description = "Motivo obligatorio para devoluciones" });
            }
            if (!context.SystemConfigurations.Any(c => c.Key == "Refund_RequireAdminRole"))
            {
                context.SystemConfigurations.Add(new SystemConfiguration { Key = "Refund_RequireAdminRole", Value = "true", Description = "Solo administradores pueden procesar devoluciones" });
            }
            if (!context.SystemConfigurations.Any(c => c.Key == "Refund_AllowNegativeCashRegister"))
            {
                context.SystemConfigurations.Add(new SystemConfiguration { Key = "Refund_AllowNegativeCashRegister", Value = "false", Description = "Permitir caja en negativo por devoluciones" });
            }
            if (!context.SystemConfigurations.Any(c => c.Key == "Refund_RegisterAsExpenseIfCashClosed"))
            {
                context.SystemConfigurations.Add(new SystemConfiguration { Key = "Refund_RegisterAsExpenseIfCashClosed", Value = "true", Description = "Registrar como egreso si caja cerrada" });
            }

            await context.SaveChangesAsync();
        }
    }
}

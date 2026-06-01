using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SistemaInventario.Data;
using SistemaInventario.Models.Entities;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
    options.ExpireTimeSpan = TimeSpan.FromDays(5);
    options.SlidingExpiration = true;
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 5 * 1024 * 1024; // 5MB
});

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddMemoryCache();
builder.Services.AddScoped<SistemaInventario.Services.IConfigurationService, SistemaInventario.Services.ConfigurationService>();

// Configurar localización a Español Guatemala
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("es-GT") };
    options.DefaultRequestCulture = new RequestCulture("es-GT");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

var app = builder.Build();

// Inicializar base de datos y datos semilla
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Crear la base de datos automáticamente si no existe
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        // Sembrar roles, usuario admin y configuraciones
        await SistemaInventario.Data.DbInitializer.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al inicializar la base de datos.");

        // Si Migrate falla (no hay migraciones), intentar con EnsureCreated
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureCreated();
            await SistemaInventario.Data.DbInitializer.Initialize(services);
        }
        catch (Exception innerEx)
        {
            var logger2 = services.GetRequiredService<ILogger<Program>>();
            logger2.LogError(innerEx, "Falló también EnsureCreated.");
        }
    }
}

// Verificar imágenes faltantes al iniciar
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var env     = services.GetRequiredService<IWebHostEnvironment>();
        var logger  = services.GetRequiredService<ILogger<Program>>();

        var productos = await context.Products
            .Where(p => p.ImageUrl != null && p.ImageUrl != "")
            .Select(p => new { p.Id, p.ImageUrl })
            .ToListAsync();

        var faltantes = productos.Count(p => {
            var rel  = p.ImageUrl!.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var path = Path.Combine(env.WebRootPath, rel);
            return !File.Exists(path);
        });

        if (faltantes > 0)
            logger.LogWarning("⚠️  IMÁGENES FALTANTES: {Faltantes}/{Total} productos tienen imagen en BD pero el archivo no existe en disco.", faltantes, productos.Count);
        else if (productos.Count > 0)
            logger.LogInformation("✅ Imágenes: {Total} archivos presentes en disco.", productos.Count);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("No se pudo verificar imágenes al iniciar: {Error}", ex.Message);
    }
}

// Configurar el pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // En Docker, HTTPS se gestiona con un proxy reverso (nginx, traefik, etc.)
}

app.UseMiddleware<SistemaInventario.Middleware.RequestLoggingMiddleware>();

app.UseStaticFiles();

// Habilitar localización por defecto
var supportedCultures = new[] { new CultureInfo("es-GT") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("es-GT"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
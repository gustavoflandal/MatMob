using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MatMob.Data;
using MatMob.Services;
using MatMob.Models.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configure Entity Framework with MySQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
           .EnableDetailedErrors()
           .EnableSensitiveDataLogging());

// Configure ASP.NET Core Identity
builder.Services.AddDefaultIdentity<MatMob.Models.Entities.ApplicationUser>(options => 
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddRoles<MatMob.Models.Entities.ApplicationRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Registrar o SignInManager auditável
//builder.Services.AddScoped<SignInManager<ApplicationUser>, MatMob.Services.AuditableSignInManager<ApplicationUser>>();

builder.Services.AddControllersWithViews();

// Add custom services
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAuditService, AuditService>();
// builder.Services.AddScoped<MatMob.Services.IPermissionService, MatMob.Services.PermissionService>(); // Temporariamente comentado
builder.Services.AddScoped<IAuditImmutabilityService, AuditImmutabilityService>();
builder.Services.AddScoped<EstoqueService>();

// Add background services
builder.Services.AddSingleton<AuditBackgroundService>();
builder.Services.AddSingleton<IAuditBackgroundService>(provider => provider.GetRequiredService<AuditBackgroundService>());
builder.Services.AddHostedService<AuditBackgroundService>(provider => provider.GetRequiredService<AuditBackgroundService>());

// Add memory cache
builder.Services.AddMemoryCache();

// Add session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Add session middleware
app.UseSession();

app.UseRouting();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Add audit middleware
// app.UseAuditMiddleware(); // Temporariamente comentado até resolver dependências

app.MapStaticAssets();

// Configure routing
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

    await InitializeDatabase(context, userManager, roleManager);
}

app.Run();

// Database initialization method
static async Task InitializeDatabase(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
{
    try
    {
        // Create roles if they don't exist
        string[] roles = { "Administrador", "Gestor", "Tecnico" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole(role));
            }
        }

        // Create default admin user
        var adminEmail = "admin@matmob.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "Administrador",
                LastName = "Sistema"
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Administrador");
            }
        }

        // Seed default permissions
        //await permissionService.SeedDefaultPermissionsAsync();
    }
    catch (Exception ex)
    {
        // Log the error but don't crash the application
        Console.WriteLine($"Erro na inicialização do banco de dados: {ex.Message}");
    }
}

// Classe pública para acesso em testes
public partial class Program { }

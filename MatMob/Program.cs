using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MatMob.Data;
using MatMob.Services;

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
builder.Services.AddDefaultIdentity<IdentityUser>(options => 
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Register custom SignInManager for audit
builder.Services.AddScoped<SignInManager<IdentityUser>, MatMob.Services.AuditableSignInManager<IdentityUser>>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<AuditActionFilter>();
});

// Add custom services
builder.Services.AddScoped<MatMob.Services.IDashboardService, MatMob.Services.DashboardService>();
builder.Services.AddScoped<MatMob.Services.EstoqueService>();

// Registrar serviços de auditoria
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddSingleton<AuditBackgroundService>();
builder.Services.AddHostedService<AuditBackgroundService>(provider => provider.GetService<AuditBackgroundService>()!);
builder.Services.AddScoped<AuthenticationAuditService>();
builder.Services.AddScoped<AuditImmutabilityService>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<AuditableSignInManager<IdentityUser>>();

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
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    await InitializeDatabase(context, userManager, roleManager);
}

app.Run();

// Database initialization method
static async Task InitializeDatabase(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
{
    try
    {
        // Create roles if they don't exist
        string[] roles = { "Administrador", "Gestor", "Tecnico" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Create default admin user
        var adminEmail = "admin@matmob.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Administrador");
            }
        }
    }
    catch (Exception ex)
    {
        // Log the error but don't crash the application
        Console.WriteLine($"Erro na inicialização do banco de dados: {ex.Message}");
    }
}

// Classe pública para acesso em testes
public partial class Program { }

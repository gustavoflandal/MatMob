using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MatMob.Data;

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

builder.Services.AddControllersWithViews();

// Add custom services
builder.Services.AddScoped<MatMob.Services.IDashboardService, MatMob.Services.DashboardService>();
builder.Services.AddScoped<MatMob.Services.EstoqueService>();

// Add audit services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<MatMob.Services.IAuditService, MatMob.Services.AuditService>();

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
        // Criar tabela AuditLogs manualmente ANTES de qualquer migração
        await CreateAuditLogsTableManually(context);
        
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

// Método para criar a tabela AuditLogs manualmente
static async Task CreateAuditLogsTableManually(ApplicationDbContext context)
{
    try
    {
        // Verificar se a tabela já existe
        var tableExists = await context.Database.ExecuteSqlRawAsync(
            "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'AuditLogs'");

        if (tableExists == 0)
        {
            // Criar a tabela AuditLogs
            await context.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE AuditLogs (
                    Id int NOT NULL AUTO_INCREMENT,
                    UserId varchar(255) CHARACTER SET utf8mb4 NULL,
                    UserName varchar(255) CHARACTER SET utf8mb4 NULL,
                    IpAddress varchar(45) CHARACTER SET utf8mb4 NULL,
                    UserAgent varchar(500) CHARACTER SET utf8mb4 NULL,
                    Action varchar(50) CHARACTER SET utf8mb4 NOT NULL,
                    EntityName varchar(100) CHARACTER SET utf8mb4 NULL,
                    EntityId int NULL,
                    PropertyName varchar(100) CHARACTER SET utf8mb4 NULL,
                    OldValue TEXT CHARACTER SET utf8mb4 NULL,
                    NewValue TEXT CHARACTER SET utf8mb4 NULL,
                    OldData TEXT CHARACTER SET utf8mb4 NULL,
                    NewData TEXT CHARACTER SET utf8mb4 NULL,
                    Description varchar(1000) CHARACTER SET utf8mb4 NULL,
                    Context varchar(200) CHARACTER SET utf8mb4 NULL,
                    Severity varchar(20) CHARACTER SET utf8mb4 NOT NULL,
                    Category varchar(50) CHARACTER SET utf8mb4 NULL,
                    AdditionalData TEXT CHARACTER SET utf8mb4 NULL,
                    CreatedAt timestamp(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
                    Duration bigint NULL,
                    Success tinyint(1) NOT NULL,
                    ErrorMessage varchar(2000) CHARACTER SET utf8mb4 NULL,
                    StackTrace TEXT CHARACTER SET utf8mb4 NULL,
                    SessionId varchar(255) CHARACTER SET utf8mb4 NULL,
                    CorrelationId varchar(255) CHARACTER SET utf8mb4 NULL,
                    HttpMethod varchar(10) CHARACTER SET utf8mb4 NULL,
                    RequestUrl varchar(500) CHARACTER SET utf8mb4 NULL,
                    HttpStatusCode int NULL,
                    PermanentRetention tinyint(1) NOT NULL,
                    ExpirationDate datetime(6) NULL,
                    CONSTRAINT PK_AuditLogs PRIMARY KEY (Id)
                ) CHARACTER SET=utf8mb4;
            ");

            // Criar os índices
            await context.Database.ExecuteSqlRawAsync("CREATE INDEX IX_AuditLogs_Action ON AuditLogs (Action);");
            await context.Database.ExecuteSqlRawAsync("CREATE INDEX IX_AuditLogs_CorrelationId ON AuditLogs (CorrelationId);");
            await context.Database.ExecuteSqlRawAsync("CREATE INDEX IX_AuditLogs_EntityName ON AuditLogs (EntityName);");
            await context.Database.ExecuteSqlRawAsync("CREATE INDEX IX_AuditLogs_Timestamp ON AuditLogs (Timestamp);");
            await context.Database.ExecuteSqlRawAsync("CREATE INDEX IX_AuditLogs_UserId ON AuditLogs (UserId);");

            Console.WriteLine("Tabela AuditLogs criada com sucesso!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao criar tabela AuditLogs: {ex.Message}");
    }
}

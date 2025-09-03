using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MatMob.Data;

namespace MatMob.Controllers
{
    [AllowAnonymous]
    public class DiagnosticoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<DiagnosticoController> _logger;

        public DiagnosticoController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<DiagnosticoController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<IActionResult> Login()
        {
            var diagnostico = new DiagnosticoLoginViewModel();

            try
            {
                // Verificar conexão com banco
                diagnostico.ConexaoBanco = await _context.Database.CanConnectAsync();
                
                // Contar usuários e roles
                diagnostico.TotalUsuarios = await _context.Users.CountAsync();
                diagnostico.TotalRoles = await _context.Roles.CountAsync();

                // Listar todos os usuários
                var usuarios = await _context.Users.ToListAsync();
                diagnostico.Usuarios = usuarios.Select(u => new UsuarioInfo
                {
                    Id = u.Id,
                    Email = u.Email ?? "N/A",
                    UserName = u.UserName ?? "N/A",
                    EmailConfirmed = u.EmailConfirmed,
                    LockoutEnabled = u.LockoutEnabled,
                    AccessFailedCount = u.AccessFailedCount,
                    LockoutEnd = u.LockoutEnd?.ToString("dd/MM/yyyy HH:mm") ?? "N/A"
                }).ToList();

                // Listar roles
                var roles = await _context.Roles.ToListAsync();
                diagnostico.Roles = roles.Select(r => new RoleInfo
                {
                    Nome = r.Name ?? "N/A",
                    NormalizedName = r.NormalizedName ?? "N/A"
                }).ToList();

                // Verificar especificamente o usuário admin
                var adminEmail = "admin@matmob.com";
                var adminUser = await _userManager.FindByEmailAsync(adminEmail);
                
                if (adminUser != null)
                {
                    diagnostico.AdminEncontrado = true;
                    diagnostico.AdminInfo = new UsuarioInfo
                    {
                        Id = adminUser.Id,
                        Email = adminUser.Email ?? "N/A",
                        UserName = adminUser.UserName ?? "N/A",
                        EmailConfirmed = adminUser.EmailConfirmed,
                        LockoutEnabled = adminUser.LockoutEnabled,
                        AccessFailedCount = adminUser.AccessFailedCount,
                        LockoutEnd = adminUser.LockoutEnd?.ToString("dd/MM/yyyy HH:mm") ?? "N/A"
                    };

                    // Testar senha
                    diagnostico.SenhaCorreta = await _userManager.CheckPasswordAsync(adminUser, "Admin123!");

                    // Verificar roles do admin
                    var adminRoles = await _userManager.GetRolesAsync(adminUser);
                    diagnostico.AdminRoles = adminRoles.ToList();

                    // Testar login
                    var loginResult = await _signInManager.CheckPasswordSignInAsync(adminUser, "Admin123!", false);
                    diagnostico.LoginPossivel = loginResult.Succeeded;
                    diagnostico.LoginDetalhes = $"Succeeded: {loginResult.Succeeded}, IsLockedOut: {loginResult.IsLockedOut}, RequiresTwoFactor: {loginResult.RequiresTwoFactor}, IsNotAllowed: {loginResult.IsNotAllowed}";
                }
                else
                {
                    diagnostico.AdminEncontrado = false;
                    diagnostico.MensagemErro = "Usuário admin não foi encontrado no banco de dados.";
                }

            }
            catch (Exception ex)
            {
                diagnostico.ErroGeral = true;
                diagnostico.MensagemErro = ex.Message;
                _logger.LogError(ex, "Erro durante diagnóstico de login");
            }

            return View(diagnostico);
        }

        [HttpPost]
        public async Task<IActionResult> CriarAdmin()
        {
            try
            {
                var adminEmail = "admin@matmob.com";
                var existingUser = await _userManager.FindByEmailAsync(adminEmail);
                
                if (existingUser != null)
                {
                    TempData["DiagnosticoMessage"] = "Usuário admin já existe.";
                    return RedirectToAction("Login");
                }

                // Criar usuário admin
                var adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(adminUser, "Admin123!");
                
                if (result.Succeeded)
                {
                    // Verificar se a role existe
                    if (!await _roleManager.RoleExistsAsync("Administrador"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Administrador"));
                    }

                    // Adicionar role
                    await _userManager.AddToRoleAsync(adminUser, "Administrador");
                    
                    TempData["DiagnosticoMessage"] = "Usuário admin criado com sucesso!";
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    TempData["DiagnosticoMessage"] = $"Erro ao criar usuário: {errors}";
                }
            }
            catch (Exception ex)
            {
                TempData["DiagnosticoMessage"] = $"Erro: {ex.Message}";
            }

            return RedirectToAction("Login");
        }
    }

    public class DiagnosticoLoginViewModel
    {
        public bool ConexaoBanco { get; set; }
        public int TotalUsuarios { get; set; }
        public int TotalRoles { get; set; }
        public List<UsuarioInfo> Usuarios { get; set; } = new();
        public List<RoleInfo> Roles { get; set; } = new();
        public bool AdminEncontrado { get; set; }
        public UsuarioInfo? AdminInfo { get; set; }
        public bool SenhaCorreta { get; set; }
        public List<string> AdminRoles { get; set; } = new();
        public bool LoginPossivel { get; set; }
        public string LoginDetalhes { get; set; } = string.Empty;
        public bool ErroGeral { get; set; }
        public string MensagemErro { get; set; } = string.Empty;
    }

    public class UsuarioInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public string LockoutEnd { get; set; } = string.Empty;
    }

    public class RoleInfo
    {
        public string Nome { get; set; } = string.Empty;
        public string NormalizedName { get; set; } = string.Empty;
    }
}

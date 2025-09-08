using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MatMob.Models.ViewModels;
using MatMob.Models.Entities;

namespace MatMob.Controllers
{
    [AllowAnonymous]
    public class DiagnosticoLoginController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public DiagnosticoLoginController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            var diagnostico = new DiagnosticoLoginDetalhadoViewModel();

            try
            {
                // Verificar roles
                diagnostico.RolesExistem = new Dictionary<string, bool>();
                string[] roles = { "Administrador", "Gestor", "Tecnico" };
                foreach (var role in roles)
                {
                    diagnostico.RolesExistem[role] = await _roleManager.RoleExistsAsync(role);
                }

                // Verificar usuário admin
                var adminEmail = "admin@matmob.com";
                var adminUser = await _userManager.FindByEmailAsync(adminEmail);
                
                if (adminUser != null)
                {
                    diagnostico.UsuarioAdminExiste = true;
                    diagnostico.AdminEmailConfirmado = adminUser.EmailConfirmed;
                    diagnostico.AdminUserName = adminUser.UserName ?? string.Empty;
                    diagnostico.AdminId = adminUser.Id;
                    diagnostico.AdminLockoutEnabled = adminUser.LockoutEnabled;
                    diagnostico.AdminLockoutEnd = adminUser.LockoutEnd;
                    diagnostico.AdminAccessFailedCount = adminUser.AccessFailedCount;

                    // Verificar roles do usuário admin
                    var userRoles = await _userManager.GetRolesAsync(adminUser);
                    diagnostico.AdminRoles = userRoles?.ToList() ?? new List<string>();

                    // Verificar se a senha está correta
                    var senhaCorreta = await _userManager.CheckPasswordAsync(adminUser, "Admin123!");
                    diagnostico.SenhaAdminCorreta = senhaCorreta;

                    // Verificar se pode fazer login
                    var canSignIn = _signInManager.CanSignInAsync(adminUser);
                    diagnostico.PodeFazerLogin = await canSignIn;

                    // Verificar claims
                    var claims = await _userManager.GetClaimsAsync(adminUser);
                    diagnostico.AdminClaims = claims.Select(c => $"{c.Type}: {c.Value}").ToList();
                }
                else
                {
                    diagnostico.UsuarioAdminExiste = false;
                }

                // Verificar configurações do Identity
                diagnostico.RequireConfirmedEmail = _userManager.Options.SignIn.RequireConfirmedEmail;
                diagnostico.RequireConfirmedPhoneNumber = _userManager.Options.SignIn.RequireConfirmedPhoneNumber;
                diagnostico.RequireConfirmedAccount = _userManager.Options.SignIn.RequireConfirmedAccount;

                // Contar total de usuários
                diagnostico.TotalUsuarios = await _userManager.Users.CountAsync();
            }
            catch (Exception ex)
            {
                diagnostico.Erro = ex.Message;
            }

            return View(diagnostico);
        }

        [HttpPost]
        public async Task<IActionResult> TestarLogin(string email, string senha)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return Json(new { sucesso = false, erro = "Usuário não encontrado" });
                }

                var result = await _signInManager.PasswordSignInAsync(email, senha, false, false);
                
                var diagnostico = new
                {
                    sucesso = result.Succeeded,
                    requiresTwoFactor = result.RequiresTwoFactor,
                    isLockedOut = result.IsLockedOut,
                    isNotAllowed = result.IsNotAllowed,
                    emailConfirmed = user.EmailConfirmed,
                    lockoutEnabled = user.LockoutEnabled,
                    accessFailedCount = user.AccessFailedCount,
                    roles = await _userManager.GetRolesAsync(user),
                    canSignIn = await _signInManager.CanSignInAsync(user)
                };

                return Json(diagnostico);
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, erro = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RecriarUsuarioAdmin()
        {
            try
            {
                var adminEmail = "admin@matmob.com";
                
                // Remover usuário existente se houver
                var existingUser = await _userManager.FindByEmailAsync(adminEmail);
                if (existingUser != null)
                {
                    await _userManager.DeleteAsync(existingUser);
                }

                // Criar novo usuário
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "Administrador",
                    LastName = "Sistema"
                };

                var result = await _userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Administrador");
                    return Json(new { sucesso = true, mensagem = "Usuário administrador recriado com sucesso!" });
                }
                else
                {
                    return Json(new { sucesso = false, erro = string.Join(", ", result.Errors.Select(e => e.Description)) });
                }
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, erro = ex.Message });
            }
        }
    }


}

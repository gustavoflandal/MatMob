using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MatMob.Services;

namespace MatMob.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly IAuditService _auditService;

        public LogoutModel(SignInManager<IdentityUser> signInManager, ILogger<LogoutModel> logger, IAuditService auditService)
        {
            _signInManager = signInManager;
            _logger = logger;
            _auditService = auditService;
        }

        public async Task<IActionResult> OnGet()
        {
            // Capturar informações do usuário antes do logout para auditoria
            var userName = User.Identity?.Name ?? "Usuário desconhecido";
            
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            
            // Registrar auditoria de logout
            await _auditService.LogLogoutAsync(userName);
            
            return Page();
        }

        public async Task<IActionResult> OnPost(string? returnUrl = null)
        {
            // Capturar informações do usuário antes do logout para auditoria
            var userName = User.Identity?.Name ?? "Usuário desconhecido";
            
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            
            // Registrar auditoria de logout
            await _auditService.LogLogoutAsync(userName);
            
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return RedirectToPage();
            }
        }
    }
}


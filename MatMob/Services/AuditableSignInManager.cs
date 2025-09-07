using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MatMob.Services;

namespace MatMob.Services
{
    public class AuditableSignInManager<TUser> : SignInManager<TUser> where TUser : class
    {
        private readonly AuthenticationAuditService _authAuditService;

        public AuditableSignInManager(
            UserManager<TUser> userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<TUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<TUser>> logger,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<TUser> confirmation,
            AuthenticationAuditService authAuditService)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
            _authAuditService = authAuditService;
        }

        public override async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            var result = await base.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);
            
            try
            {
                var user = await UserManager.FindByNameAsync(userName);
                var userId = user != null ? await UserManager.GetUserIdAsync(user) : "Unknown";

                if (result.Succeeded)
                {
                    await _authAuditService.LogLoginSuccessAsync(userId, userName);
                }
                else
                {
                    string reason = "Credenciais inválidas";
                    if (result.IsLockedOut)
                        reason = "Conta bloqueada";
                    else if (result.IsNotAllowed)
                        reason = "Login não permitido";
                    else if (result.RequiresTwoFactor)
                        reason = "Requer autenticação de dois fatores";

                    await _authAuditService.LogLoginFailureAsync(userName, reason);

                    if (result.IsLockedOut && user != null)
                    {
                        await _authAuditService.LogAccountLockoutAsync(userId, userName);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the login process
                Logger.LogError(ex, "Erro ao registrar auditoria de login para usuário {UserName}", userName);
            }

            return result;
        }

        public override async Task SignOutAsync()
        {
            try
            {
                if (Context.User.Identity?.IsAuthenticated == true)
                {
                    var userName = Context.User.Identity.Name ?? "Unknown";
                    var userId = UserManager.GetUserId(Context.User) ?? "Unknown";
                    
                    await _authAuditService.LogLogoutAsync(userId, userName);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Erro ao registrar auditoria de logout");
            }

            await base.SignOutAsync();
        }
    }
}
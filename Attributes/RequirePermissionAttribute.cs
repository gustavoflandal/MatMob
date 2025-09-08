using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MatMob.Services;

namespace MatMob.Attributes
{
    /// <summary>
    /// Atributo para verificação de permissões específicas
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _permissionCode;
        private readonly bool _requireAll;

        /// <summary>
        /// Requer uma permissão específica
        /// </summary>
        /// <param name="permissionCode">Código da permissão requerida</param>
        public RequirePermissionAttribute(string permissionCode)
        {
            _permissionCode = permissionCode ?? throw new ArgumentNullException(nameof(permissionCode));
            _requireAll = false;
        }

        /// <summary>
        /// Requer múltiplas permissões
        /// </summary>
        /// <param name="permissionCodes">Códigos das permissões requeridas</param>
        /// <param name="requireAll">Se true, requer todas as permissões. Se false, requer pelo menos uma</param>
        public RequirePermissionAttribute(string[] permissionCodes, bool requireAll = true)
        {
            if (permissionCodes == null || permissionCodes.Length == 0)
                throw new ArgumentException("At least one permission code must be provided", nameof(permissionCodes));

            _permissionCode = string.Join(",", permissionCodes);
            _requireAll = requireAll;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Verificar se o usuário está autenticado
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var permissionService = context.HttpContext.RequestServices.GetService<IPermissionService>();
            if (permissionService == null)
            {
                context.Result = new StatusCodeResult(500); // Internal Server Error
                return;
            }

            var userId = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            try
            {
                var permissionCodes = _permissionCode.Split(',', StringSplitOptions.RemoveEmptyEntries);
                bool hasAccess = false;

                if (_requireAll)
                {
                    // Verificar se o usuário tem TODAS as permissões
                    hasAccess = true;
                    foreach (var code in permissionCodes)
                    {
                        if (!await permissionService.UserHasPermissionAsync(userId, code.Trim()))
                        {
                            hasAccess = false;
                            break;
                        }
                    }
                }
                else
                {
                    // Verificar se o usuário tem PELO MENOS UMA das permissões
                    foreach (var code in permissionCodes)
                    {
                        if (await permissionService.UserHasPermissionAsync(userId, code.Trim()))
                        {
                            hasAccess = true;
                            break;
                        }
                    }
                }

                if (!hasAccess)
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }
            catch (Exception)
            {
                // Em caso de erro, negar acesso por segurança
                context.Result = new StatusCodeResult(500);
                return;
            }
        }
    }

    /// <summary>
    /// Atributo para verificação de permissões com redirecionamento personalizado
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequirePermissionWithRedirectAttribute : RequirePermissionAttribute
    {
        private readonly string _redirectAction;
        private readonly string _redirectController;

        public RequirePermissionWithRedirectAttribute(string permissionCode, string redirectAction = "AccessDenied", string redirectController = "Home")
            : base(permissionCode)
        {
            _redirectAction = redirectAction;
            _redirectController = redirectController;
        }

        public new async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            await base.OnAuthorizationAsync(context);

            // Se o resultado foi ForbidResult, redirecionar para a página específica
            if (context.Result is ForbidResult)
            {
                context.Result = new RedirectToActionResult(_redirectAction, _redirectController, null);
            }
        }
    }
}

/// <summary>
/// Classe estática para facilitar o uso das permissões em atributos
/// </summary>
public static class PermissionCodes
{
    // Usuários
    public const string USERS_VIEW = "users.view";
    public const string USERS_CREATE = "users.create";
    public const string USERS_EDIT = "users.edit";
    public const string USERS_DELETE = "users.delete";
    public const string USERS_ACTIVATE = "users.activate";

    // Perfis
    public const string ROLES_VIEW = "roles.view";
    public const string ROLES_CREATE = "roles.create";
    public const string ROLES_EDIT = "roles.edit";
    public const string ROLES_DELETE = "roles.delete";

    // Permissões
    public const string PERMISSIONS_VIEW = "permissions.view";
    public const string PERMISSIONS_MANAGE = "permissions.manage";
    public const string PERMISSIONS_ASSIGN = "permissions.assign";

    // Ativos
    public const string ASSETS_VIEW = "assets.view";
    public const string ASSETS_CREATE = "assets.create";
    public const string ASSETS_EDIT = "assets.edit";
    public const string ASSETS_DELETE = "assets.delete";

    // Ordens de Serviço
    public const string WORKORDERS_VIEW = "workorders.view";
    public const string WORKORDERS_CREATE = "workorders.create";
    public const string WORKORDERS_EDIT = "workorders.edit";
    public const string WORKORDERS_DELETE = "workorders.delete";
    public const string WORKORDERS_APPROVE = "workorders.approve";
    public const string WORKORDERS_EXECUTE = "workorders.execute";

    // Técnicos
    public const string TECHNICIANS_VIEW = "technicians.view";
    public const string TECHNICIANS_CREATE = "technicians.create";
    public const string TECHNICIANS_EDIT = "technicians.edit";
    public const string TECHNICIANS_DELETE = "technicians.delete";

    // Equipes
    public const string TEAMS_VIEW = "teams.view";
    public const string TEAMS_CREATE = "teams.create";
    public const string TEAMS_EDIT = "teams.edit";
    public const string TEAMS_DELETE = "teams.delete";

    // Peças
    public const string PARTS_VIEW = "parts.view";
    public const string PARTS_CREATE = "parts.create";
    public const string PARTS_EDIT = "parts.edit";
    public const string PARTS_DELETE = "parts.delete";
    public const string PARTS_RESERVE = "parts.reserve";

    // Manutenção
    public const string MAINTENANCE_VIEW = "maintenance.view";
    public const string MAINTENANCE_CREATE = "maintenance.create";
    public const string MAINTENANCE_EDIT = "maintenance.edit";
    public const string MAINTENANCE_DELETE = "maintenance.delete";
    public const string MAINTENANCE_EXECUTE = "maintenance.execute";

    // Relatórios
    public const string REPORTS_VIEW = "reports.view";
    public const string REPORTS_EXPORT = "reports.export";
    public const string REPORTS_CREATE = "reports.create";

    // Dashboard
    public const string DASHBOARD_VIEW = "dashboard.view";
    public const string DASHBOARD_ADMIN = "dashboard.admin";

    // Auditoria
    public const string AUDIT_VIEW = "auditoria.view";
    public const string AUDIT_CONFIG = "auditoria.config";
    public const string AUDIT_INTEGRITY = "auditoria.integrity";

    // Sistema
    public const string SYSTEM_CONFIG = "system.config";
    public const string SYSTEM_MAINTENANCE = "system.maintenance";
}
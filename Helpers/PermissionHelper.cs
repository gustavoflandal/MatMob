using Microsoft.AspNetCore.Mvc.Rendering;
using MatMob.Services;
using System.Security.Claims;

namespace MatMob.Helpers
{
    /// <summary>
    /// Helper para verificação de permissões nas views
    /// </summary>
    public class PermissionHelper
    {
        private readonly IPermissionService _permissionService;

        public PermissionHelper(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        /// <summary>
        /// Verifica se o usuário atual tem uma permissão específica
        /// </summary>
        public async Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permissionCode)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return false;

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return false;

            return await _permissionService.UserHasPermissionAsync(userId, permissionCode);
        }

        /// <summary>
        /// Verifica se o usuário atual tem pelo menos uma das permissões especificadas
        /// </summary>
        public async Task<bool> HasAnyPermissionAsync(ClaimsPrincipal user, params string[] permissionCodes)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return false;

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return false;

            foreach (var code in permissionCodes)
            {
                if (await _permissionService.UserHasPermissionAsync(userId, code))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Verifica se o usuário atual tem todas as permissões especificadas
        /// </summary>
        public async Task<bool> HasAllPermissionsAsync(ClaimsPrincipal user, params string[] permissionCodes)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return false;

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return false;

            foreach (var code in permissionCodes)
            {
                if (!await _permissionService.UserHasPermissionAsync(userId, code))
                    return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Tag Helper para verificação de permissões em elementos HTML
    /// </summary>
    [HtmlTargetElement(Attributes = "asp-permission")]
    [HtmlTargetElement(Attributes = "asp-permissions")]
    [HtmlTargetElement(Attributes = "asp-require-all-permissions")]
    public class PermissionTagHelper : TagHelper
    {
        private readonly IPermissionService _permissionService;

        [HtmlAttributeName("asp-permission")]
        public string Permission { get; set; } = "";

        [HtmlAttributeName("asp-permissions")]
        public string Permissions { get; set; } = "";

        [HtmlAttributeName("asp-require-all-permissions")]
        public bool RequireAllPermissions { get; set; } = false;

        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public PermissionTagHelper(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var user = ViewContext.HttpContext.User;
            
            if (user?.Identity?.IsAuthenticated != true)
            {
                output.SuppressOutput();
                return;
            }

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                output.SuppressOutput();
                return;
            }

            bool hasPermission = false;

            try
            {
                if (!string.IsNullOrEmpty(Permission))
                {
                    hasPermission = await _permissionService.UserHasPermissionAsync(userId, Permission);
                }
                else if (!string.IsNullOrEmpty(Permissions))
                {
                    var permissionCodes = Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                   .Select(p => p.Trim())
                                                   .ToArray();

                    if (RequireAllPermissions)
                    {
                        hasPermission = true;
                        foreach (var code in permissionCodes)
                        {
                            if (!await _permissionService.UserHasPermissionAsync(userId, code))
                            {
                                hasPermission = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        foreach (var code in permissionCodes)
                        {
                            if (await _permissionService.UserHasPermissionAsync(userId, code))
                            {
                                hasPermission = true;
                                break;
                            }
                        }
                    }
                }
            }
            catch
            {
                hasPermission = false;
            }

            if (!hasPermission)
            {
                output.SuppressOutput();
            }
        }
    }

    /// <summary>
    /// Extension methods para IServiceCollection
    /// </summary>
    public static class PermissionHelperExtensions
    {
        public static IServiceCollection AddPermissionHelpers(this IServiceCollection services)
        {
            services.AddScoped<PermissionHelper>();
            services.AddScoped<PermissionTagHelper>();
            return services;
        }
    }
}
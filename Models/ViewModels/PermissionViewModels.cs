using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using MatMob.Models.Entities;

namespace MatMob.Models.ViewModels
{
    public class PermissionListViewModel
    {
        public Dictionary<string, List<Permission>> PermissionsByCategory { get; set; } = new();
        public List<string> Categories { get; set; } = new();
    }

    public class PermissionMatrixViewModel
    {
        public List<ApplicationRole> Roles { get; set; } = new();
        public List<Permission> Permissions { get; set; } = new();
        public Dictionary<string, List<Permission>> RolePermissions { get; set; } = new();
        public Dictionary<string, List<Permission>> PermissionsByCategory { get; set; } = new();
    }

    public class RoleManagementViewModel
    {
        public List<ApplicationRole> Roles { get; set; } = new();
        public List<Permission> AllPermissions { get; set; } = new();
        public Dictionary<string, List<Permission>> PermissionsByCategory { get; set; } = new();
    }

    public class CreateRoleViewModel
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [Display(Name = "Nome")]
        [StringLength(256)]
        public string Name { get; set; } = "";

        [Display(Name = "Descrição")]
        [StringLength(500)]
        public string Description { get; set; } = "";

        [Required(ErrorMessage = "Categoria é obrigatória")]
        [Display(Name = "Categoria")]
        [StringLength(100)]
        public string Category { get; set; } = "";

        [Display(Name = "Ativo")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Permissões")]
        public List<int> SelectedPermissionIds { get; set; } = new();

        public List<SelectListItem> AvailablePermissions { get; set; } = new();
        public Dictionary<string, List<Permission>> PermissionsByCategory { get; set; } = new();
    }

    public class EditRoleViewModel
    {
        [Required]
        public string Id { get; set; } = "";

        [Required(ErrorMessage = "Nome é obrigatório")]
        [Display(Name = "Nome")]
        [StringLength(256)]
        public string Name { get; set; } = "";

        [Display(Name = "Descrição")]
        [StringLength(500)]
        public string Description { get; set; } = "";

        [Required(ErrorMessage = "Categoria é obrigatória")]
        [Display(Name = "Categoria")]
        [StringLength(100)]
        public string Category { get; set; } = "";

        [Display(Name = "Ativo")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Permissões")]
        public List<int> SelectedPermissionIds { get; set; } = new();

        public List<SelectListItem> AvailablePermissions { get; set; } = new();
        public Dictionary<string, List<Permission>> PermissionsByCategory { get; set; } = new();
    }

    public class UserPermissionsViewModel
    {
        public ApplicationUser User { get; set; }
        public List<ApplicationRole> UserRoles { get; set; } = new();
        public List<Permission> DirectPermissions { get; set; } = new();
        public List<Permission> RolePermissions { get; set; } = new();
        public List<Permission> AllUserPermissions { get; set; } = new();
        public List<Permission> AllPermissions { get; set; } = new();
        public Dictionary<string, List<Permission>> PermissionsByCategory { get; set; } = new();
    }

    public class PermissionStatisticsViewModel
    {
        public int TotalPermissions { get; set; }
        public Dictionary<string, int> PermissionsByCategory { get; set; } = new();
        public int TotalRoles { get; set; }
        public int ActiveRoles { get; set; }
        public int TotalUsers { get; set; }
        public int UsersWithDirectPermissions { get; set; }
        public int UsersWithRolePermissions { get; set; }
        public Dictionary<string, int> RolePermissionCounts { get; set; } = new();
        public List<PermissionUsageInfo> MostUsedPermissions { get; set; } = new();
    }

    public class PermissionUsageInfo
    {
        public Permission Permission { get; set; }
        public int UsageCount { get; set; }
    }

    public class AssignPermissionViewModel
    {
        public string EntityId { get; set; } = ""; // Role ID ou User ID
        public string EntityType { get; set; } = ""; // "role" ou "user"
        public string EntityName { get; set; } = "";
        public List<int> SelectedPermissionIds { get; set; } = new();
        public List<Permission> AllPermissions { get; set; } = new();
        public Dictionary<string, List<Permission>> PermissionsByCategory { get; set; } = new();
    }

    public class PermissionAuditViewModel
    {
        public List<PermissionAuditEntry> AuditEntries { get; set; } = new();
        public string EntityFilter { get; set; } = "";
        public string ActionFilter { get; set; } = "";
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; } = 1;
    }

    public class PermissionAuditEntry
    {
        public int Id { get; set; }
        public string EntityType { get; set; } = "";
        public string EntityId { get; set; } = "";
        public string EntityName { get; set; } = "";
        public string Action { get; set; } = "";
        public string PermissionCode { get; set; } = "";
        public string PermissionName { get; set; } = "";
        public string PerformedBy { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; } = "";
        public string UserAgent { get; set; } = "";
    }

    public class BulkPermissionAssignmentViewModel
    {
        [Required(ErrorMessage = "Selecione pelo menos um usuário")]
        [Display(Name = "Usuários")]
        public List<string> SelectedUserIds { get; set; } = new();

        [Display(Name = "Roles para Adicionar")]
        public List<string> RolesToAdd { get; set; } = new();

        [Display(Name = "Roles para Remover")]
        public List<string> RolesToRemove { get; set; } = new();

        [Display(Name = "Permissões para Adicionar")]
        public List<int> PermissionsToAdd { get; set; } = new();

        [Display(Name = "Permissões para Remover")]
        public List<int> PermissionsToRemove { get; set; } = new();

        public List<SelectListItem> AvailableUsers { get; set; } = new();
        public List<SelectListItem> AvailableRoles { get; set; } = new();
        public List<SelectListItem> AvailablePermissions { get; set; } = new();
        public Dictionary<string, List<Permission>> PermissionsByCategory { get; set; } = new();
    }
}
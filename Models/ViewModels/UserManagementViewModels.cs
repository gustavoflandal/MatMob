using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using MatMob.Models.Entities;

namespace MatMob.Models.ViewModels
{
    public class UserManagementListViewModel
    {
        public List<ApplicationUser> Users { get; set; } = new();
        public string SearchTerm { get; set; } = "";
        public string RoleFilter { get; set; } = "";
        public bool? IsActiveFilter { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; } = 1;
        public List<SelectListItem> AvailableRoles { get; set; } = new();
    }

    public class UserDetailsViewModel
    {
        public ApplicationUser User { get; set; }
        public List<ApplicationRole> Roles { get; set; } = new();
        public List<Permission> DirectPermissions { get; set; } = new();
        public List<Permission> AllPermissions { get; set; } = new();
    }

    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Senha é obrigatória")]
        [StringLength(100, ErrorMessage = "A senha deve ter pelo menos {2} caracteres", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Senha")]
        [Compare("Password", ErrorMessage = "As senhas não coincidem")]
        public string ConfirmPassword { get; set; } = "";

        [Required(ErrorMessage = "Nome é obrigatório")]
        [Display(Name = "Nome")]
        [StringLength(100)]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "Sobrenome é obrigatório")]
        [Display(Name = "Sobrenome")]
        [StringLength(100)]
        public string LastName { get; set; } = "";

        [Display(Name = "Ativo")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Perfis")]
        public List<string> SelectedRoleIds { get; set; } = new();

        [Display(Name = "Permissões Diretas")]
        public List<int> SelectedPermissionIds { get; set; } = new();

        public List<SelectListItem> AvailableRoles { get; set; } = new();
        public List<SelectListItem> AvailablePermissions { get; set; } = new();
    }

    public class EditUserViewModel
    {
        [Required]
        public string Id { get; set; } = "";

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Nome é obrigatório")]
        [Display(Name = "Nome")]
        [StringLength(100)]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "Sobrenome é obrigatório")]
        [Display(Name = "Sobrenome")]
        [StringLength(100)]
        public string LastName { get; set; } = "";

        [Display(Name = "Ativo")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Perfis")]
        public List<string> SelectedRoleIds { get; set; } = new();

        [Display(Name = "Permissões Diretas")]
        public List<int> SelectedPermissionIds { get; set; } = new();

        public List<SelectListItem> AvailableRoles { get; set; } = new();
        public List<SelectListItem> AvailablePermissions { get; set; } = new();
    }

    public class ResetPasswordViewModel
    {
        [Required]
        public string UserId { get; set; } = "";

        [Required(ErrorMessage = "Nova senha é obrigatória")]
        [StringLength(100, ErrorMessage = "A senha deve ter pelo menos {2} caracteres", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nova Senha")]
        public string NewPassword { get; set; } = "";

        [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Nova Senha")]
        [Compare("NewPassword", ErrorMessage = "As senhas não coincidem")]
        public string ConfirmNewPassword { get; set; } = "";
    }

    public class UserStatisticsViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int UsersWithRoles { get; set; }
        public int UsersWithDirectPermissions { get; set; }
        public Dictionary<string, int> UsersByRole { get; set; } = new();
        public Dictionary<string, int> UsersByPermissionCategory { get; set; } = new();
        public List<RecentUserActivity> RecentActivities { get; set; } = new();
    }

    public class RecentUserActivity
    {
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Activity { get; set; } = "";
        public DateTime Date { get; set; }
        public string IpAddress { get; set; } = "";
    }
}
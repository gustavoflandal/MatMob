using System.ComponentModel.DataAnnotations;
using MatMob.Models.Entities;

namespace MatMob.Models.ViewModels
{
    public class PermissaoViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Nome")]
        public string Name { get; set; } = string.Empty;
        
        [Display(Name = "Descrição")]
        public string? Description { get; set; }
        
        [Display(Name = "Categoria")]
        public string? Category { get; set; }
        
        [Display(Name = "Ativa")]
        public bool IsActive { get; set; }
        
        [Display(Name = "Data de Criação")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? CreatedAt { get; set; }
        
        [Display(Name = "Status")]
        public string Status => IsActive ? "Ativa" : "Inativa";
    }

    public class RoleViewModel
    {
        public string Id { get; set; } = string.Empty;
        
        [Display(Name = "Nome")]
        public string Name { get; set; } = string.Empty;
        
        [Display(Name = "Descrição")]
        public string? Description { get; set; }
        
        [Display(Name = "Categoria")]
        public string? Category { get; set; }
        
        [Display(Name = "Ativa")]
        public bool IsActive { get; set; }
        
        [Display(Name = "Data de Criação")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? CreatedAt { get; set; }
        
        [Display(Name = "Status")]
        public string Status => IsActive ? "Ativa" : "Inativa";
    }

    public class RoleDetailsViewModel : RoleViewModel
    {
        [Display(Name = "Data de Atualização")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? UpdatedAt { get; set; }
        
        [Display(Name = "Permissões")]
        public List<PermissaoViewModel> Permissions { get; set; } = new List<PermissaoViewModel>();
        
        [Display(Name = "Quantidade de Permissões")]
        public int PermissionsCount => Permissions.Count;
        
        [Display(Name = "Permissões Ativas")]
        public int ActivePermissionsCount => Permissions.Count(p => p.IsActive);
    }

    public class PermissoesIndexViewModel
    {
        public List<PermissaoViewModel> Permissoes { get; set; } = new List<PermissaoViewModel>();
        public List<RoleViewModel> Roles { get; set; } = new List<RoleViewModel>();
        
        public int TotalPermissoes => Permissoes.Count;
        public int PermissoesAtivas => Permissoes.Count(p => p.IsActive);
        public int TotalRoles => Roles.Count;
        public int RolesAtivas => Roles.Count(r => r.IsActive);
    }

    public class CreatePermissaoViewModel
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        [Display(Name = "Nome")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        [Display(Name = "Descrição")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "A categoria é obrigatória")]
        [Display(Name = "Categoria")]
        public string Category { get; set; } = string.Empty;
        
        public List<string> CategoriasDisponiveis { get; set; } = new List<string>();
    }

    public class EditPermissaoViewModel : CreatePermissaoViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Ativa")]
        public bool IsActive { get; set; }
    }

    public class CreateRoleViewModel
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        [Display(Name = "Nome")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        [Display(Name = "Descrição")]
        public string? Description { get; set; }
        
        [Display(Name = "Ativa")]
        public bool IsActive { get; set; } = true;
        
        [Display(Name = "Permissões")]
        public List<int> SelectedPermissions { get; set; } = new List<int>();
        
        // Propriedade para armazenar permissões disponíveis agrupadas por categoria
        public Dictionary<string, List<Permission>> PermissoesDisponiveis { get; set; } = new Dictionary<string, List<Permission>>();
    }

    public class EditRoleViewModel : CreateRoleViewModel
    {
        public string Id { get; set; } = string.Empty;
    }

    public class UserRolesViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        
        [Display(Name = "Roles Atribuídas")]
        public List<string> AssignedRoles { get; set; } = new List<string>();
        
        [Display(Name = "Roles Disponíveis")]
        public List<RoleViewModel> AvailableRoles { get; set; } = new List<RoleViewModel>();
        
        [Display(Name = "Roles Selecionadas")]
        public List<string> SelectedRoles { get; set; } = new List<string>();
    }

    public class PermissionMatrixViewModel
    {
        public List<RoleDetailsViewModel> Roles { get; set; } = new List<RoleDetailsViewModel>();
        public List<PermissaoViewModel> Permissions { get; set; } = new List<PermissaoViewModel>();
        
        public Dictionary<string, Dictionary<string, bool>> Matrix { get; set; } = new Dictionary<string, Dictionary<string, bool>>();
        
        // Agrupa permissões por categoria
        public Dictionary<string, List<PermissaoViewModel>> PermissionsByCategory
        {
            get
            {
                return Permissions
                    .GroupBy(p => p.Category ?? "Geral")
                    .ToDictionary(g => g.Key, g => g.ToList());
            }
        }
        
        // Agrupa roles por categoria
        public Dictionary<string, List<RoleDetailsViewModel>> RolesByCategory
        {
            get
            {
                return Roles
                    .GroupBy(r => r.Category ?? "Geral")
                    .ToDictionary(g => g.Key, g => g.ToList());
            }
        }
    }

    // Alias para compatibilidade com as views
    public class CreatePermissionViewModel
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        [Display(Name = "Nome")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        [Display(Name = "Descrição")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "O módulo é obrigatório")]
        [Display(Name = "Módulo")]
        public string Module { get; set; } = string.Empty;
        
        [Display(Name = "Ativa")]
        public bool IsActive { get; set; } = true;
    }

    public class EditPermissionViewModel : CreatePermissionViewModel
    {
        public int Id { get; set; }
    }
}
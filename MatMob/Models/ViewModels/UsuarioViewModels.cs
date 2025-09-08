using System.ComponentModel.DataAnnotations;

namespace MatMob.Models.ViewModels
{
    public class UsuarioViewModel
    {
        public string Id { get; set; } = string.Empty;
        
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
        
        [Display(Name = "Nome de Usuário")]
        public string UserName { get; set; } = string.Empty;
        
        [Display(Name = "Nome")]
        public string? FirstName { get; set; }
        
        [Display(Name = "Sobrenome")]
        public string? LastName { get; set; }
        
        [Display(Name = "Nome Completo")]
        public string FullName => $"{FirstName} {LastName}".Trim();
        
        [Display(Name = "Ativo")]
        public bool IsActive { get; set; }
        
        [Display(Name = "Email Confirmado")]
        public bool EmailConfirmed { get; set; }
        
        [Display(Name = "Data de Criação")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? CreatedAt { get; set; }
        
        [Display(Name = "Último Login")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? LastLoginAt { get; set; }
        
        [Display(Name = "Roles")]
        public IList<string> Roles { get; set; } = new List<string>();
        
        [Display(Name = "Status")]
        public string Status => IsActive ? "Ativo" : "Inativo";
        
        [Display(Name = "Roles (texto)")]
        public string RolesText => string.Join(", ", Roles);
    }

    public class UsuarioDetailsViewModel : UsuarioViewModel
    {
        [Display(Name = "Telefone")]
        public string? PhoneNumber { get; set; }
        
        [Display(Name = "Autenticação de Dois Fatores")]
        public bool TwoFactorEnabled { get; set; }
        
        [Display(Name = "Bloqueio Habilitado")]
        public bool LockoutEnabled { get; set; }
        
        [Display(Name = "Tentativas de Acesso Falharam")]
        public int AccessFailedCount { get; set; }
        
        [Display(Name = "Data de Atualização")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? UpdatedAt { get; set; }
        
        [Display(Name = "IP do Último Login")]
        public string? LastLoginIp { get; set; }
        
        [Display(Name = "Tentativas de Login")]
        public int LoginAttempts { get; set; }
        
        [Display(Name = "Permissões")]
        public List<string> Permissions { get; set; } = new List<string>();
        
        [Display(Name = "Permissões (texto)")]
        public string PermissionsText => string.Join(", ", Permissions);
    }

    public class CreateUsuarioViewModel
    {
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "A senha é obrigatória")]
        [StringLength(100, ErrorMessage = "A senha deve ter pelo menos {2} caracteres", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "A confirmação de senha é obrigatória")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Senha")]
        [Compare("Password", ErrorMessage = "A senha e a confirmação não coincidem")]
        public string ConfirmPassword { get; set; } = string.Empty;
        
        [StringLength(100)]
        [Display(Name = "Nome")]
        public string? FirstName { get; set; }
        
        [StringLength(100)]
        [Display(Name = "Sobrenome")]
        public string? LastName { get; set; }
        
        [Display(Name = "Roles")]
        public List<string>? SelectedRoles { get; set; }
        
        public Dictionary<string, string> RolesDisponiveis { get; set; } = new Dictionary<string, string>();
    }

    public class EditUsuarioViewModel
    {
        public string Id { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
        
        [StringLength(100)]
        [Display(Name = "Nome")]
        public string? FirstName { get; set; }
        
        [StringLength(100)]
        [Display(Name = "Sobrenome")]
        public string? LastName { get; set; }
        
        [Phone]
        [Display(Name = "Telefone")]
        public string? PhoneNumber { get; set; }
        
        [Display(Name = "Ativo")]
        public bool IsActive { get; set; }
        
        [Display(Name = "Email Confirmado")]
        public bool EmailConfirmed { get; set; }
        
        [Display(Name = "Autenticação de Dois Fatores")]
        public bool TwoFactorEnabled { get; set; }
        
        [Display(Name = "Bloqueio Habilitado")]
        public bool LockoutEnabled { get; set; }
        
        [StringLength(100, ErrorMessage = "A nova senha deve ter pelo menos {2} caracteres", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nova Senha (deixe em branco para não alterar)")]
        public string? NewPassword { get; set; }
        
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Nova Senha")]
        [Compare("NewPassword", ErrorMessage = "A senha e a confirmação não coincidem")]
        public string? ConfirmNewPassword { get; set; }
        
        [Display(Name = "Roles")]
        public List<string>? SelectedRoles { get; set; }
        
        public Dictionary<string, string> RolesDisponiveis { get; set; } = new Dictionary<string, string>();
    }
}
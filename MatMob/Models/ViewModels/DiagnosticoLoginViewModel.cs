namespace MatMob.Models.ViewModels
{
    public class DiagnosticoLoginDetalhadoViewModel
    {
        public Dictionary<string, bool> RolesExistem { get; set; } = new();
        public bool UsuarioAdminExiste { get; set; }
        public bool AdminEmailConfirmado { get; set; }
        public string AdminUserName { get; set; } = string.Empty;
        public string AdminId { get; set; } = string.Empty;
        public bool AdminLockoutEnabled { get; set; }
        public DateTimeOffset? AdminLockoutEnd { get; set; }
        public int AdminAccessFailedCount { get; set; }
        public List<string> AdminRoles { get; set; } = new();
        public bool SenhaAdminCorreta { get; set; }
        public bool PodeFazerLogin { get; set; }
        public List<string> AdminClaims { get; set; } = new();
        public bool RequireConfirmedEmail { get; set; }
        public bool RequireConfirmedPhoneNumber { get; set; }
        public bool RequireConfirmedAccount { get; set; }
        public int TotalUsuarios { get; set; }
        public string Erro { get; set; } = string.Empty;
    }
}

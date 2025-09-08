using System;
using System.ComponentModel.DataAnnotations;

namespace MatMob.Models.Entities
{
    /// <summary>
    /// Relacionamento muitos-para-muitos entre Users e Permissions (permissões diretas)
    /// </summary>
    public class UserPermission
    {
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int PermissionId { get; set; }

        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string? GrantedBy { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? Reason { get; set; }

        public DateTime? ExpiresAt { get; set; }

        // Navegação
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Permission Permission { get; set; } = null!;
    }
}
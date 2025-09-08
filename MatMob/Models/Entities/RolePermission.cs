using System;
using System.ComponentModel.DataAnnotations;

namespace MatMob.Models.Entities
{
    /// <summary>
    /// Relacionamento muitos-para-muitos entre Roles e Permissions
    /// </summary>
    public class RolePermission
    {
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string RoleId { get; set; } = string.Empty;

        [Required]
        public int PermissionId { get; set; }

        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string? GrantedBy { get; set; }

        public bool IsActive { get; set; } = true;

        // Navegação
        public virtual ApplicationRole Role { get; set; } = null!;
        public virtual Permission Permission { get; set; } = null!;
    }
}
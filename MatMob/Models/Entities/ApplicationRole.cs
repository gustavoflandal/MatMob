using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace MatMob.Models.Entities
{
    /// <summary>
    /// Extensão da IdentityRole para incluir funcionalidades adicionais
    /// </summary>
    public class ApplicationRole : IdentityRole
    {
        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [StringLength(450)]
        public string? UpdatedBy { get; set; }

        // Navegação para relacionamentos
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

        public ApplicationRole() : base()
        {
        }

        public ApplicationRole(string roleName) : base(roleName)
        {
            Name = roleName;
        }
    }
}
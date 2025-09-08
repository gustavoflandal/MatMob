using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace MatMob.Models.Entities
{
    /// <summary>
    /// Extensão da IdentityUser para incluir funcionalidades adicionais
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [StringLength(200)]
        public string FullName => $"{FirstName} {LastName}".Trim();

        [StringLength(500)]
        public string? ProfilePicture { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [StringLength(450)]
        public string? UpdatedBy { get; set; }

        public DateTime? LastLoginAt { get; set; }

        [StringLength(45)]
        public string? LastLoginIp { get; set; }

        public int LoginAttempts { get; set; } = 0;

        public DateTime? LastFailedLoginAt { get; set; }

        // Navegação para relacionamentos
        public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();

        public ApplicationUser() : base()
        {
        }

        public ApplicationUser(string userName) : base(userName)
        {
            UserName = userName;
        }
    }
}
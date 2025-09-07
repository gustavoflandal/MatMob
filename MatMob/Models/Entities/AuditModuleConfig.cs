using System;
using System.ComponentModel.DataAnnotations;

namespace MatMob.Models.Entities
{
    public class AuditModuleConfig
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Module { get; set; } = string.Empty; // Ex.: AUTHENTICATION, DATA_MODIFICATION, REPORTING

        [MaxLength(100)]
        public string? Process { get; set; } // Ex.: LOGIN, CREATE, UPDATE, EXPORT

        public bool Enabled { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}

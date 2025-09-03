using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatMob.Models.Entities
{
    public class ApontamentoHoras
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrdemServicoId { get; set; }

        [Required]
        public int TecnicoId { get; set; }

        [Required]
        public DateTime DataInicio { get; set; }

        [Required]
        public DateTime DataFim { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal HorasTrabalhadas { get; set; }

        [StringLength(1000)]
        public string? DescricaoAtividade { get; set; }

        [StringLength(500)]
        public string? Observacoes { get; set; }

        public DateTime DataRegistro { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? UsuarioRegistro { get; set; }

        // Navigation Properties
        public virtual OrdemServico OrdemServico { get; set; } = null!;
        public virtual Tecnico Tecnico { get; set; } = null!;

        // Calculated Properties
        [NotMapped]
        public decimal HorasCalculadas => (decimal)(DataFim - DataInicio).TotalHours;
    }
}

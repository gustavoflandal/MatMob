using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatMob.Models.Entities
{
    public class MaterialOrdemServico
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrdemServicoId { get; set; }

        [Required]
        public int PecaId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal QuantidadeUtilizada { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PrecoUnitario { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorTotal { get; set; }

        [StringLength(500)]
        public string? Observacoes { get; set; }

        public DateTime DataUtilizacao { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? UsuarioRegistro { get; set; }

        // Navigation Properties
        public virtual OrdemServico OrdemServico { get; set; } = null!;
        public virtual Peca Peca { get; set; } = null!;
    }
}

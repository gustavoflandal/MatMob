using System.ComponentModel.DataAnnotations;

namespace MatMob.Models.Entities
{
    public enum StatusAgenda
    {
        Agendada = 1,
        EmAndamento = 2,
        Concluida = 3,
        Cancelada = 4,
        Reagendada = 5
    }

    public enum TipoAgenda
    {
        ManutencaoPreventiva = 1,
        ManutencaoCorretiva = 2,
        Inspecao = 3,
        Calibracao = 4,
        Teste = 5,
        OrdemServico = 6
    }

    public class AgendaManutencao
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Descricao { get; set; }

        [Required]
        public DateTime DataPrevista { get; set; }

        public DateTime? DataInicio { get; set; }

        public DateTime? DataFim { get; set; }

        [Required]
        public TipoAgenda Tipo { get; set; }

        [Required]
        public StatusAgenda Status { get; set; } = StatusAgenda.Agendada;

        public int? AtivoId { get; set; }

        public int? OrdemServicoId { get; set; }

        public int? EquipeId { get; set; }

        [Required]
        public int ResponsavelId { get; set; }

        [StringLength(500)]
        public string? Observacoes { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? UsuarioCriacao { get; set; }

        // Navigation Properties
        public virtual Ativo? Ativo { get; set; }
        public virtual OrdemServico? OrdemServico { get; set; }
        public virtual Equipe? Equipe { get; set; }
        public virtual Tecnico Responsavel { get; set; } = null!;
    }
}

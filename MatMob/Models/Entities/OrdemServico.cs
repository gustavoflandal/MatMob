using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MatMob.Models.Entities
{
    public class OrdemServico
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O número da OS é obrigatório")]
        [StringLength(20, ErrorMessage = "O número da OS deve ter no máximo 20 caracteres")]
        [Display(Name = "Número da OS")]
        public string NumeroOS { get; set; } = string.Empty;

        [Required(ErrorMessage = "O tipo de serviço é obrigatório")]
        public TipoServico TipoServico { get; set; }

        [Required(ErrorMessage = "A prioridade é obrigatória")]
        public PrioridadeOS Prioridade { get; set; } = PrioridadeOS.Media;

        [Required(ErrorMessage = "O status é obrigatório")]
        public StatusOS Status { get; set; } = StatusOS.Aberta;

        [Required(ErrorMessage = "A descrição do problema é obrigatória")]
        [StringLength(1000, ErrorMessage = "A descrição deve ter no máximo 1000 caracteres")]
        [Display(Name = "Descrição do Problema")]
        public string DescricaoProblema { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "A solução deve ter no máximo 1000 caracteres")]
        [Display(Name = "Solução Aplicada")]
        public string? SolucaoAplicada { get; set; }

        [Display(Name = "Data de Abertura")]
        public DateTime DataAbertura { get; set; } = DateTime.Now;

        [Display(Name = "Data de Início")]
        public DateTime? DataInicio { get; set; }

        [Display(Name = "Data de Conclusão")]
        public DateTime? DataConclusao { get; set; }

        [Display(Name = "Data Programada")]
        public DateTime? DataProgramada { get; set; }

        [Display(Name = "Hora Início Programada")]
        public TimeSpan? HoraInicioProgramada { get; set; }

        [Display(Name = "Hora Fim Programada")]
        public TimeSpan? HoraFimProgramada { get; set; }

        [Display(Name = "Tempo Gasto (horas)")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? TempoGastoHoras { get; set; }

        [Display(Name = "Custo Estimado")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? CustoEstimado { get; set; }

        [Display(Name = "Custo Real")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? CustoReal { get; set; }

        // Chaves estrangeiras
        [Required(ErrorMessage = "O ativo é obrigatório")]
        [Display(Name = "Ativo")]
        public int AtivoId { get; set; }

        [Display(Name = "Equipe Responsável")]
        public int? EquipeId { get; set; }

        [Display(Name = "Técnico Responsável")]
        public int? TecnicoResponsavelId { get; set; }

        // Relacionamentos
        [ForeignKey("AtivoId")]
        [BindNever]
        public virtual Ativo? Ativo { get; set; }

        [ForeignKey("EquipeId")]
        [BindNever]
        public virtual Equipe? Equipe { get; set; }

        [ForeignKey("TecnicoResponsavelId")]
        [BindNever]
        public virtual Tecnico? TecnicoResponsavel { get; set; }

        [BindNever]
        public virtual ICollection<ItemOrdemServico> ItensUtilizados { get; set; } = new List<ItemOrdemServico>();
        [BindNever]
        public virtual ICollection<MaterialOrdemServico> MateriaisUtilizados { get; set; } = new List<MaterialOrdemServico>();
        [BindNever]
        public virtual ICollection<ApontamentoHoras> ApontamentosHoras { get; set; } = new List<ApontamentoHoras>();
        [BindNever]
        public virtual ICollection<AgendaManutencao> AgendaItens { get; set; } = new List<AgendaManutencao>();
        
        [Display(Name = "Solicitante")]
        public int? SolicitanteId { get; set; }
        // Se houver entidade de usuário/solicitante, relacione aqui
        // public virtual Usuario? Solicitante { get; set; }
    }

    public enum TipoServico
    {
        [Display(Name = "Manutenção Preventiva")]
        ManutencaoPreventiva = 1,
        
        [Display(Name = "Manutenção Corretiva")]
        ManutencaoCorretiva = 2,
        
        [Display(Name = "Instalação")]
        Instalacao = 3,
        
        [Display(Name = "Inspeção")]
        Inspecao = 4,
        
        [Display(Name = "Reparo")]
        Reparo = 5
    }

    public enum StatusOS
    {
        [Display(Name = "Aberta")]
        Aberta = 1,
        
        [Display(Name = "Em Andamento")]
        EmAndamento = 2,
        
        [Display(Name = "Aguardando Peças")]
        AguardandoPecas = 3,
        
        [Display(Name = "Aguardando Aprovação")]
        AguardandoAprovacao = 6,
        
        [Display(Name = "Concluída")]
        Concluida = 4,
        
        [Display(Name = "Cancelada")]
        Cancelada = 5
    }

    public enum PrioridadeOS
    {
        [Display(Name = "Baixa")]
        Baixa = 1,
        
        [Display(Name = "Média")]
        Media = 2,
        
        [Display(Name = "Alta")]
        Alta = 3,
        
        [Display(Name = "Crítica")]
        Critica = 4
    }
}

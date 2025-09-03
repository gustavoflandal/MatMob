using System.ComponentModel.DataAnnotations;

namespace MatMob.Models.Entities
{
    public class PlanoManutencao
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do plano é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        [Display(Name = "Descrição")]
        public string? Descricao { get; set; }

        [Required(ErrorMessage = "A periodicidade é obrigatória")]
        public PeriodicidadeManutencao Periodicidade { get; set; }

        [Required(ErrorMessage = "O intervalo é obrigatório")]
        [Range(1, int.MaxValue, ErrorMessage = "O intervalo deve ser maior que zero")]
        [Display(Name = "Intervalo (dias)")]
        public int IntervaloDias { get; set; }

        [Display(Name = "Próxima Manutenção")]
        [DataType(DataType.Date)]
        public DateTime? ProximaManutencao { get; set; }

        [Display(Name = "Última Manutenção")]
        [DataType(DataType.Date)]
        public DateTime? UltimaManutencao { get; set; }

        public bool Habilitado { get; set; } = true;

        [Display(Name = "Data de Criação")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        // Chave estrangeira
        [Required]
        public int AtivoId { get; set; }

        // Relacionamentos
        public virtual Ativo Ativo { get; set; } = null!;
    }

    public enum PeriodicidadeManutencao
    {
        [Display(Name = "Diária")]
        Diaria = 1,
        
        [Display(Name = "Semanal")]
        Semanal = 2,
        
        [Display(Name = "Mensal")]
        Mensal = 3,
        
        [Display(Name = "Bimestral")]
        Bimestral = 4,
        
        [Display(Name = "Trimestral")]
        Trimestral = 5,
        
        [Display(Name = "Semestral")]
        Semestral = 6,
        
        [Display(Name = "Anual")]
        Anual = 7
    }
}

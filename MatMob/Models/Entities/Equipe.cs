using System.ComponentModel.DataAnnotations;

namespace MatMob.Models.Entities
{
    public class Equipe
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da equipe é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        [Display(Name = "Descrição")]
        public string? Descricao { get; set; }

        [Required]
        public StatusEquipe Status { get; set; } = StatusEquipe.Ativa;

        [Display(Name = "Data de Criação")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        // Relacionamentos
        public virtual ICollection<EquipeTecnico> EquipesTecnico { get; set; } = new List<EquipeTecnico>();
        public virtual ICollection<OrdemServico> OrdensServico { get; set; } = new List<OrdemServico>();
    }

    public class EquipeTecnico
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EquipeId { get; set; }

        [Required]
        public int TecnicoId { get; set; }

        [Display(Name = "Data de Entrada")]
        public DateTime DataEntrada { get; set; } = DateTime.Now;

        [Display(Name = "Data de Saída")]
        public DateTime? DataSaida { get; set; }

        public bool Ativo { get; set; } = true;

        // Relacionamentos
        public virtual Equipe Equipe { get; set; } = null!;
        public virtual Tecnico Tecnico { get; set; } = null!;
    }

    public enum StatusEquipe
    {
        [Display(Name = "Ativa")]
        Ativa = 1,
        
        [Display(Name = "Inativa")]
        Inativa = 2,
        
        [Display(Name = "Suspensa")]
        Suspensa = 3
    }
}

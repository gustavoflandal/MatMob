using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatMob.Models.Entities
{
    public class Ativo
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O tipo é obrigatório")]
        [StringLength(50, ErrorMessage = "O tipo deve ter no máximo 50 caracteres")]
        public string Tipo { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "O número de série deve ter no máximo 100 caracteres")]
        [Display(Name = "Número de Série")]
        public string? NumeroSerie { get; set; }

        [Required(ErrorMessage = "A localização é obrigatória")]
        [StringLength(200, ErrorMessage = "A localização deve ter no máximo 200 caracteres")]
        [Display(Name = "Localização")]
        public string Localizacao { get; set; } = string.Empty;

        [Display(Name = "Data de Instalação")]
        [DataType(DataType.Date)]
        public DateTime? DataInstalacao { get; set; }

        [Required]
        public StatusAtivo Status { get; set; } = StatusAtivo.Ativo;

        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        [Display(Name = "Descrição")]
        public string? Descricao { get; set; }

        [Display(Name = "Data de Cadastro")]
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        [Display(Name = "Última Atualização")]
        public DateTime? UltimaAtualizacao { get; set; }

        // Relacionamentos
        public virtual ICollection<OrdemServico> OrdensServico { get; set; } = new List<OrdemServico>();
        public virtual ICollection<PlanoManutencao> PlanosManutencao { get; set; } = new List<PlanoManutencao>();
    }

    public enum StatusAtivo
    {
        [Display(Name = "Ativo")]
        Ativo = 1,
        
        [Display(Name = "Em Manutenção")]
        EmManutencao = 2,
        
        [Display(Name = "Inativo")]
        Inativo = 3,
        
        [Display(Name = "Descartado")]
        Descartado = 4
    }
}

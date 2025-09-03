using System.ComponentModel.DataAnnotations;

namespace MatMob.Models.Entities
{
    public class Tecnico
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(100, ErrorMessage = "O email deve ter no máximo 100 caracteres")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres")]
        public string? Telefone { get; set; }

        [StringLength(20, ErrorMessage = "O celular deve ter no máximo 20 caracteres")]
        public string? Celular { get; set; }

        [Required(ErrorMessage = "A especialização é obrigatória")]
        [StringLength(100, ErrorMessage = "A especialização deve ter no máximo 100 caracteres")]
        [Display(Name = "Especialização")]
        public string Especializacao { get; set; } = string.Empty;

        [Display(Name = "Data de Contratação")]
        [DataType(DataType.Date)]
        public DateTime? DataContratacao { get; set; }

        // Novos campos
        [StringLength(14, ErrorMessage = "O CPF deve ter no máximo 14 caracteres")]
        public string? CPF { get; set; }

        [Display(Name = "Data de Nascimento")]
        [DataType(DataType.Date)]
        public DateTime? DataNascimento { get; set; }

        [StringLength(100, ErrorMessage = "A especialidade deve ter no máximo 100 caracteres")]
        [Display(Name = "Especialidade")]
        public string? Especialidade { get; set; }

        [Display(Name = "Data de Admissão")]
        [DataType(DataType.Date)]
        public DateTime? DataAdmissao { get; set; }

        [Required]
        public StatusTecnico Status { get; set; } = StatusTecnico.Ativo;

        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        [Display(Name = "Observações")]
        public string? Observacoes { get; set; }

        [Display(Name = "Data de Cadastro")]
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        // Relacionamentos
        public virtual ICollection<EquipeTecnico> EquipesTecnico { get; set; } = new List<EquipeTecnico>();
        public virtual ICollection<OrdemServico> OrdensServicoResponsavel { get; set; } = new List<OrdemServico>();
    }

    public enum StatusTecnico
    {
        [Display(Name = "Ativo")]
        Ativo = 1,
        
        [Display(Name = "Inativo")]
        Inativo = 2,
        
        [Display(Name = "Férias")]
        Ferias = 3,
        
        [Display(Name = "Licença")]
        Licenca = 4
    }
}

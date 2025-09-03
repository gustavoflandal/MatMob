using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatMob.Models.Entities
{
    public class Peca
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O código é obrigatório")]
        [StringLength(50, ErrorMessage = "O código deve ter no máximo 50 caracteres")]
        [Display(Name = "Código")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        [Display(Name = "Descrição")]
        public string? Descricao { get; set; }

        [StringLength(50, ErrorMessage = "A unidade de medida deve ter no máximo 50 caracteres")]
        [Display(Name = "Unidade de Medida")]
        public string? UnidadeMedida { get; set; }

        [Required(ErrorMessage = "A quantidade em estoque é obrigatória")]
        [Display(Name = "Quantidade em Estoque")]
        public int QuantidadeEstoque { get; set; }

        [Required(ErrorMessage = "O estoque mínimo é obrigatório")]
        [Display(Name = "Estoque Mínimo")]
        public int EstoqueMinimo { get; set; }

        [Display(Name = "Preço Unitário")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? PrecoUnitario { get; set; }

        [StringLength(100, ErrorMessage = "O fornecedor deve ter no máximo 100 caracteres")]
        public string? Fornecedor { get; set; }

        [Display(Name = "Data de Cadastro")]
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        [Display(Name = "Última Atualização")]
        public DateTime? UltimaAtualizacao { get; set; }

        public bool Ativa { get; set; } = true;

        // Relacionamentos
        public virtual ICollection<ItemOrdemServico> ItensOrdemServico { get; set; } = new List<ItemOrdemServico>();
        public virtual ICollection<MovimentacaoEstoque> MovimentacoesEstoque { get; set; } = new List<MovimentacaoEstoque>();

        // Propriedade calculada
        [NotMapped]
        [Display(Name = "Estoque Baixo")]
        public bool EstoqueBaixo => QuantidadeEstoque <= EstoqueMinimo;
    }

    public class ItemOrdemServico
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrdemServicoId { get; set; }

        [Required]
        public int PecaId { get; set; }

        [Required(ErrorMessage = "A quantidade é obrigatória")]
        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero")]
        public int Quantidade { get; set; }

        [Display(Name = "Preço Unitário")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? PrecoUnitario { get; set; }

        [Display(Name = "Preço Total")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? PrecoTotal { get; set; }

        // Relacionamentos
        public virtual OrdemServico OrdemServico { get; set; } = null!;
        public virtual Peca Peca { get; set; } = null!;
    }

    public class MovimentacaoEstoque
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PecaId { get; set; }

        [Required]
        public TipoMovimentacao TipoMovimentacao { get; set; }

        [Required(ErrorMessage = "A quantidade é obrigatória")]
        public int Quantidade { get; set; }

        [StringLength(200, ErrorMessage = "O motivo deve ter no máximo 200 caracteres")]
        public string? Motivo { get; set; }

        [Display(Name = "Data da Movimentação")]
        public DateTime DataMovimentacao { get; set; } = DateTime.Now;

        public int? OrdemServicoId { get; set; }

        // Relacionamentos
        public virtual Peca Peca { get; set; } = null!;
        public virtual OrdemServico? OrdemServico { get; set; }
    }

    public enum TipoMovimentacao
    {
        [Display(Name = "Entrada")]
        Entrada = 1,
        
        [Display(Name = "Saída")]
        Saida = 2,
        
        [Display(Name = "Ajuste")]
        Ajuste = 3
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MatMob.Models.Entities
{
    public class ProdutoFornecedor
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O produto é obrigatório")]
        [Display(Name = "Produto")]
        public int ProdutoId { get; set; }

        [Required(ErrorMessage = "O fornecedor é obrigatório")]
        [Display(Name = "Fornecedor")]
        public int FornecedorId { get; set; }

        [Required(ErrorMessage = "O preço é obrigatório")]
        [Display(Name = "Preço Unitário")]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero")]
        public decimal Preco { get; set; }

        [StringLength(50, ErrorMessage = "O código do fornecedor deve ter no máximo 50 caracteres")]
        [Display(Name = "Código do Fornecedor")]
        public string? CodigoFornecedor { get; set; }

        [Display(Name = "Quantidade por Embalagem")]
        [Range(0.01, double.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? QuantidadeEmbalagem { get; set; }

        [StringLength(50, ErrorMessage = "O modo de faturamento deve ter no máximo 50 caracteres")]
        [Display(Name = "Modo de Faturamento")]
        public string? ModoFaturamento { get; set; }

        [Required(ErrorMessage = "A data de atualização é obrigatória")]
        [Display(Name = "Data de Atualização")]
        [DataType(DataType.Date)]
        public DateTime DataAtualizacao { get; set; } = DateTime.Now;

        [Display(Name = "Data de Validade")]
        [DataType(DataType.Date)]
        public DateTime? DataValidade { get; set; }

        [StringLength(100, ErrorMessage = "A condição de pagamento deve ter no máximo 100 caracteres")]
        [Display(Name = "Condição de Pagamento")]
        public string? CondicaoPagamento { get; set; }

        [Display(Name = "Prazo de Entrega (dias)")]
        [Range(0, 365, ErrorMessage = "O prazo deve ser entre 0 e 365 dias")]
        public int? PrazoEntregaDias { get; set; }

        [Required(ErrorMessage = "O status é obrigatório")]
        [Display(Name = "Status")]
        public StatusProdutoFornecedor Status { get; set; } = StatusProdutoFornecedor.Ativo;

        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        [Display(Name = "Observações")]
        public string? Observacoes { get; set; }

        // Relacionamentos
        [ForeignKey("ProdutoId")]
        [BindNever]
        public virtual Produto? Produto { get; set; }

        [ForeignKey("FornecedorId")]
        [BindNever]
        public virtual Fornecedor? Fornecedor { get; set; }
    }

    public enum StatusProdutoFornecedor
    {
        [Display(Name = "Ativo")]
        Ativo = 1,

        [Display(Name = "Inativo")]
        Inativo = 2,

        [Display(Name = "Expirado")]
        Expirado = 3
    }
}
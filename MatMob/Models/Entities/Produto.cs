using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MatMob.Models.Entities
{
    public class Produto
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O código é obrigatório")]
        [StringLength(50, ErrorMessage = "O código deve ter no máximo 50 caracteres")]
        [Display(Name = "Código")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        [Display(Name = "Descrição")]
        public string? Descricao { get; set; }

        [Required(ErrorMessage = "A unidade de medida é obrigatória")]
        [StringLength(20, ErrorMessage = "A unidade de medida deve ter no máximo 20 caracteres")]
        [Display(Name = "Unidade de Medida")]
        public string UnidadeMedida { get; set; } = string.Empty;

        [Required(ErrorMessage = "O estoque mínimo é obrigatório")]
        [Display(Name = "Estoque Mínimo")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal EstoqueMinimo { get; set; }

        [Required(ErrorMessage = "O status é obrigatório")]
        [Display(Name = "Status")]
        public StatusProduto Status { get; set; } = StatusProduto.Ativo;

        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        [Display(Name = "Observações")]
        public string? Observacoes { get; set; }

        [Display(Name = "Data de Cadastro")]
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        [Display(Name = "Última Atualização")]
        public DateTime? UltimaAtualizacao { get; set; }

        [Display(Name = "Estoque Atual")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal EstoqueAtual { get; set; } = 0;

        // Relacionamentos
        [BindNever]
        public virtual ICollection<ProdutoFornecedor> Fornecedores { get; set; } = new List<ProdutoFornecedor>();

        [BindNever]
        public virtual ICollection<ItemPedidoCompra> ItensPedidoCompra { get; set; } = new List<ItemPedidoCompra>();

        [BindNever]
        public virtual ICollection<ItemNotaFiscal> ItensNotaFiscal { get; set; } = new List<ItemNotaFiscal>();
    }

    public enum StatusProduto
    {
        [Display(Name = "Ativo")]
        Ativo = 1,

        [Display(Name = "Inativo")]
        Inativo = 2,

        [Display(Name = "Descontinuado")]
        Descontinuado = 3
    }
}
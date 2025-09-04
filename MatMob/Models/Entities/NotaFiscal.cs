using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MatMob.Models.Entities
{
    public class NotaFiscal
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O número da nota fiscal é obrigatório")]
        [StringLength(50, ErrorMessage = "O número da nota fiscal deve ter no máximo 50 caracteres")]
        [Display(Name = "Número da NF")]
        public string NumeroNF { get; set; } = string.Empty;

        [Required(ErrorMessage = "A série é obrigatória")]
        [StringLength(10, ErrorMessage = "A série deve ter no máximo 10 caracteres")]
        [Display(Name = "Série")]
        public string Serie { get; set; } = string.Empty;

        [Required(ErrorMessage = "O pedido de compra é obrigatório")]
        [Display(Name = "Pedido de Compra")]
        public int PedidoCompraId { get; set; }

        [Required(ErrorMessage = "A data de emissão é obrigatória")]
        [Display(Name = "Data de Emissão")]
        [DataType(DataType.Date)]
        public DateTime DataEmissao { get; set; }

        [Required(ErrorMessage = "A data de entrada é obrigatória")]
        [Display(Name = "Data de Entrada")]
        public DateTime DataEntrada { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "O fornecedor é obrigatório")]
        [Display(Name = "Fornecedor")]
        public int FornecedorId { get; set; }

        [Display(Name = "Valor Total dos Produtos")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? ValorProdutos { get; set; }

        [Display(Name = "Valor do ICMS")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? ValorICMS { get; set; }

        [Display(Name = "Valor do IPI")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? ValorIPI { get; set; }

        [Display(Name = "Valor do PIS")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? ValorPIS { get; set; }

        [Display(Name = "Valor do COFINS")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? ValorCOFINS { get; set; }

        [Display(Name = "Valor Total da NF")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? ValorTotal { get; set; }

        [StringLength(100, ErrorMessage = "A chave de acesso deve ter no máximo 100 caracteres")]
        [Display(Name = "Chave de Acesso")]
        public string? ChaveAcesso { get; set; }

        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        [Display(Name = "Observações")]
        public string? Observacoes { get; set; }

        [Display(Name = "Data de Cadastro")]
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        // Relacionamentos
        [ForeignKey("PedidoCompraId")]
        [BindNever]
        public virtual PedidoCompra? PedidoCompra { get; set; }

        [ForeignKey("FornecedorId")]
        [BindNever]
        public virtual Fornecedor? Fornecedor { get; set; }

        [BindNever]
        public virtual ICollection<ItemNotaFiscal> Itens { get; set; } = new List<ItemNotaFiscal>();
    }
}
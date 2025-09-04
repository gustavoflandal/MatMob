using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MatMob.Models.Entities
{
    public class ItemNotaFiscal
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "A nota fiscal é obrigatória")]
        [Display(Name = "Nota Fiscal")]
        public int NotaFiscalId { get; set; }

        [Required(ErrorMessage = "O produto é obrigatório")]
        [Display(Name = "Produto")]
        public int ProdutoId { get; set; }

        [Required(ErrorMessage = "O item do pedido é obrigatório")]
        [Display(Name = "Item do Pedido")]
        public int ItemPedidoCompraId { get; set; }

        [Required(ErrorMessage = "A quantidade recebida é obrigatória")]
        [Display(Name = "Quantidade Recebida")]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero")]
        public decimal QuantidadeRecebida { get; set; }

        [Required(ErrorMessage = "O preço unitário é obrigatório")]
        [Display(Name = "Preço Unitário")]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "O preço deve ser maior ou igual a zero")]
        public decimal PrecoUnitario { get; set; }

        [Display(Name = "Valor Total")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal ValorTotal => QuantidadeRecebida * PrecoUnitario;

        [Display(Name = "Valor ICMS")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? ValorICMS { get; set; }

        [Display(Name = "Valor IPI")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? ValorIPI { get; set; }

        [Display(Name = "Valor PIS")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? ValorPIS { get; set; }

        [Display(Name = "Valor COFINS")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? ValorCOFINS { get; set; }

        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        [Display(Name = "Observações")]
        public string? Observacoes { get; set; }

        // Relacionamentos
        [ForeignKey("NotaFiscalId")]
        [BindNever]
        public virtual NotaFiscal? NotaFiscal { get; set; }

        [ForeignKey("ProdutoId")]
        [BindNever]
        public virtual Produto? Produto { get; set; }

        [ForeignKey("ItemPedidoCompraId")]
        [BindNever]
        public virtual ItemPedidoCompra? ItemPedidoCompra { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MatMob.Models.Entities
{
    public class ItemPedidoCompra
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O pedido de compra é obrigatório")]
        [Display(Name = "Pedido de Compra")]
        public int PedidoCompraId { get; set; }

        [Required(ErrorMessage = "O produto é obrigatório")]
        [Display(Name = "Produto")]
        public int ProdutoId { get; set; }

        [Required(ErrorMessage = "A quantidade solicitada é obrigatória")]
        [Display(Name = "Quantidade Solicitada")]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero")]
        public decimal QuantidadeSolicitada { get; set; }

        [Display(Name = "Quantidade Recebida")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal QuantidadeRecebida { get; set; } = 0;

        [Required(ErrorMessage = "O preço unitário é obrigatório")]
        [Display(Name = "Preço Unitário")]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero")]
        public decimal PrecoUnitario { get; set; }

        [Display(Name = "Valor Total")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal ValorTotal => QuantidadeSolicitada * PrecoUnitario;

        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        [Display(Name = "Observações")]
        public string? Observacoes { get; set; }

        // Relacionamentos
        [ForeignKey("PedidoCompraId")]
        [BindNever]
        public virtual PedidoCompra? PedidoCompra { get; set; }

        [ForeignKey("ProdutoId")]
        [BindNever]
        public virtual Produto? Produto { get; set; }

        [BindNever]
        public virtual ICollection<ItemNotaFiscal> ItensNotaFiscal { get; set; } = new List<ItemNotaFiscal>();
    }
}
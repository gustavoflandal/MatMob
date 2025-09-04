using System.ComponentModel.DataAnnotations;
using MatMob.Models.Entities;

namespace MatMob.ViewModels
{
    public class PedidoCompraViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O número do pedido é obrigatório")]
        [StringLength(20, ErrorMessage = "O número do pedido deve ter no máximo 20 caracteres")]
        [Display(Name = "Número do Pedido")]
        public string NumeroPedido { get; set; } = string.Empty;

        [Required(ErrorMessage = "O fornecedor é obrigatório")]
        [Display(Name = "Fornecedor")]
        public int FornecedorId { get; set; }

        [Required(ErrorMessage = "O status é obrigatório")]
        [Display(Name = "Status")]
        public StatusPedidoCompra Status { get; set; } = StatusPedidoCompra.Aberto;

        [Required(ErrorMessage = "A prioridade é obrigatória")]
        [Display(Name = "Prioridade")]
        public PrioridadePedidoCompra Prioridade { get; set; } = PrioridadePedidoCompra.Media;

        [Display(Name = "Data do Pedido")]
        public DateTime DataPedido { get; set; } = DateTime.Now;

        [Display(Name = "Data Prevista de Entrega")]
        [DataType(DataType.Date)]
        public DateTime? DataPrevistaEntrega { get; set; }

        [StringLength(100, ErrorMessage = "A condição de pagamento deve ter no máximo 100 caracteres")]
        [Display(Name = "Condição de Pagamento")]
        public string? CondicaoPagamento { get; set; }

        [Display(Name = "Valor Total")]
        public decimal? ValorTotal { get; set; }

        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        [Display(Name = "Observações")]
        public string? Observacoes { get; set; }

        [Display(Name = "Data de Cadastro")]
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        [Display(Name = "Última Atualização")]
        public DateTime? UltimaAtualizacao { get; set; }

        // Itens do pedido
        public List<ItemPedidoCompraViewModel> Itens { get; set; } = new List<ItemPedidoCompraViewModel>();
    }

    public class ItemPedidoCompraViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O produto é obrigatório")]
        [Display(Name = "Produto")]
        public int ProdutoId { get; set; }

        [Required(ErrorMessage = "A quantidade solicitada é obrigatória")]
        [Display(Name = "Quantidade Solicitada")]
        public decimal QuantidadeSolicitada { get; set; }

        [Required(ErrorMessage = "O preço unitário é obrigatório")]
        [Display(Name = "Preço Unitário")]
        public decimal PrecoUnitario { get; set; }

        [Display(Name = "Valor Total")]
        public decimal ValorTotal => QuantidadeSolicitada * PrecoUnitario;

        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        [Display(Name = "Observações")]
        public string? Observacoes { get; set; }
    }
}
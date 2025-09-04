using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MatMob.Models.Entities;

namespace MatMob.Models.ViewModels
{
    public class PedidoCompraViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O fornecedor é obrigatório")]
        [Display(Name = "Fornecedor")]
        public int FornecedorId { get; set; }

        [Display(Name = "Número do Pedido")]
        public string? NumeroPedido { get; set; }

        [Required(ErrorMessage = "A prioridade é obrigatória")]
        [Display(Name = "Prioridade")]
        public PrioridadePedidoCompra Prioridade { get; set; } = PrioridadePedidoCompra.Media;

        [Display(Name = "Data Prevista de Entrega")]
        [DataType(DataType.Date)]
        public DateTime? DataPrevistaEntrega { get; set; }

        [Display(Name = "Condição de Pagamento")]
        [StringLength(100, ErrorMessage = "A condição de pagamento deve ter no máximo 100 caracteres")]
        public string? CondicaoPagamento { get; set; }

        [Display(Name = "Observações")]
        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        public string? Observacoes { get; set; }

        // Propriedades de navegação
        public Fornecedor? Fornecedor { get; set; }

        // Itens do pedido
        public List<ItemPedidoCompraViewModel> Itens { get; set; } = new List<ItemPedidoCompraViewModel>();

        // Propriedades calculadas
        [Display(Name = "Valor Total")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal ValorTotal { get; set; }

        [Display(Name = "Data do Pedido")]
        [DataType(DataType.DateTime)]
        public DateTime DataPedido { get; set; }

        [Display(Name = "Data de Aprovação")]
        [DataType(DataType.DateTime)]
        public DateTime? DataAprovacao { get; set; }

        [Display(Name = "Data de Entrega")]
        [DataType(DataType.DateTime)]
        public DateTime? DataEntrega { get; set; }

        [Display(Name = "Data de Cadastro")]
        [DataType(DataType.DateTime)]
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        [Display(Name = "Status")]
        public StatusPedidoCompra Status { get; set; } = StatusPedidoCompra.Aberto;

        [Display(Name = "Última Atualização")]
        [DataType(DataType.DateTime)]
        public DateTime? UltimaAtualizacao { get; set; }
    }

    public class ItemPedidoCompraViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O produto é obrigatório")]
        [Display(Name = "Produto")]
        public int ProdutoId { get; set; }

        [Required(ErrorMessage = "A quantidade solicitada é obrigatória")]
        [Display(Name = "Quantidade Solicitada")]
        [Range(0.01, double.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero")]
        public decimal QuantidadeSolicitada { get; set; }

        [Required(ErrorMessage = "O preço unitário é obrigatório")]
        [Display(Name = "Preço Unitário")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal PrecoUnitario { get; set; }

        [Display(Name = "Observações")]
        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        public string? Observacoes { get; set; }

        // Propriedades de navegação
        public Produto? Produto { get; set; }

        // Propriedades calculadas
        [Display(Name = "Valor Total")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal ValorTotal => QuantidadeSolicitada * PrecoUnitario;
    }
}
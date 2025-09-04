using System.ComponentModel.DataAnnotations;
using MatMob.Models.Entities;

namespace MatMob.ViewModels
{
    public class NotaFiscalViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O número da NF é obrigatório")]
        [Display(Name = "Número da NF")]
        public string NumeroNF { get; set; }

        [Required(ErrorMessage = "A série é obrigatória")]
        [Display(Name = "Série")]
        public string Serie { get; set; }

        [Required(ErrorMessage = "A data de emissão é obrigatória")]
        [Display(Name = "Data de Emissão")]
        [DataType(DataType.Date)]
        public DateTime DataEmissao { get; set; }

        [Display(Name = "Chave de Acesso")]
        public string ChaveAcesso { get; set; }

        [Display(Name = "Pedido de Compra")]
        public int? PedidoCompraId { get; set; }

        [Required(ErrorMessage = "O fornecedor é obrigatório")]
        [Display(Name = "Fornecedor")]
        public int FornecedorId { get; set; }

        [Required(ErrorMessage = "O valor dos produtos é obrigatório")]
        [Display(Name = "Valor dos Produtos")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
        public decimal ValorProdutos { get; set; }

        [Display(Name = "Valor ICMS")]
        [Range(0, double.MaxValue, ErrorMessage = "O valor não pode ser negativo")]
        public decimal ValorICMS { get; set; }

        [Display(Name = "Valor IPI")]
        [Range(0, double.MaxValue, ErrorMessage = "O valor não pode ser negativo")]
        public decimal ValorIPI { get; set; }

        [Display(Name = "Valor PIS")]
        [Range(0, double.MaxValue, ErrorMessage = "O valor não pode ser negativo")]
        public decimal ValorPIS { get; set; }

        [Display(Name = "Valor COFINS")]
        [Range(0, double.MaxValue, ErrorMessage = "O valor não pode ser negativo")]
        public decimal ValorCOFINS { get; set; }

        [Display(Name = "Observações")]
        public string Observacoes { get; set; }

        public List<ItemNotaFiscalViewModel> Itens { get; set; } = new List<ItemNotaFiscalViewModel>();

        // Propriedades de navegação para exibição
        public PedidoCompra PedidoCompra { get; set; }
        public Fornecedor Fornecedor { get; set; }

        // Propriedades calculadas
        public decimal ValorTotal => ValorProdutos + ValorICMS + ValorIPI + ValorPIS + ValorCOFINS;
        public int QuantidadeItens => Itens?.Count ?? 0;
    }

    public class ItemNotaFiscalViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O produto é obrigatório")]
        [Display(Name = "Produto")]
        public int ProdutoId { get; set; }

        public int ItemPedidoCompraId { get; set; }

        [Required(ErrorMessage = "A quantidade é obrigatória")]
        [Display(Name = "Quantidade")]
        [Range(0.01, double.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero")]
        public decimal Quantidade { get; set; }

        [Required(ErrorMessage = "O valor unitário é obrigatório")]
        [Display(Name = "Valor Unitário")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor unitário deve ser maior que zero")]
        public decimal ValorUnitario { get; set; }

        [Display(Name = "Observações")]
        public string Observacoes { get; set; }

        // Propriedades calculadas
        public decimal ValorTotal => Quantidade * ValorUnitario;

        // Propriedades de navegação para exibição
        public Produto Produto { get; set; }
    }
}
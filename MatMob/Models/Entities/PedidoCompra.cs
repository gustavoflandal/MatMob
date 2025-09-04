using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MatMob.Models.Entities
{
    public class PedidoCompra
    {
        [Key]
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

        [Display(Name = "Data de Aprovação")]
        public DateTime? DataAprovacao { get; set; }

        [Display(Name = "Data Prevista de Entrega")]
        [DataType(DataType.Date)]
        public DateTime? DataPrevistaEntrega { get; set; }

        [Display(Name = "Data de Entrega")]
        public DateTime? DataEntrega { get; set; }

        [StringLength(100, ErrorMessage = "A condição de pagamento deve ter no máximo 100 caracteres")]
        [Display(Name = "Condição de Pagamento")]
        public string? CondicaoPagamento { get; set; }

        [Display(Name = "Valor Total")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? ValorTotal { get; set; }

        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        [Display(Name = "Observações")]
        public string? Observacoes { get; set; }

        [Display(Name = "Data de Cadastro")]
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        [Display(Name = "Última Atualização")]
        public DateTime? UltimaAtualizacao { get; set; }

        // Relacionamentos
        [ForeignKey("FornecedorId")]
        [BindNever]
        public virtual Fornecedor? Fornecedor { get; set; }

        [BindNever]
        public virtual ICollection<ItemPedidoCompra> Itens { get; set; } = new List<ItemPedidoCompra>();

        [BindNever]
        public virtual ICollection<NotaFiscal> NotasFiscais { get; set; } = new List<NotaFiscal>();
    }

    public enum StatusPedidoCompra
    {
        [Display(Name = "Aberto")]
        Aberto = 1,

        [Display(Name = "Aprovado")]
        Aprovado = 2,

        [Display(Name = "Rejeitado")]
        Rejeitado = 3,

        [Display(Name = "Parcialmente Recebido")]
        ParcialmenteRecebido = 4,

        [Display(Name = "Totalmente Recebido")]
        TotalmenteRecebido = 5,

        [Display(Name = "Cancelado")]
        Cancelado = 6
    }

    public enum PrioridadePedidoCompra
    {
        [Display(Name = "Baixa")]
        Baixa = 1,

        [Display(Name = "Média")]
        Media = 2,

        [Display(Name = "Alta")]
        Alta = 3,

        [Display(Name = "Urgente")]
        Urgente = 4
    }
}
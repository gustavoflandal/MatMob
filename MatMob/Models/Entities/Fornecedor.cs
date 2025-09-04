using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MatMob.Models.Entities
{
    public class Fornecedor
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        [Display(Name = "Nome/Razão Social")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "O nome fantasia deve ter no máximo 100 caracteres")]
        [Display(Name = "Nome Fantasia")]
        public string? NomeFantasia { get; set; }

        [Required(ErrorMessage = "O CNPJ é obrigatório")]
        [StringLength(18, ErrorMessage = "O CNPJ deve ter no máximo 18 caracteres")]
        [Display(Name = "CNPJ")]
        public string CNPJ { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "A inscrição estadual deve ter no máximo 20 caracteres")]
        [Display(Name = "Inscrição Estadual")]
        public string? InscricaoEstadual { get; set; }

        [Required(ErrorMessage = "O endereço é obrigatório")]
        [StringLength(200, ErrorMessage = "O endereço deve ter no máximo 200 caracteres")]
        [Display(Name = "Endereço")]
        public string Endereco { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "O bairro deve ter no máximo 100 caracteres")]
        [Display(Name = "Bairro")]
        public string? Bairro { get; set; }

        [Required(ErrorMessage = "A cidade é obrigatória")]
        [StringLength(100, ErrorMessage = "A cidade deve ter no máximo 100 caracteres")]
        [Display(Name = "Cidade")]
        public string Cidade { get; set; } = string.Empty;

        [Required(ErrorMessage = "O estado é obrigatório")]
        [StringLength(2, ErrorMessage = "O estado deve ter 2 caracteres")]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = string.Empty;

        [Required(ErrorMessage = "O CEP é obrigatório")]
        [StringLength(9, ErrorMessage = "O CEP deve ter no máximo 9 caracteres")]
        [Display(Name = "CEP")]
        public string CEP { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres")]
        [Display(Name = "Telefone")]
        public string? Telefone { get; set; }

        [StringLength(20, ErrorMessage = "O celular deve ter no máximo 20 caracteres")]
        [Display(Name = "Celular")]
        public string? Celular { get; set; }

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(100, ErrorMessage = "O email deve ter no máximo 100 caracteres")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "O nome do contato deve ter no máximo 100 caracteres")]
        [Display(Name = "Nome do Contato")]
        public string? NomeContato { get; set; }

        [Required(ErrorMessage = "O status é obrigatório")]
        [Display(Name = "Status")]
        public StatusFornecedor Status { get; set; } = StatusFornecedor.Ativo;

        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        [Display(Name = "Observações")]
        public string? Observacoes { get; set; }

        [Display(Name = "Data de Cadastro")]
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        [Display(Name = "Última Atualização")]
        public DateTime? UltimaAtualizacao { get; set; }

        // Relacionamentos
        [BindNever]
        public virtual ICollection<ProdutoFornecedor> ProdutosFornecidos { get; set; } = new List<ProdutoFornecedor>();

        [BindNever]
        public virtual ICollection<PedidoCompra> PedidosCompra { get; set; } = new List<PedidoCompra>();
    }

    public enum StatusFornecedor
    {
        [Display(Name = "Ativo")]
        Ativo = 1,

        [Display(Name = "Inativo")]
        Inativo = 2,

        [Display(Name = "Bloqueado")]
        Bloqueado = 3
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatMob.Models.Entities
{
    /// <summary>
    /// Entidade para armazenar logs de auditoria do sistema
    /// </summary>
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// ID do usuário que executou a ação
        /// </summary>
        [MaxLength(255)]
        public string? UserId { get; set; }

        /// <summary>
        /// Nome do usuário que executou a ação
        /// </summary>
        [MaxLength(255)]
        public string? UserName { get; set; }

        /// <summary>
        /// Endereço IP do usuário
        /// </summary>
        [MaxLength(45)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// User Agent do navegador
        /// </summary>
        [MaxLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Tipo de ação executada (CREATE, UPDATE, DELETE, LOGIN, LOGOUT, etc.)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Nome da entidade afetada (PedidoCompra, Ativo, Tecnico, etc.)
        /// </summary>
        [MaxLength(100)]
        public string? EntityName { get; set; }

        /// <summary>
        /// ID da entidade afetada
        /// </summary>
        public int? EntityId { get; set; }

        /// <summary>
        /// Nome da propriedade alterada (para operações de UPDATE)
        /// </summary>
        [MaxLength(100)]
        public string? PropertyName { get; set; }

        /// <summary>
        /// Valor antigo da propriedade (serializado em JSON)
        /// </summary>
        [Column(TypeName = "TEXT")]
        public string? OldValue { get; set; }

        /// <summary>
        /// Novo valor da propriedade (serializado em JSON)
        /// </summary>
        [Column(TypeName = "TEXT")]
        public string? NewValue { get; set; }

        /// <summary>
        /// Dados completos da entidade antes da alteração (JSON)
        /// </summary>
        [Column(TypeName = "TEXT")]
        public string? OldData { get; set; }

        /// <summary>
        /// Dados completos da entidade após a alteração (JSON)
        /// </summary>
        [Column(TypeName = "TEXT")]
        public string? NewData { get; set; }

        /// <summary>
        /// Descrição adicional da operação
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Contexto adicional da operação (controller, action, etc.)
        /// </summary>
        [MaxLength(200)]
        public string? Context { get; set; }

        /// <summary>
        /// Nível de severidade do log (INFO, WARNING, ERROR)
        /// </summary>
        [MaxLength(20)]
        public string Severity { get; set; } = "INFO";

        /// <summary>
        /// Categoria do log para agrupamento
        /// </summary>
        [MaxLength(50)]
        public string? Category { get; set; }

        /// <summary>
        /// Dados adicionais em formato JSON
        /// </summary>
        [Column(TypeName = "TEXT")]
        public string? AdditionalData { get; set; }

        /// <summary>
        /// Data e hora da execução da ação
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Duração da operação em milissegundos
        /// </summary>
        public long? Duration { get; set; }

        /// <summary>
        /// Indica se a operação foi bem-sucedida
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Mensagem de erro, se houver
        /// </summary>
        [MaxLength(2000)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Stack trace do erro, se houver
        /// </summary>
        [Column(TypeName = "TEXT")]
        public string? StackTrace { get; set; }

        /// <summary>
        /// ID da sessão do usuário
        /// </summary>
        [MaxLength(255)]
        public string? SessionId { get; set; }

        /// <summary>
        /// ID de correlação para rastrear operações relacionadas
        /// </summary>
        [MaxLength(255)]
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Método HTTP utilizado (GET, POST, PUT, DELETE)
        /// </summary>
        [MaxLength(10)]
        public string? HttpMethod { get; set; }

        /// <summary>
        /// URL da requisição
        /// </summary>
        [MaxLength(500)]
        public string? RequestUrl { get; set; }

        /// <summary>
        /// Código de status HTTP da resposta
        /// </summary>
        public int? HttpStatusCode { get; set; }

        /// <summary>
        /// Indica se este log deve ser retido permanentemente
        /// </summary>
        public bool PermanentRetention { get; set; } = false;

        /// <summary>
        /// Data de expiração para limpeza automática (se não for retenção permanente)
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Hash SHA-256 do conteúdo do log para verificação de integridade
        /// </summary>
        [MaxLength(64)]
        public string? ContentHash { get; set; }

        /// <summary>
        /// Hash SHA-256 do log anterior para criar uma cadeia imutável
        /// </summary>
        [MaxLength(64)]
        public string? PreviousHash { get; set; }

        /// <summary>
        /// Número sequencial do log para garantir ordem e detecção de alterações
        /// </summary>
        public long SequenceNumber { get; set; }

        /// <summary>
        /// Indica se este log foi verificado quanto à integridade
        /// </summary>
        public bool IntegrityVerified { get; set; } = false;
    }
}
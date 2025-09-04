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
        [Column("CreatedAt")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

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
    }

    /// <summary>
    /// Enum para tipos de ação de auditoria
    /// </summary>
    public static class AuditActions
    {
        public const string CREATE = "CREATE";
        public const string UPDATE = "UPDATE";
        public const string DELETE = "DELETE";
        public const string VIEW = "VIEW";
        public const string LOGIN = "LOGIN";
        public const string LOGOUT = "LOGOUT";
        public const string LOGIN_FAILED = "LOGIN_FAILED";
        public const string PASSWORD_CHANGE = "PASSWORD_CHANGE";
        public const string EXPORT = "EXPORT";
        public const string IMPORT = "IMPORT";
        public const string APPROVE = "APPROVE";
        public const string REJECT = "REJECT";
        public const string CANCEL = "CANCEL";
        public const string ACTIVATE = "ACTIVATE";
        public const string DEACTIVATE = "DEACTIVATE";
        public const string SEND_EMAIL = "SEND_EMAIL";
        public const string GENERATE_REPORT = "GENERATE_REPORT";
        public const string BACKUP = "BACKUP";
        public const string RESTORE = "RESTORE";
        public const string CONFIGURATION_CHANGE = "CONFIGURATION_CHANGE";
        public const string SYSTEM_ERROR = "SYSTEM_ERROR";
    }

    /// <summary>
    /// Enum para níveis de severidade
    /// </summary>
    public static class AuditSeverity
    {
        public const string INFO = "INFO";
        public const string WARNING = "WARNING";
        public const string ERROR = "ERROR";
        public const string CRITICAL = "CRITICAL";
    }

    /// <summary>
    /// Enum para categorias de auditoria
    /// </summary>
    public static class AuditCategory
    {
        public const string AUTHENTICATION = "AUTHENTICATION";
        public const string AUTHORIZATION = "AUTHORIZATION";
        public const string DATA_ACCESS = "DATA_ACCESS";
        public const string DATA_MODIFICATION = "DATA_MODIFICATION";
        public const string SYSTEM_ADMINISTRATION = "SYSTEM_ADMINISTRATION";
        public const string BUSINESS_PROCESS = "BUSINESS_PROCESS";
        public const string SECURITY = "SECURITY";
        public const string PERFORMANCE = "PERFORMANCE";
        public const string INTEGRATION = "INTEGRATION";
        public const string REPORTING = "REPORTING";
    }
}
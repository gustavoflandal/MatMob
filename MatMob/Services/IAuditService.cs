using MatMob.Models.Entities;

namespace MatMob.Services
{
    /// <summary>
    /// Interface para o serviço de auditoria
    /// </summary>
    public interface IAuditService
    {
        /// <summary>
        /// Registra um log de auditoria básico
        /// </summary>
        Task LogAsync(string action, string? entityName = null, int? entityId = null, 
                     string? description = null, string? category = null, string? severity = AuditSeverity.INFO);

        /// <summary>
        /// Registra um log de auditoria com dados detalhados
        /// </summary>
        Task LogAsync(AuditLog auditLog);

        /// <summary>
        /// Registra uma operação de criação de entidade
        /// </summary>
        Task LogCreateAsync<T>(T entity, string? description = null) where T : class;

        /// <summary>
        /// Registra uma operação de atualização de entidade
        /// </summary>
        Task LogUpdateAsync<T>(T oldEntity, T newEntity, string? description = null) where T : class;

        /// <summary>
        /// Registra uma operação de exclusão de entidade
        /// </summary>
        Task LogDeleteAsync<T>(T entity, string? description = null) where T : class;

        /// <summary>
        /// Registra um acesso/visualização de entidade
        /// </summary>
        Task LogViewAsync<T>(T entity, string? description = null) where T : class;

        /// <summary>
        /// Registra uma tentativa de login
        /// </summary>
        Task LogLoginAttemptAsync(string username, bool success, string? errorMessage = null);

        /// <summary>
        /// Registra um logout
        /// </summary>
        Task LogLogoutAsync(string username);

        /// <summary>
        /// Registra uma operação de exportação
        /// </summary>
        Task LogExportAsync(string exportType, string? entityName = null, int recordCount = 0);

        /// <summary>
        /// Registra uma operação de importação
        /// </summary>
        Task LogImportAsync(string importType, string? entityName = null, int recordCount = 0);

        /// <summary>
        /// Registra um erro do sistema
        /// </summary>
        Task LogErrorAsync(Exception exception, string? context = null, string? additionalData = null);

        /// <summary>
        /// Registra uma alteração de configuração
        /// </summary>
        Task LogConfigurationChangeAsync(string setting, string? oldValue, string? newValue, string? description = null);

        /// <summary>
        /// Registra uma operação de aprovação
        /// </summary>
        Task LogApprovalAsync(string entityName, int entityId, bool approved, string? comments = null);

        /// <summary>
        /// Inicia o contexto de auditoria para uma operação (retorna ID de correlação)
        /// </summary>
        string StartAuditContext(string operation);

        /// <summary>
        /// Finaliza o contexto de auditoria
        /// </summary>
        Task EndAuditContextAsync(string correlationId, bool success = true, string? errorMessage = null);

        /// <summary>
        /// Busca logs de auditoria com filtros
        /// </summary>
        Task<IEnumerable<AuditLog>> SearchLogsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? userId = null,
            string? action = null,
            string? entityName = null,
            int? entityId = null,
            string? severity = null,
            string? category = null,
            int skip = 0,
            int take = 100);

        /// <summary>
        /// Conta total de logs com filtros aplicados
        /// </summary>
        Task<int> CountLogsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? userId = null,
            string? action = null,
            string? entityName = null,
            int? entityId = null,
            string? severity = null,
            string? category = null);

        /// <summary>
        /// Limpa logs antigos baseado na política de retenção
        /// </summary>
        Task CleanupOldLogsAsync(int retentionDays = 365);

        /// <summary>
        /// Exporta logs para um arquivo
        /// </summary>
        Task<byte[]> ExportLogsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? userId = null,
            string? action = null,
            string? entityName = null,
            string format = "csv");
    }
}
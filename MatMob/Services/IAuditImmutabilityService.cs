using MatMob.Models.Entities;

namespace MatMob.Services
{
    /// <summary>
    /// Interface para o serviço de imutabilidade dos logs de auditoria
    /// </summary>
    public interface IAuditImmutabilityService
    {
        /// <summary>
        /// Prepara um log de auditoria para inserção, calculando hashes e número sequencial
        /// </summary>
        Task<AuditLog> PrepareAuditLogAsync(AuditLog auditLog);

        /// <summary>
        /// Verifica a integridade de um log de auditoria
        /// </summary>
        bool VerifyLogIntegrity(AuditLog auditLog);

        /// <summary>
        /// Verifica a integridade de toda a cadeia de logs
        /// </summary>
        Task<AuditChainIntegrityResult> VerifyChainIntegrityAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Reconstrói os hashes de todos os logs (use apenas em casos de migração)
        /// </summary>
        Task RebuildHashChainAsync();
    }
}
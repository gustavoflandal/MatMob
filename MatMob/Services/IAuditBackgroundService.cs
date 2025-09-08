using MatMob.Models.Entities;

namespace MatMob.Services
{
    /// <summary>
    /// Interface para o serviço de processamento em background de logs de auditoria
    /// </summary>
    public interface IAuditBackgroundService
    {
        /// <summary>
        /// Adiciona um log de auditoria à fila para processamento
        /// </summary>
        void EnqueueLog(AuditLog auditLog);
    }
}
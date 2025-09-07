using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MatMob.Models.Entities;
using MatMob.Data;
using Microsoft.EntityFrameworkCore;

namespace MatMob.Services
{
    /// <summary>
    /// Serviço para garantir a imutabilidade dos logs de auditoria
    /// </summary>
    public class AuditImmutabilityService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuditImmutabilityService> _logger;

        public AuditImmutabilityService(ApplicationDbContext context, ILogger<AuditImmutabilityService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Prepara um log de auditoria para inserção, calculando hashes e número sequencial
        /// </summary>
        public async Task<AuditLog> PrepareAuditLogAsync(AuditLog auditLog)
        {
            try
            {
                // Obter o último log para calcular o hash anterior e número sequencial
                var lastLog = await _context.AuditLogs
                    .OrderByDescending(a => a.SequenceNumber)
                    .FirstOrDefaultAsync();

                // Definir número sequencial
                auditLog.SequenceNumber = (lastLog?.SequenceNumber ?? 0) + 1;

                // Definir hash do log anterior
                auditLog.PreviousHash = lastLog?.ContentHash ?? "0000000000000000000000000000000000000000000000000000000000000000";

                // Calcular hash do conteúdo atual
                auditLog.ContentHash = CalculateContentHash(auditLog);

                // Marcar como verificado inicialmente
                auditLog.IntegrityVerified = true;

                return auditLog;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao preparar log de auditoria para inserção");
                throw;
            }
        }

        /// <summary>
        /// Calcula o hash SHA-256 do conteúdo do log
        /// </summary>
        private string CalculateContentHash(AuditLog auditLog)
        {
            // Criar objeto com os dados essenciais para o hash (excluindo Id, ContentHash e PreviousHash)
            var hashData = new
            {
                auditLog.UserId,
                auditLog.UserName,
                auditLog.IpAddress,
                auditLog.Action,
                auditLog.EntityName,
                auditLog.EntityId,
                auditLog.PropertyName,
                auditLog.OldValue,
                auditLog.NewValue,
                auditLog.OldData,
                auditLog.NewData,
                auditLog.Description,
                auditLog.Context,
                auditLog.Severity,
                auditLog.Category,
                auditLog.CreatedAt,
                auditLog.Duration,
                auditLog.Success,
                auditLog.ErrorMessage,
                auditLog.SessionId,
                auditLog.CorrelationId,
                auditLog.HttpMethod,
                auditLog.RequestUrl,
                auditLog.HttpStatusCode,
                auditLog.SequenceNumber,
                auditLog.PreviousHash
            };

            var jsonString = JsonSerializer.Serialize(hashData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(jsonString));
            return Convert.ToHexString(hashBytes).ToLower();
        }

        /// <summary>
        /// Verifica a integridade de um log de auditoria
        /// </summary>
        public bool VerifyLogIntegrity(AuditLog auditLog)
        {
            try
            {
                var calculatedHash = CalculateContentHash(auditLog);
                return calculatedHash.Equals(auditLog.ContentHash, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar integridade do log {LogId}", auditLog.Id);
                return false;
            }
        }

        /// <summary>
        /// Verifica a integridade de toda a cadeia de logs
        /// </summary>
        public async Task<AuditChainIntegrityResult> VerifyChainIntegrityAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var result = new AuditChainIntegrityResult();

            try
            {
                var query = _context.AuditLogs.AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(a => a.CreatedAt >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(a => a.CreatedAt <= endDate.Value);

                var logs = await query
                    .OrderBy(a => a.SequenceNumber)
                    .ToListAsync();

                result.TotalLogsChecked = logs.Count;

                string expectedPreviousHash = "0000000000000000000000000000000000000000000000000000000000000000";

                foreach (var log in logs)
                {
                    // Verificar hash do conteúdo
                    var contentValid = VerifyLogIntegrity(log);
                    if (!contentValid)
                    {
                        result.CorruptedLogs.Add(new AuditLogIntegrityIssue
                        {
                            LogId = log.Id,
                            SequenceNumber = log.SequenceNumber,
                            IssueType = "Content Hash Mismatch",
                            Description = "O hash do conteúdo não confere com o calculado"
                        });
                    }

                    // Verificar encadeamento
                    if (!log.PreviousHash?.Equals(expectedPreviousHash, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        result.CorruptedLogs.Add(new AuditLogIntegrityIssue
                        {
                            LogId = log.Id,
                            SequenceNumber = log.SequenceNumber,
                            IssueType = "Chain Break",
                            Description = $"Hash anterior esperado: {expectedPreviousHash}, encontrado: {log.PreviousHash}"
                        });
                    }

                    expectedPreviousHash = log.ContentHash ?? "";
                }

                result.IsValid = result.CorruptedLogs.Count == 0;
                result.VerificationDate = DateTime.UtcNow;

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar integridade da cadeia de logs");
                result.IsValid = false;
                result.CorruptedLogs.Add(new AuditLogIntegrityIssue
                {
                    LogId = 0,
                    SequenceNumber = 0,
                    IssueType = "Verification Error",
                    Description = $"Erro durante verificação: {ex.Message}"
                });
                return result;
            }
        }

        /// <summary>
        /// Reconstrói os hashes de todos os logs (use apenas em casos de migração)
        /// </summary>
        public async Task RebuildHashChainAsync()
        {
            var logs = await _context.AuditLogs
                .OrderBy(a => a.CreatedAt)
                .ThenBy(a => a.Id)
                .ToListAsync();

            string previousHash = "0000000000000000000000000000000000000000000000000000000000000000";
            long sequenceNumber = 1;

            foreach (var log in logs)
            {
                log.SequenceNumber = sequenceNumber++;
                log.PreviousHash = previousHash;
                log.ContentHash = CalculateContentHash(log);
                log.IntegrityVerified = true;

                previousHash = log.ContentHash;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Cadeia de hash reconstruída para {Count} logs", logs.Count);
        }
    }

    /// <summary>
    /// Resultado da verificação de integridade da cadeia de logs
    /// </summary>
    public class AuditChainIntegrityResult
    {
        public bool IsValid { get; set; }
        public int TotalLogsChecked { get; set; }
        public List<AuditLogIntegrityIssue> CorruptedLogs { get; set; } = new();
        public DateTime VerificationDate { get; set; }
    }

    /// <summary>
    /// Representa um problema de integridade em um log específico
    /// </summary>
    public class AuditLogIntegrityIssue
    {
        public int LogId { get; set; }
        public long SequenceNumber { get; set; }
        public string IssueType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
using MatMob.Data;
using MatMob.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MatMob.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AuditService> _logger;
        private readonly IAuditImmutabilityService _immutabilityService;
        private readonly IMemoryCache _cache;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IAuditBackgroundService _backgroundService;

        public AuditService(ApplicationDbContext context, IServiceProvider serviceProvider, ILogger<AuditService> logger, IAuditImmutabilityService immutabilityService, IMemoryCache cache, IAuditBackgroundService backgroundService)
        {
            _context = context;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _immutabilityService = immutabilityService;
            _cache = cache;
            _backgroundService = backgroundService;
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };
        }

        private async Task<bool> IsAuditEnabledAsync(string? category, string? action)
        {
            try
            {
                var cacheKey = ($"audit_cfg:{category}:{action}").ToUpperInvariant();
                if (_cache.TryGetValue(cacheKey, out bool enabled))
                {
                    return enabled;
                }

                // Carrega do banco regra específica e regras mais genéricas (por categoria e global)
                var query = _context.AuditModuleConfigs.AsQueryable();

                // Se não houver nenhuma regra cadastrada, habilita tudo (fail-open)
                var hasAnyRule = await query.AnyAsync();
                if (!hasAnyRule)
                {
                    enabled = true;
                }
                else
                {
                    // Habilita apenas se existir pelo menos uma regra Enabled que case com categoria/ação (suporta genéricos por módulo/processo vazio)
                    var rules = await query
                        .Where(r => r.Enabled)
                        .Where(r =>
                            (string.IsNullOrEmpty(r.Module) || r.Module == category) &&
                            (string.IsNullOrEmpty(r.Process) || r.Process == action))
                        .ToListAsync();

                    enabled = rules.Any();
                }

                _cache.Set(cacheKey, enabled, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
                });

                return enabled;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao consultar configurações de auditoria, assumindo habilitado");
                return true; // fail-open
            }
        }

        // ===== Implementações restantes de IAuditService =====

        public async Task LogCreateAsync<T>(T entity, string? description = null) where T : class
        {
            var auditLog = new AuditLog
            {
                Action = AuditActions.CREATE,
                EntityName = typeof(T).Name,
                EntityId = GetEntityId(entity),
                NewData = JsonSerializer.Serialize(entity, _jsonOptions),
                Description = description ?? $"Criação de {typeof(T).Name}",
                Category = AuditCategory.DATA_MODIFICATION,
                Severity = AuditSeverity.INFO,
                CreatedAt = DateTime.UtcNow
            };

            bool isTestEnvironment = _context.Database.ProviderName?.Contains("InMemory") == true;
            await SaveAuditLogAsync(auditLog, saveImmediately: isTestEnvironment);
        }

        public async Task LogUpdateAsync<T>(T oldEntity, T newEntity, string? description = null) where T : class
        {
            var changes = GetChanges(oldEntity, newEntity);
            
            var auditLog = new AuditLog
            {
                Action = AuditActions.UPDATE,
                EntityName = typeof(T).Name,
                EntityId = GetEntityId(newEntity),
                OldData = JsonSerializer.Serialize(oldEntity, _jsonOptions),
                NewData = JsonSerializer.Serialize(newEntity, _jsonOptions),
                Description = description ?? $"Atualização de {typeof(T).Name}",
                Category = AuditCategory.DATA_MODIFICATION,
                Severity = AuditSeverity.INFO,
                CreatedAt = DateTime.UtcNow
            };

            if (changes.Any())
            {
                auditLog.AdditionalData = JsonSerializer.Serialize(changes, _jsonOptions);
            }

            bool isTestEnvironment = _context.Database.ProviderName?.Contains("InMemory") == true;
            await SaveAuditLogAsync(auditLog, saveImmediately: isTestEnvironment);
        }

        public async Task LogDeleteAsync<T>(T entity, string? description = null) where T : class
        {
            var auditLog = new AuditLog
            {
                Action = AuditActions.DELETE,
                EntityName = typeof(T).Name,
                EntityId = GetEntityId(entity),
                OldData = JsonSerializer.Serialize(entity, _jsonOptions),
                Description = description ?? $"Exclusão de {typeof(T).Name}",
                Category = AuditCategory.DATA_MODIFICATION,
                Severity = AuditSeverity.WARNING,
                CreatedAt = DateTime.UtcNow
            };

            // Para testes ou logs críticos, salva imediatamente
            bool isTestEnvironment = _context.Database.ProviderName?.Contains("InMemory") == true;
            await SaveAuditLogAsync(auditLog, saveImmediately: isTestEnvironment);
        }

        public async Task LogViewAsync<T>(T entity, string? description = null) where T : class
        {
            var auditLog = new AuditLog
            {
                Action = AuditActions.VIEW,
                EntityName = typeof(T).Name,
                EntityId = GetEntityId(entity),
                Description = description ?? $"Visualização de {typeof(T).Name}",
                Category = AuditCategory.DATA_ACCESS,
                Severity = AuditSeverity.INFO,
                CreatedAt = DateTime.UtcNow
            };

            await SaveAuditLogAsync(auditLog);
        }

        public async Task LogLoginAttemptAsync(string username, bool success, string? errorMessage = null)
        {
            var auditLog = new AuditLog
            {
                Action = success ? AuditActions.LOGIN : AuditActions.LOGIN_FAILED,
                Description = success ? $"Login bem-sucedido: {username}" : $"Falha no login: {username}",
                Category = AuditCategory.AUTHENTICATION,
                Severity = success ? AuditSeverity.INFO : AuditSeverity.WARNING,
                Success = success,
                ErrorMessage = errorMessage,
                CreatedAt = DateTime.UtcNow
            };

            if (!success)
            {
                auditLog.AdditionalData = JsonSerializer.Serialize(new { Username = username, ErrorMessage = errorMessage }, _jsonOptions);
            }

            bool isTestEnvironment = _context.Database.ProviderName?.Contains("InMemory") == true;
            await SaveAuditLogAsync(auditLog, saveImmediately: isTestEnvironment);
        }

        public async Task LogLogoutAsync(string username)
        {
            var auditLog = new AuditLog
            {
                Action = AuditActions.LOGOUT,
                Description = $"Logout: {username}",
                Category = AuditCategory.AUTHENTICATION,
                Severity = AuditSeverity.INFO,
                CreatedAt = DateTime.UtcNow
            };

            await SaveAuditLogAsync(auditLog);
        }

        public async Task LogExportAsync(string exportType, string? entityName = null, int recordCount = 0)
        {
            var auditLog = new AuditLog
            {
                Action = AuditActions.EXPORT,
                EntityName = entityName,
                Description = $"Exportação de dados: {exportType}",
                Category = AuditCategory.REPORTING,
                Severity = AuditSeverity.INFO,
                CreatedAt = DateTime.UtcNow,
                AdditionalData = JsonSerializer.Serialize(new { ExportType = exportType, RecordCount = recordCount }, _jsonOptions)
            };

            await SaveAuditLogAsync(auditLog);
        }

        public async Task LogImportAsync(string importType, string? entityName = null, int recordCount = 0)
        {
            var auditLog = new AuditLog
            {
                Action = AuditActions.IMPORT,
                EntityName = entityName,
                Description = $"Importação de dados: {importType}",
                Category = AuditCategory.DATA_MODIFICATION,
                Severity = AuditSeverity.INFO,
                CreatedAt = DateTime.UtcNow,
                AdditionalData = JsonSerializer.Serialize(new { ImportType = importType, RecordCount = recordCount }, _jsonOptions)
            };

            await SaveAuditLogAsync(auditLog);
        }

        public async Task LogErrorAsync(Exception exception, string? context = null, string? additionalData = null)
        {
            var auditLog = new AuditLog
            {
                Action = AuditActions.SYSTEM_ERROR,
                Description = $"Erro do sistema: {exception.Message}",
                Category = AuditCategory.SYSTEM_ADMINISTRATION,
                Severity = AuditSeverity.ERROR,
                Success = false,
                ErrorMessage = exception.Message,
                StackTrace = exception.StackTrace,
                Context = context,
                AdditionalData = additionalData,
                CreatedAt = DateTime.UtcNow
            };

            await SaveAuditLogAsync(auditLog, saveImmediately: true);
        }

        public async Task LogConfigurationChangeAsync(string setting, string? oldValue, string? newValue, string? description = null)
        {
            var auditLog = new AuditLog
            {
                Action = AuditActions.CONFIGURATION_CHANGE,
                PropertyName = setting,
                OldValue = oldValue,
                NewValue = newValue,
                Description = description ?? $"Alteração de configuração: {setting}",
                Category = AuditCategory.SYSTEM_ADMINISTRATION,
                Severity = AuditSeverity.INFO,
                CreatedAt = DateTime.UtcNow
            };

            await SaveAuditLogAsync(auditLog);
        }

        public async Task LogApprovalAsync(string entityName, int entityId, bool approved, string? comments = null)
        {
            var auditLog = new AuditLog
            {
                Action = approved ? AuditActions.APPROVE : AuditActions.REJECT,
                EntityName = entityName,
                EntityId = entityId,
                Description = $"{(approved ? "Aprovação" : "Rejeição")} de {entityName} ID {entityId}",
                Category = AuditCategory.BUSINESS_PROCESS,
                Severity = AuditSeverity.INFO,
                CreatedAt = DateTime.UtcNow,
                AdditionalData = JsonSerializer.Serialize(new { Approved = approved, Comments = comments }, _jsonOptions)
            };

            await SaveAuditLogAsync(auditLog);
        }

        public string StartAuditContext(string operation)
        {
            return Guid.NewGuid().ToString();
        }

        public async Task EndAuditContextAsync(string correlationId, bool success = true, string? errorMessage = null)
        {
            var auditLog = new AuditLog
            {
                CorrelationId = correlationId,
                Action = success ? "OPERATION_COMPLETED" : "OPERATION_FAILED",
                Description = success ? "Operação concluída com sucesso" : "Operação falhou",
                Success = success,
                ErrorMessage = errorMessage,
                Severity = success ? AuditSeverity.INFO : AuditSeverity.ERROR,
                CreatedAt = DateTime.UtcNow
            };

            await SaveAuditLogAsync(auditLog, saveImmediately: true);
        }

        public async Task<IEnumerable<AuditLog>> SearchLogsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? userId = null,
            string? action = null,
            string? entityName = null,
            int? entityId = null,
            string? severity = null,
            string? category = null,
            int skip = 0,
            int take = 100)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(l => l.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(l => l.CreatedAt <= endDate.Value);

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(l => l.UserId == userId);

            if (!string.IsNullOrEmpty(action))
                query = query.Where(l => l.Action == action);

            if (!string.IsNullOrEmpty(entityName))
                query = query.Where(l => l.EntityName == entityName);

            if (entityId.HasValue)
                query = query.Where(l => l.EntityId == entityId);

            if (!string.IsNullOrEmpty(severity))
                query = query.Where(l => l.Severity == severity);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(l => l.Category == category);

            return await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> CountLogsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? userId = null,
            string? action = null,
            string? entityName = null,
            int? entityId = null,
            string? severity = null,
            string? category = null)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(l => l.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(l => l.CreatedAt <= endDate.Value);

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(l => l.UserId == userId);

            if (!string.IsNullOrEmpty(action))
                query = query.Where(l => l.Action == action);

            if (!string.IsNullOrEmpty(entityName))
                query = query.Where(l => l.EntityName == entityName);

            if (entityId.HasValue)
                query = query.Where(l => l.EntityId == entityId);

            if (!string.IsNullOrEmpty(severity))
                query = query.Where(l => l.Severity == severity);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(l => l.Category == category);

            return await query.CountAsync();
        }

        public async Task CleanupOldLogsAsync(int retentionDays = 365)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

            var logsToDelete = await _context.AuditLogs
                .Where(l => l.CreatedAt < cutoffDate && !l.PermanentRetention)
                .ToListAsync();

            if (logsToDelete.Any())
            {
                _context.AuditLogs.RemoveRange(logsToDelete);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Cleanup de logs de auditoria: {Count} registros removidos", logsToDelete.Count);
            }
        }

        public async Task<byte[]> ExportLogsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? userId = null,
            string? action = null,
            string? entityName = null,
            string format = "csv")
        {
            var logs = await SearchLogsAsync(startDate, endDate, userId, action, entityName, null, null, null, 0, int.MaxValue);

            if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
            {
                return ExportToCsv(logs);
            }
            else
            {
                return ExportToJson(logs);
            }
        }

        // ===== Helpers =====

        private int? GetEntityId<T>(T entity) where T : class
        {
            if (entity == null) return null;

            var idProperty = typeof(T).GetProperty("Id") ?? typeof(T).GetProperty($"{typeof(T).Name}Id");
            if (idProperty != null && idProperty.CanRead)
            {
                var value = idProperty.GetValue(entity);
                if (value is int i) return i;
            }
            return null;
        }

        private List<PropertyChange> DetectChanges<T>(T oldEntity, T newEntity) where T : class
        {
            var changes = new List<PropertyChange>();
            if (oldEntity == null || newEntity == null) return changes;

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0);

            foreach (var p in properties)
            {
                try
                {
                    var oldVal = p.GetValue(oldEntity)?.ToString();
                    var newVal = p.GetValue(newEntity)?.ToString();
                    if (!Equals(oldVal, newVal))
                    {
                        changes.Add(new PropertyChange { PropertyName = p.Name, OldValue = oldVal, NewValue = newVal, ChangeType = "Modified" });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erro ao detectar alterações na propriedade {Prop}", p.Name);
                }
            }
            return changes;
        }

        private class PropertyChange
        {
            public string PropertyName { get; set; } = string.Empty;
            public string? OldValue { get; set; }
            public string? NewValue { get; set; }
            public string? ChangeType { get; set; }
        }

        private byte[] ExportToCsv(IEnumerable<AuditLog> logs)
        {
            var sb = new StringBuilder();
            sb.AppendLine("CreatedAt,UserId,UserName,Action,EntityName,EntityId,Description,Category,Severity,IpAddress,Success");
            foreach (var l in logs)
            {
                sb.AppendLine($"{l.CreatedAt:yyyy-MM-dd HH:mm:ss},{l.UserId},{l.UserName},{l.Action},{l.EntityName},{l.EntityId},\"{(l.Description ?? string.Empty).Replace("\"","\"\"")}\",{l.Category},{l.Severity},{l.IpAddress},{l.Success}");
            }
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private byte[] ExportToJson(IEnumerable<AuditLog> logs)
        {
            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(logs, _jsonOptions));
        }

        // Implementação dos métodos da interface IAuditService
        public async Task LogAsync(string action, string? entityName = null, int? entityId = null,
                                 string? description = null, string? category = null, string? severity = AuditSeverity.INFO)
        {
            if (string.IsNullOrEmpty(action)) throw new ArgumentNullException(nameof(action));
            
            var auditLog = new AuditLog
            {
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Description = description,
                Category = category ?? AuditCategory.DATA_ACCESS,
                Severity = severity ?? AuditSeverity.INFO,
                CreatedAt = DateTime.UtcNow
            };

            bool isTestEnvironment = _context.Database.ProviderName?.Contains("InMemory") == true;
            await SaveAuditLogAsync(auditLog, saveImmediately: isTestEnvironment);
        }

        public async Task LogAsync(AuditLog auditLog)
        {
            if (auditLog == null) throw new ArgumentNullException(nameof(auditLog));
            
            if (auditLog.CreatedAt == default)
                auditLog.CreatedAt = DateTime.UtcNow;
            
            bool isTestEnvironment = _context.Database.ProviderName?.Contains("InMemory") == true;
            await SaveAuditLogAsync(auditLog, saveImmediately: isTestEnvironment);
        }

        private async Task SaveAuditLogAsync(AuditLog auditLog, bool saveImmediately = false)
        {
            if (auditLog == null) throw new ArgumentNullException(nameof(auditLog));

            // Verifica se auditoria está habilitada para este módulo/processo
            if (!await IsAuditEnabledAsync(auditLog.Category, auditLog.Action))
            {
                _logger.LogDebug("Auditoria desabilitada para Category={Category}, Action={Action}", auditLog.Category, auditLog.Action);
                return;
            }

            // Preenche dados contextuais
            FillContextualData(auditLog);

            // Prepara o log com hash e dados de imutabilidade
            auditLog = await _immutabilityService.PrepareAuditLogAsync(auditLog);

            if (saveImmediately)
            {
                // Salva diretamente no banco para casos críticos ou testes
                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Usa o background service para performance
                _backgroundService.EnqueueLog(auditLog);
            }
        }

        private void FillContextualData(AuditLog auditLog)
        {
            try
            {
                var httpContextAccessor = _serviceProvider.GetService<IHttpContextAccessor>();
                var httpContext = httpContextAccessor?.HttpContext;
                
                if (httpContext != null)
                {
                    // IP Address
                    if (string.IsNullOrEmpty(auditLog.IpAddress))
                    {
                        auditLog.IpAddress = httpContext.Connection.RemoteIpAddress?.ToString();
                    }

                    // User information
                    if (httpContext.User?.Identity?.IsAuthenticated == true)
                    {
                        if (string.IsNullOrEmpty(auditLog.UserId))
                        {
                            auditLog.UserId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                        }

                        if (string.IsNullOrEmpty(auditLog.UserName))
                        {
                            auditLog.UserName = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ??
                                               httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ??
                                               httpContext.User.Identity.Name;
                        }
                    }

                    // HTTP Method and URL
                    if (string.IsNullOrEmpty(auditLog.HttpMethod))
                    {
                        auditLog.HttpMethod = httpContext.Request.Method;
                    }

                    if (string.IsNullOrEmpty(auditLog.RequestUrl))
                    {
                        auditLog.RequestUrl = $"{httpContext.Request.Path}{httpContext.Request.QueryString}";
                    }

                    // Session ID
                    if (string.IsNullOrEmpty(auditLog.SessionId))
                    {
                        auditLog.SessionId = httpContext.Session?.Id;
                    }
                }

                // Generate correlation ID if not provided
                if (string.IsNullOrEmpty(auditLog.CorrelationId))
                {
                    auditLog.CorrelationId = Guid.NewGuid().ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao preencher dados contextuais do log de auditoria");
            }
        }

        private Dictionary<string, object> GetChanges<T>(T oldEntity, T newEntity) where T : class
        {
            var changes = new Dictionary<string, object>();
            
            if (oldEntity == null || newEntity == null)
                return changes;

            var properties = typeof(T).GetProperties();
            
            foreach (var property in properties)
            {
                try
                {
                    var oldValue = property.GetValue(oldEntity);
                    var newValue = property.GetValue(newEntity);
                    
                    if (!Equals(oldValue, newValue))
                    {
                        changes[property.Name] = new { OldValue = oldValue, NewValue = newValue };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erro ao comparar propriedade {Property} para auditoria", property.Name);
                }
            }
            
            return changes;
        }
    }
}

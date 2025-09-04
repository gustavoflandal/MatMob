using MatMob.Data;
using MatMob.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using System.Reflection;

namespace MatMob.Services
{
    /// <summary>
    /// Implementação do serviço de auditoria
    /// </summary>
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<AuditService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public AuditService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            UserManager<IdentityUser> userManager,
            ILogger<AuditService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task LogAsync(string action, string? entityName = null, int? entityId = null, 
                                  string? description = null, string? category = null, string? severity = AuditSeverity.INFO)
        {
            var auditLog = await CreateBaseAuditLogAsync();
            auditLog.Action = action;
            auditLog.EntityName = entityName;
            auditLog.EntityId = entityId;
            auditLog.Description = description;
            auditLog.Category = category ?? AuditCategory.DATA_ACCESS;
            auditLog.Severity = severity ?? AuditSeverity.INFO;

            await SaveAuditLogAsync(auditLog);
        }

        public async Task LogAsync(AuditLog auditLog)
        {
            // Completar dados básicos se não fornecidos
            await PopulateBaseAuditDataAsync(auditLog);
            await SaveAuditLogAsync(auditLog);
        }

        public async Task LogCreateAsync<T>(T entity, string? description = null) where T : class
        {
            var auditLog = await CreateBaseAuditLogAsync();
            auditLog.Action = AuditActions.CREATE;
            auditLog.EntityName = typeof(T).Name;
            auditLog.EntityId = GetEntityId(entity);
            auditLog.NewData = JsonSerializer.Serialize(entity, _jsonOptions);
            auditLog.Description = description ?? $"Criação de {typeof(T).Name}";
            auditLog.Category = AuditCategory.DATA_MODIFICATION;
            auditLog.Severity = AuditSeverity.INFO;

            await SaveAuditLogAsync(auditLog);
        }

        public async Task LogUpdateAsync<T>(T oldEntity, T newEntity, string? description = null) where T : class
        {
            var auditLog = await CreateBaseAuditLogAsync();
            auditLog.Action = AuditActions.UPDATE;
            auditLog.EntityName = typeof(T).Name;
            auditLog.EntityId = GetEntityId(newEntity);
            auditLog.OldData = JsonSerializer.Serialize(oldEntity, _jsonOptions);
            auditLog.NewData = JsonSerializer.Serialize(newEntity, _jsonOptions);
            auditLog.Description = description ?? $"Atualização de {typeof(T).Name}";
            auditLog.Category = AuditCategory.DATA_MODIFICATION;
            auditLog.Severity = AuditSeverity.INFO;

            // Detectar propriedades alteradas
            var changes = DetectChanges(oldEntity, newEntity);
            if (changes.Any())
            {
                auditLog.AdditionalData = JsonSerializer.Serialize(changes, _jsonOptions);
            }

            await SaveAuditLogAsync(auditLog);
        }

        public async Task LogDeleteAsync<T>(T entity, string? description = null) where T : class
        {
            var auditLog = await CreateBaseAuditLogAsync();
            auditLog.Action = AuditActions.DELETE;
            auditLog.EntityName = typeof(T).Name;
            auditLog.EntityId = GetEntityId(entity);
            auditLog.OldData = JsonSerializer.Serialize(entity, _jsonOptions);
            auditLog.Description = description ?? $"Exclusão de {typeof(T).Name}";
            auditLog.Category = AuditCategory.DATA_MODIFICATION;
            auditLog.Severity = AuditSeverity.WARNING;

            await SaveAuditLogAsync(auditLog);
        }

        public async Task LogViewAsync<T>(T entity, string? description = null) where T : class
        {
            var auditLog = await CreateBaseAuditLogAsync();
            auditLog.Action = AuditActions.VIEW;
            auditLog.EntityName = typeof(T).Name;
            auditLog.EntityId = GetEntityId(entity);
            auditLog.Description = description ?? $"Visualização de {typeof(T).Name}";
            auditLog.Category = AuditCategory.DATA_ACCESS;
            auditLog.Severity = AuditSeverity.INFO;

            await SaveAuditLogAsync(auditLog);
        }

        public async Task LogLoginAttemptAsync(string username, bool success, string? errorMessage = null)
        {
            var auditLog = await CreateBaseAuditLogAsync();
            auditLog.Action = success ? AuditActions.LOGIN : AuditActions.LOGIN_FAILED;
            auditLog.Description = success ? $"Login bem-sucedido: {username}" : $"Falha no login: {username}";
            auditLog.Category = AuditCategory.AUTHENTICATION;
            auditLog.Severity = success ? AuditSeverity.INFO : AuditSeverity.WARNING;
            auditLog.Success = success;
            auditLog.ErrorMessage = errorMessage;

            if (!success)
            {
                auditLog.AdditionalData = JsonSerializer.Serialize(new { Username = username, ErrorMessage = errorMessage }, _jsonOptions);
            }

            await SaveAuditLogAsync(auditLog);
        }

        public async Task LogLogoutAsync(string username)
        {
            var auditLog = await CreateBaseAuditLogAsync();
            auditLog.Action = AuditActions.LOGOUT;
            auditLog.Description = $"Logout: {username}";
            auditLog.Category = AuditCategory.AUTHENTICATION;
            auditLog.Severity = AuditSeverity.INFO;

            await SaveAuditLogAsync(auditLog);
        }

        public async Task LogExportAsync(string exportType, string? entityName = null, int recordCount = 0)
        {
            var auditLog = await CreateBaseAuditLogAsync();
            auditLog.Action = AuditActions.EXPORT;
            auditLog.EntityName = entityName;
            auditLog.Description = $"Exportação de dados: {exportType}";
            auditLog.Category = AuditCategory.REPORTING;
            auditLog.Severity = AuditSeverity.INFO;
            auditLog.AdditionalData = JsonSerializer.Serialize(new { ExportType = exportType, RecordCount = recordCount }, _jsonOptions);

            await SaveAuditLogAsync(auditLog);
        }

        public async Task LogImportAsync(string importType, string? entityName = null, int recordCount = 0)
        {
            var auditLog = await CreateBaseAuditLogAsync();
            auditLog.Action = AuditActions.IMPORT;
            auditLog.EntityName = entityName;
            auditLog.Description = $"Importação de dados: {importType}";
            auditLog.Category = AuditCategory.DATA_MODIFICATION;
            auditLog.Severity = AuditSeverity.INFO;
            auditLog.AdditionalData = JsonSerializer.Serialize(new { ImportType = importType, RecordCount = recordCount }, _jsonOptions);

            await SaveAuditLogAsync(auditLog);
        }

        public async Task LogErrorAsync(Exception exception, string? context = null, string? additionalData = null)
        {
            var auditLog = await CreateBaseAuditLogAsync();
            auditLog.Action = AuditActions.SYSTEM_ERROR;
            auditLog.Description = $"Erro do sistema: {exception.Message}";
            auditLog.Category = AuditCategory.SYSTEM_ADMINISTRATION;
            auditLog.Severity = AuditSeverity.ERROR;
            auditLog.Success = false;
            auditLog.ErrorMessage = exception.Message;
            auditLog.StackTrace = exception.StackTrace;
            auditLog.Context = context;
            auditLog.AdditionalData = additionalData;

            await SaveAuditLogAsync(auditLog);
        }

        public async Task LogConfigurationChangeAsync(string setting, string? oldValue, string? newValue, string? description = null)
        {
            var auditLog = await CreateBaseAuditLogAsync();
            auditLog.Action = AuditActions.CONFIGURATION_CHANGE;
            auditLog.PropertyName = setting;
            auditLog.OldValue = oldValue;
            auditLog.NewValue = newValue;
            auditLog.Description = description ?? $"Alteração de configuração: {setting}";
            auditLog.Category = AuditCategory.SYSTEM_ADMINISTRATION;
            auditLog.Severity = AuditSeverity.INFO;

            await SaveAuditLogAsync(auditLog);
        }

        public async Task LogApprovalAsync(string entityName, int entityId, bool approved, string? comments = null)
        {
            var auditLog = await CreateBaseAuditLogAsync();
            auditLog.Action = approved ? AuditActions.APPROVE : AuditActions.REJECT;
            auditLog.EntityName = entityName;
            auditLog.EntityId = entityId;
            auditLog.Description = $"{(approved ? "Aprovação" : "Rejeição")} de {entityName} ID {entityId}";
            auditLog.Category = AuditCategory.BUSINESS_PROCESS;
            auditLog.Severity = AuditSeverity.INFO;
            auditLog.AdditionalData = JsonSerializer.Serialize(new { Approved = approved, Comments = comments }, _jsonOptions);

            await SaveAuditLogAsync(auditLog);
        }

        public string StartAuditContext(string operation)
        {
            var correlationId = Guid.NewGuid().ToString();
            // Aqui você pode armazenar o contexto se necessário
            return correlationId;
        }

        public async Task EndAuditContextAsync(string correlationId, bool success = true, string? errorMessage = null)
        {
            var auditLog = await CreateBaseAuditLogAsync();
            auditLog.CorrelationId = correlationId;
            auditLog.Action = success ? "OPERATION_COMPLETED" : "OPERATION_FAILED";
            auditLog.Description = success ? "Operação concluída com sucesso" : "Operação falhou";
            auditLog.Success = success;
            auditLog.ErrorMessage = errorMessage;
            auditLog.Severity = success ? AuditSeverity.INFO : AuditSeverity.ERROR;

            await SaveAuditLogAsync(auditLog);
        }

        public async Task<IEnumerable<AuditLog>> SearchLogsAsync(
            DateTime? startDate = null, DateTime? endDate = null, string? userId = null,
            string? action = null, string? entityName = null, int? entityId = null,
            string? severity = null, string? category = null, int skip = 0, int take = 100)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(l => l.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(l => l.Timestamp <= endDate.Value);

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
                .OrderByDescending(l => l.Timestamp)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> CountLogsAsync(
            DateTime? startDate = null, DateTime? endDate = null, string? userId = null,
            string? action = null, string? entityName = null, int? entityId = null,
            string? severity = null, string? category = null)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(l => l.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(l => l.Timestamp <= endDate.Value);

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
                .Where(l => l.Timestamp < cutoffDate && !l.PermanentRetention)
                .ToListAsync();

            if (logsToDelete.Any())
            {
                _context.AuditLogs.RemoveRange(logsToDelete);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Cleanup de logs de auditoria: {Count} registros removidos", logsToDelete.Count);
            }
        }

        public async Task<byte[]> ExportLogsAsync(
            DateTime? startDate = null, DateTime? endDate = null, string? userId = null,
            string? action = null, string? entityName = null, string format = "csv")
        {
            var logs = await SearchLogsAsync(startDate, endDate, userId, action, entityName, null, null, null, 0, int.MaxValue);

            if (format.ToLower() == "csv")
            {
                return ExportToCsv(logs);
            }
            else
            {
                return ExportToJson(logs);
            }
        }

        #region Private Methods

        private async Task<AuditLog> CreateBaseAuditLogAsync()
        {
            var auditLog = new AuditLog
            {
                Timestamp = DateTime.UtcNow
            };

            await PopulateBaseAuditDataAsync(auditLog);
            return auditLog;
        }

        private async Task PopulateBaseAuditDataAsync(AuditLog auditLog)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            
            if (httpContext != null)
            {
                // Informações do usuário
                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    auditLog.UserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    auditLog.UserName = httpContext.User.FindFirstValue(ClaimTypes.Name) ?? 
                                       httpContext.User.FindFirstValue(ClaimTypes.Email);
                }

                // Informações da requisição
                auditLog.IpAddress = GetClientIpAddress(httpContext);
                auditLog.UserAgent = httpContext.Request.Headers["User-Agent"].ToString();
                auditLog.HttpMethod = httpContext.Request.Method;
                auditLog.RequestUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}{httpContext.Request.QueryString}";
                auditLog.SessionId = httpContext.Session?.Id;

                // Informações do contexto
                var routeData = httpContext.GetRouteData();
                if (routeData != null)
                {
                    var controller = routeData.Values["controller"]?.ToString();
                    var action = routeData.Values["action"]?.ToString();
                    if (!string.IsNullOrEmpty(controller) && !string.IsNullOrEmpty(action))
                    {
                        auditLog.Context = $"{controller}.{action}";
                    }
                }
            }

            // Definir data de expiração padrão (1 ano)
            auditLog.ExpirationDate = DateTime.UtcNow.AddYears(1);
        }

        private async Task SaveAuditLogAsync(AuditLog auditLog)
        {
            try
            {
                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar log de auditoria");
                // Não propagar a exceção para não afetar a operação principal
            }
        }

        private string GetClientIpAddress(HttpContext httpContext)
        {
            var ipAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            
            if (string.IsNullOrEmpty(ipAddress) || ipAddress.ToLower() == "unknown")
            {
                ipAddress = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
            }
            
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            }

            return ipAddress ?? "Unknown";
        }

        private int? GetEntityId<T>(T entity) where T : class
        {
            try
            {
                var idProperty = typeof(T).GetProperty("Id");
                if (idProperty != null && idProperty.PropertyType == typeof(int))
                {
                    return (int?)idProperty.GetValue(entity);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private List<PropertyChange> DetectChanges<T>(T oldEntity, T newEntity) where T : class
        {
            var changes = new List<PropertyChange>();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (property.CanRead)
                {
                    var oldValue = property.GetValue(oldEntity);
                    var newValue = property.GetValue(newEntity);

                    if (!Equals(oldValue, newValue))
                    {
                        changes.Add(new PropertyChange
                        {
                            PropertyName = property.Name,
                            OldValue = oldValue?.ToString(),
                            NewValue = newValue?.ToString()
                        });
                    }
                }
            }

            return changes;
        }

        private byte[] ExportToCsv(IEnumerable<AuditLog> logs)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Timestamp,UserId,UserName,Action,EntityName,EntityId,Description,IpAddress,Success");

            foreach (var log in logs)
            {
                csv.AppendLine($"{log.Timestamp:yyyy-MM-dd HH:mm:ss},{log.UserId},{log.UserName},{log.Action},{log.EntityName},{log.EntityId},{log.Description},{log.IpAddress},{log.Success}");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        private byte[] ExportToJson(IEnumerable<AuditLog> logs)
        {
            var json = JsonSerializer.Serialize(logs, _jsonOptions);
            return Encoding.UTF8.GetBytes(json);
        }

        #endregion

        private class PropertyChange
        {
            public string PropertyName { get; set; } = string.Empty;
            public string? OldValue { get; set; }
            public string? NewValue { get; set; }
        }
    }
}
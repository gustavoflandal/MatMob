using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MatMob.Services;
using System.Security.Claims;
using System.IO;

namespace MatMob.Tests.Middleware
{
    /// <summary>
    /// Middleware para auditoria de acesso e operações
    /// </summary>
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditMiddleware> _logger;

    public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger, IServiceProvider serviceProvider)
    {
        _next = next;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    private readonly IServiceProvider _serviceProvider;        public async Task InvokeAsync(HttpContext context)
        {
            var startTime = DateTime.UtcNow;
            var originalBodyStream = context.Response.Body;

            try
            {
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                await _next(context);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                // Log apenas requisições específicas para auditoria
                if (ShouldAudit(context))
                {
                    await LogAuditEvent(context, startTime, endTime, duration);
                }

                // Restaurar o stream original e copiar a resposta
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in audit middleware for request {Path}", context.Request.Path);
                throw;
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        private bool ShouldAudit(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";
            var method = context.Request.Method.ToUpper();

            // Auditar apenas operações críticas ou de modificação
            var criticalPaths = new[]
            {
                "/usermanagement",
                "/permission",
                "/api/",
                "/ordensservico",
                "/ativos",
                "/tecnicos",
                "/equipes",
                "/pecas"
            };

            var auditMethods = new[] { "POST", "PUT", "DELETE", "PATCH" };

            // Auditar GET apenas para controladores de gerenciamento
            var managementPaths = new[] { "/usermanagement", "/permission" };

            bool shouldAudit = false;

            // Verificar se é uma operação de modificação
            if (auditMethods.Contains(method))
            {
                shouldAudit = criticalPaths.Any(cp => path.StartsWith(cp));
            }
            // Verificar se é acesso a páginas de gerenciamento (GET)
            else if (method == "GET")
            {
                shouldAudit = managementPaths.Any(mp => path.StartsWith(mp));
            }

            // Não auditar recursos estáticos
            var staticExtensions = new[] { ".css", ".js", ".png", ".jpg", ".jpeg", ".gif", ".ico", ".svg", ".woff", ".woff2" };
            if (staticExtensions.Any(ext => path.EndsWith(ext)))
            {
                shouldAudit = false;
            }

            return shouldAudit;
        }

        private Task LogAuditEvent(HttpContext context, DateTime startTime, DateTime endTime, TimeSpan duration)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();

                var user = context.User;
                var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
                var userName = user?.FindFirst(ClaimTypes.Name)?.Value ?? user?.FindFirst(ClaimTypes.Email)?.Value ?? "Anonymous";

                var auditData = new
                {
                    Timestamp = startTime,
                    UserId = userId,
                    UserName = userName,
                    IpAddress = GetClientIpAddress(context),
                    UserAgent = context.Request.Headers["User-Agent"].ToString(),
                    Method = context.Request.Method,
                    Path = context.Request.Path.Value,
                    Query = context.Request.QueryString.Value,
                    StatusCode = context.Response.StatusCode,
                    Duration = duration.TotalMilliseconds,
                    Referer = context.Request.Headers["Referer"].ToString()
                };

                // Log estruturado para auditoria
                _logger.LogInformation("AUDIT: {@AuditData}", auditData);

                // Para operações sensíveis, log adicional
                if (IsSensitiveOperation(context))
                {
                    var sensitiveData = new
                    {
                        Action = "SENSITIVE_OPERATION",
                        Details = $"{context.Request.Method} {context.Request.Path}",
                        User = userName,
                        IP = GetClientIpAddress(context),
                        Timestamp = startTime,
                        Success = context.Response.StatusCode < 400
                    };

                    _logger.LogWarning("SENSITIVE_AUDIT: {@SensitiveData}", sensitiveData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging audit event");
            }
            
            return Task.CompletedTask;
        }

        private bool IsSensitiveOperation(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";
            var method = context.Request.Method.ToUpper();

            var sensitivePaths = new[]
            {
                "/usermanagement/create",
                "/usermanagement/edit",
                "/usermanagement/delete",
                "/permission/createrole",
                "/permission/editrole",
                "/permission/deleterole",
                "/permission/togglerolepermission"
            };

            return sensitivePaths.Any(sp => path.StartsWith(sp)) ||
                   (path.Contains("usermanagement") && method != "GET") ||
                   (path.Contains("permission") && method != "GET");
        }

        private string GetClientIpAddress(HttpContext context)
        {
            try
            {
                // Verificar headers de proxy
                var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    return forwardedFor.Split(',')[0].Trim();
                }

                var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
                if (!string.IsNullOrEmpty(realIp))
                {
                    return realIp;
                }

                // IP direto
                return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }
    }

    /// <summary>
    /// Extension method para registrar o middleware
    /// </summary>
    public static class AuditMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuditMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuditMiddleware>();
        }
    }
}
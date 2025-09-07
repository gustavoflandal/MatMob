using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace MatMob.Services
{
    /// <summary>
    /// Filtro global para registrar auditoria automaticamente em ações MVC.
    /// Regras simples:
    /// - GET => VIEW
    /// - POST/PUT/PATCH/DELETE: tenta inferir pela ActionName (Create/Update/Edit/Delete), senão usa HttpMethod como ação.
    /// - Categoria padrão por método: DATA_ACCESS (GET) e DATA_MODIFICATION (demais).
    /// </summary>
    public class AuditActionFilter : IAsyncActionFilter
    {
        private readonly IAuditService _auditService;
        private readonly ILogger<AuditActionFilter> _logger;

        public AuditActionFilter(IAuditService auditService, ILogger<AuditActionFilter> logger)
        {
            _auditService = auditService;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpMethod = context.HttpContext.Request.Method?.ToUpperInvariant() ?? "GET";
            var routeValues = context.ActionDescriptor.RouteValues;
            string? controller = null;
            string? actionName = null;
            if (routeValues != null)
            {
                routeValues.TryGetValue("controller", out controller);
                routeValues.TryGetValue("action", out actionName);
            }
            controller ??= "Unknown";
            actionName ??= "Unknown";

            // Inicia timer
            var sw = Stopwatch.StartNew();
            var executedContext = await next();
            sw.Stop();

            try
            {
                // Determinar ação e categoria
                string auditAction = InferAction(httpMethod, actionName);
                string category = httpMethod == "GET" ? AuditCategory.DATA_ACCESS : AuditCategory.DATA_MODIFICATION;

                // Entidade e Id (se houver)
                string entityName = controller ?? "Unknown";
                int? entityId = null;
                if (context.RouteData.Values.TryGetValue("id", out var idObj) && idObj != null)
                {
                    if (int.TryParse(idObj.ToString(), out var parsedId)) entityId = parsedId;
                }

                // Severidade
                var severity = AuditSeverity.INFO;

                // Descrição
                var description = $"{httpMethod} {controller}/{actionName}";

                // Apenas registra se não houve exceção não tratada
                var success = executedContext.Exception == null;

                await _auditService.LogAsync(
                    action: auditAction,
                    entityName: entityName,
                    entityId: entityId,
                    description: description,
                    category: category,
                    severity: severity
                );
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "AuditActionFilter falhou ao registrar auditoria automática.");
            }
        }

        private static string InferAction(string httpMethod, string? actionName)
        {
            actionName = (actionName ?? string.Empty).ToUpperInvariant();
            if (httpMethod == "GET") return AuditActions.VIEW;

            if (actionName.Contains("CREATE") || actionName.Contains("NOVO") || actionName.Contains("ADD"))
                return AuditActions.CREATE;
            if (actionName.Contains("EDIT") || actionName.Contains("UPDATE") || actionName.Contains("ATUALIZA"))
                return AuditActions.UPDATE;
            if (actionName.Contains("DELETE") || actionName.Contains("EXCLUI"))
                return AuditActions.DELETE;

            // fallback por método
            return httpMethod switch
            {
                "POST" => AuditActions.CREATE,
                "PUT" => AuditActions.UPDATE,
                "PATCH" => AuditActions.UPDATE,
                "DELETE" => AuditActions.DELETE,
                _ => AuditActions.VIEW
            };
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MatMob.Data;
using MatMob.Services;
using MatMob.Models.Entities;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MatMob.Controllers
{
    [Authorize(Roles = "Administrador,Gestor")]
    public class AuditController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public AuditController(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // GET: Audit
        public async Task<IActionResult> Index(
            string? searchString,
            string? userName,
            string? action,
            string? category,
            string? severity,
            DateTime? startDate,
            DateTime? endDate,
            int page = 1,
            int pageSize = 20)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["UserNameFilter"] = userName;
            ViewData["ActionFilter"] = action;
            ViewData["CategoryFilter"] = category;
            ViewData["SeverityFilter"] = severity;
            ViewData["StartDateFilter"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDateFilter"] = endDate?.ToString("yyyy-MM-dd");

            var auditLogs = _context.AuditLogs.AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(searchString))
            {
                auditLogs = auditLogs.Where(a =>
                    a.EntityName!.Contains(searchString) ||
                    a.Description!.Contains(searchString) ||
                    a.UserName!.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(userName))
            {
                auditLogs = auditLogs.Where(a => a.UserName == userName);
            }

            if (!string.IsNullOrEmpty(action))
            {
                auditLogs = auditLogs.Where(a => a.Action == action);
            }

            if (!string.IsNullOrEmpty(category))
            {
                auditLogs = auditLogs.Where(a => a.Category == category);
            }

            if (!string.IsNullOrEmpty(severity))
            {
                auditLogs = auditLogs.Where(a => a.Severity == severity);
            }

            if (startDate.HasValue)
            {
                auditLogs = auditLogs.Where(a => a.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                // Incluir o final do dia
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                auditLogs = auditLogs.Where(a => a.CreatedAt <= endOfDay);
            }

            // Ordenar por data decrescente (mais recentes primeiro)
            auditLogs = auditLogs.OrderByDescending(a => a.CreatedAt);

            // Paginação
            var totalRecords = await auditLogs.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            var auditLogsPaged = await auditLogs
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Preparar dados para as dropdowns
            ViewBag.Users = await _context.AuditLogs
                .Select(a => a.UserName)
                .Distinct()
                .OrderBy(u => u)
                .ToListAsync();

            ViewBag.Actions = await _context.AuditLogs
                .Select(a => a.Action)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();

            ViewBag.Categories = await _context.AuditLogs
                .Select(a => a.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            ViewBag.Severities = await _context.AuditLogs
                .Select(a => a.Severity)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            // Dados de paginação
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalRecords = totalRecords;

            return View(auditLogsPaged);
        }

        // GET: Audit/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auditLog = await _context.AuditLogs
                .FirstOrDefaultAsync(m => m.Id == id);

            if (auditLog == null)
            {
                return NotFound();
            }

            // Registrar auditoria de visualização
            await _auditService.LogViewAsync(
                entity: auditLog,
                description: $"Visualização do log de auditoria ID: {auditLog.Id}"
            );

            return View(auditLog);
        }

        // GET: Audit/Export
        public async Task<IActionResult> Export(
            string? searchString,
            string? userName,
            string? action,
            string? category,
            string? severity,
            DateTime? startDate,
            DateTime? endDate,
            string format = "csv")
        {
            var auditLogs = _context.AuditLogs.AsQueryable();

            // Aplicar os mesmos filtros do Index
            if (!string.IsNullOrEmpty(searchString))
            {
                auditLogs = auditLogs.Where(a =>
                    a.EntityName!.Contains(searchString) ||
                    a.Description!.Contains(searchString) ||
                    a.UserName!.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(userName))
            {
                auditLogs = auditLogs.Where(a => a.UserName == userName);
            }

            if (!string.IsNullOrEmpty(action))
            {
                auditLogs = auditLogs.Where(a => a.Action == action);
            }

            if (!string.IsNullOrEmpty(category))
            {
                auditLogs = auditLogs.Where(a => a.Category == category);
            }

            if (!string.IsNullOrEmpty(severity))
            {
                auditLogs = auditLogs.Where(a => a.Severity == severity);
            }

            if (startDate.HasValue)
            {
                auditLogs = auditLogs.Where(a => a.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                auditLogs = auditLogs.Where(a => a.CreatedAt <= endOfDay);
            }

            var logsToExport = await auditLogs
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            // Registrar auditoria de exportação
            await _auditService.LogAsync(
                action: Services.AuditActions.EXPORT,
                entityName: "AuditLogs",
                description: $"Exportação de {logsToExport.Count} logs de auditoria no formato {format.ToUpper()}",
                category: Services.AuditCategory.REPORTING
            );

            if (format.ToLower() == "csv")
            {
                return ExportToCsv(logsToExport);
            }
            else if (format.ToLower() == "json")
            {
                return ExportToJson(logsToExport);
            }

            return RedirectToAction(nameof(Index));
        }

        private IActionResult ExportToCsv(List<AuditLog> logs)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("ID,CreatedAt,UserName,Action,EntityName,EntityId,Description,Category,Severity,IpAddress,UserAgent,OldValue,NewValue");

            foreach (var log in logs)
            {
                csv.AppendLine($"{log.Id},{log.CreatedAt:yyyy-MM-dd HH:mm:ss},{log.UserName},{log.Action},{log.EntityName},{log.EntityId},{log.Description},{log.Category},{log.Severity},{log.IpAddress},{log.UserAgent},{log.OldValue},{log.NewValue}");
            }

            var fileName = $"audit_logs_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
        }

        private IActionResult ExportToJson(List<AuditLog> logs)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(logs, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            var fileName = $"audit_logs_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", fileName);
        }

        // GET: Audit/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            // Estatísticas gerais
            var totalLogs = await _context.AuditLogs.CountAsync();
            var logsLast24h = await _context.AuditLogs
                .Where(a => a.CreatedAt >= DateTime.Now.AddDays(-1))
                .CountAsync();

            var logsLast7d = await _context.AuditLogs
                .Where(a => a.CreatedAt >= DateTime.Now.AddDays(-7))
                .CountAsync();

            var logsLast30d = await _context.AuditLogs
                .Where(a => a.CreatedAt >= DateTime.Now.AddDays(-30))
                .CountAsync();

            // Estatísticas por ação
            var actionsStats = await _context.AuditLogs
                .GroupBy(a => a.Action)
                .Select(g => new { Action = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            // Estatísticas por usuário
            var usersStats = await _context.AuditLogs
                .GroupBy(a => a.UserName)
                .Select(g => new { UserName = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            // Estatísticas por categoria
            var categoriesStats = await _context.AuditLogs
                .GroupBy(a => a.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            // Estatísticas por severidade
            var severityStats = await _context.AuditLogs
                .GroupBy(a => a.Severity)
                .Select(g => new { Severity = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            // Logs mais recentes
            var recentLogs = await _context.AuditLogs
                .OrderByDescending(a => a.CreatedAt)
                .Take(20)
                .ToListAsync();

            ViewBag.TotalLogs = totalLogs;
            ViewBag.LogsLast24h = logsLast24h;
            ViewBag.LogsLast7d = logsLast7d;
            ViewBag.LogsLast30d = logsLast30d;
            ViewBag.ActionsStats = actionsStats;
            ViewBag.UsersStats = usersStats;
            ViewBag.CategoriesStats = categoriesStats;
            ViewBag.SeverityStats = severityStats;
            ViewBag.RecentLogs = recentLogs;

            // Registrar auditoria de acesso ao dashboard
            await _auditService.LogAsync(
                action: Services.AuditActions.VIEW,
                entityName: "AuditDashboard",
                description: "Acesso ao dashboard de auditoria",
                category: Services.AuditCategory.REPORTING
            );

            return View();
        }

        // GET: Audit/GenerateTestLogs
        public async Task<IActionResult> GenerateTestLogs()
        {
            try
            {
                // Gerar alguns logs de teste
                await _auditService.LogAsync(
                    action: Services.AuditActions.CREATE,
                    entityName: "TestLog",
                    description: "Log de teste gerado automaticamente",
                    category: Services.AuditCategory.SYSTEM_ADMINISTRATION
                );

                await _auditService.LogAsync(
                    action: Services.AuditActions.UPDATE,
                    entityName: "TestLog",
                    description: "Log de atualização de teste",
                    category: Services.AuditCategory.DATA_MODIFICATION
                );

                await _auditService.LogAsync(
                    action: Services.AuditActions.DELETE,
                    entityName: "TestLog",
                    description: "Log de exclusão de teste",
                    category: Services.AuditCategory.SECURITY
                );

                TempData["SuccessMessage"] = "3 logs de teste foram gerados com sucesso!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao gerar logs de teste: {ex.Message}";
            }

            return RedirectToAction("Diagnostics");
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MatMob.Data;

namespace MatMob.Controllers
{
    [Authorize(Roles = "Administrador,Gestor")]
    public class AuditController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var logs = await _context.AuditLogs
                .OrderByDescending(x => x.CreatedAt)
                .Take(100)
                .ToListAsync();
            
            return View(logs);
        }

        public async Task<IActionResult> Details(int id)
        {
            var log = await _context.AuditLogs.FindAsync(id);
            if (log == null)
                return NotFound();
            
            return View(log);
        }

        public async Task<IActionResult> Dashboard()
        {
            // Estatísticas básicas dos logs de auditoria
            var totalLogs = await _context.AuditLogs.CountAsync();
            var logsHoje = await _context.AuditLogs
                .Where(x => x.CreatedAt.Date == DateTime.Today)
                .CountAsync();
            
            var logsSemana = await _context.AuditLogs
                .Where(x => x.CreatedAt >= DateTime.Today.AddDays(-7))
                .CountAsync();

            var logsErro = await _context.AuditLogs
                .Where(x => !x.Success)
                .CountAsync();

            // Estatísticas por ação
            var acoesMaisComuns = await _context.AuditLogs
                .GroupBy(x => x.Action)
                .Select(g => new { Acao = g.Key, Total = g.Count() })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToListAsync();

            // Estatísticas por categoria
            var categorias = await _context.AuditLogs
                .Where(x => !string.IsNullOrEmpty(x.Category))
                .GroupBy(x => x.Category)
                .Select(g => new { Categoria = g.Key, Total = g.Count() })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            // Logs recentes
            var logsRecentes = await _context.AuditLogs
                .OrderByDescending(x => x.CreatedAt)
                .Take(10)
                .ToListAsync();

            ViewBag.TotalLogs = totalLogs;
            ViewBag.LogsHoje = logsHoje;
            ViewBag.LogsSemana = logsSemana;
            ViewBag.LogsErro = logsErro;
            ViewBag.AcoesMaisComuns = acoesMaisComuns;
            ViewBag.Categorias = categorias;
            ViewBag.LogsRecentes = logsRecentes;

            return View();
        }
    }
}

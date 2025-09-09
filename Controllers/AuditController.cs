using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MatMob.Data;
using MatMob.Models.Entities;
using MatMob.Services;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace MatMob.Controllers
{
    [Authorize]
    public class AuditController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly UserManager<IdentityUser> _userManager;

        public AuditController(
            ApplicationDbContext context,
            IAuditService auditService,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _auditService = auditService;
            _userManager = userManager;
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
            int pageNumber = 1,
            int pageSize = 20)
        {
            Console.WriteLine("=== AUDIT INDEX INICIADO ===");
            Console.WriteLine($"Parâmetros recebidos:");
            Console.WriteLine($"  SearchString: '{searchString ?? "NULL"}'");
            Console.WriteLine($"  UserName: '{userName ?? "NULL"}'");
            Console.WriteLine($"  Action: '{action ?? "NULL"}'");
            Console.WriteLine($"  Category: '{category ?? "NULL"}'");
            Console.WriteLine($"  Severity: '{severity ?? "NULL"}'");
            Console.WriteLine($"  StartDate: {startDate?.ToString("yyyy-MM-dd") ?? "NULL"}");
            Console.WriteLine($"  EndDate: {endDate?.ToString("yyyy-MM-dd") ?? "NULL"}");
            Console.WriteLine($"  PageNumber: {pageNumber}, PageSize: {pageSize}");

            // Verificar se há ALGUM filtro informado
            // IMPORTANTE: Ignorar action="Index" pois é adicionado automaticamente pelo MVC routing
            bool hasAnyFilter = !string.IsNullOrEmpty(searchString) ||
                               !string.IsNullOrEmpty(userName) ||
                               (!string.IsNullOrEmpty(action) && action != "Index") ||
                               !string.IsNullOrEmpty(category) ||
                               !string.IsNullOrEmpty(severity) ||
                               startDate.HasValue ||
                               endDate.HasValue;

            Console.WriteLine($"=== HÁ FILTROS ATIVOS: {hasAnyFilter} ===");

            var query = _context.AuditLogs.AsQueryable();

            // APLICAR FILTROS APENAS SE EXISTIREM
            if (hasAnyFilter)
            {
                Console.WriteLine("=== EXECUTANDO QUERY COM FILTROS ===");

                if (!string.IsNullOrEmpty(searchString))
                {
                    Console.WriteLine($"  Aplicando filtro de busca: '{searchString}'");
                    query = query.Where(a => 
                        (a.Description != null && a.Description.Contains(searchString)) ||
                        (a.EntityName != null && a.EntityName.Contains(searchString)) ||
                        (a.Context != null && a.Context.Contains(searchString)));
                }

                if (!string.IsNullOrEmpty(userName))
                {
                    Console.WriteLine($"  Aplicando filtro de usuário: '{userName}'");
                    query = query.Where(a => a.UserName != null && a.UserName.Contains(userName));
                }

                if (!string.IsNullOrEmpty(action) && action != "Index")
                {
                    Console.WriteLine($"  Aplicando filtro de ação: '{action}'");
                    query = query.Where(a => a.Action == action);
                }

                if (!string.IsNullOrEmpty(category))
                {
                    Console.WriteLine($"  Aplicando filtro de categoria: '{category}'");
                    query = query.Where(a => a.Category == category);
                }

                if (!string.IsNullOrEmpty(severity))
                {
                    Console.WriteLine($"  Aplicando filtro de severidade: '{severity}'");
                    query = query.Where(a => a.Severity == severity);
                }

                if (startDate.HasValue)
                {
                    Console.WriteLine($"  Aplicando filtro de data inicial: {startDate:yyyy-MM-dd}");
                    query = query.Where(a => a.CreatedAt >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    Console.WriteLine($"  Aplicando filtro de data final: {endDate:yyyy-MM-dd}");
                    var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(a => a.CreatedAt <= endOfDay);
                }
            }
            else
            {
                Console.WriteLine("=== EXECUTANDO QUERY SEM FILTROS (TODOS OS REGISTROS) ===");
                // Query sem WHERE - pega todos os registros
            }

            // Contar total de registros (com ou sem filtros)
            var totalRecords = await query.CountAsync();
            Console.WriteLine($"=== TOTAL DE REGISTROS ENCONTRADOS: {totalRecords} ===");

            // Aplicar paginação e ordenação (SEMPRE mais recentes primeiro)
            var auditLogs = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Console.WriteLine($"=== RETORNANDO {auditLogs.Count} LOGS DA PÁGINA {pageNumber} ===");

            // Preencher ViewBags para os dropdowns (sempre todos os valores disponíveis)
            ViewBag.Users = await _context.AuditLogs
                .Where(a => a.UserName != null && a.UserName != "")
                .Select(a => a.UserName)
                .Distinct()
                .OrderBy(u => u)
                .ToListAsync();

            ViewBag.Actions = await _context.AuditLogs
                .Where(a => a.Action != "")
                .Select(a => a.Action)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();

            ViewBag.Categories = await _context.AuditLogs
                .Where(a => a.Category != null && a.Category != "")
                .Select(a => a.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            ViewBag.Severities = await _context.AuditLogs
                .Where(a => a.Severity != "")
                .Select(a => a.Severity)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            // Preencher ViewData para manter filtros na interface
            ViewData["CurrentFilter"] = searchString;
            ViewData["UserNameFilter"] = userName;
            ViewData["ActionFilter"] = action;
            ViewData["CategoryFilter"] = category;
            ViewData["SeverityFilter"] = severity;
            ViewData["StartDateFilter"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDateFilter"] = endDate?.ToString("yyyy-MM-dd");

            // Preencher ViewBag para paginação
            ViewBag.TotalRecords = totalRecords;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            Console.WriteLine("=== AUDIT INDEX FINALIZADO COM SUCESSO ===");
            return View(auditLogs);
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
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(a => 
                    (a.Description != null && a.Description.Contains(searchString)) ||
                    (a.EntityName != null && a.EntityName.Contains(searchString)) ||
                    (a.Context != null && a.Context.Contains(searchString)));
            }

            if (!string.IsNullOrEmpty(userName))
            {
                query = query.Where(a => a.UserName != null && a.UserName.Contains(userName));
            }

            if (!string.IsNullOrEmpty(action))
            {
                query = query.Where(a => a.Action == action);
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(a => a.Category == category);
            }

            if (!string.IsNullOrEmpty(severity))
            {
                query = query.Where(a => a.Severity == severity);
            }

            if (startDate.HasValue)
            {
                query = query.Where(a => a.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                var endDateTime = endDate.Value.Date.AddDays(1);
                query = query.Where(a => a.CreatedAt < endDateTime);
            }

            var auditLogs = await query
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            if (format.ToLower() == "json")
            {
                var json = JsonSerializer.Serialize(auditLogs, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                return File(Encoding.UTF8.GetBytes(json), "application/json", "audit_logs.json");
            }
            else
            {
                // CSV export
                var csv = new StringBuilder();
                csv.AppendLine("ID,CreatedAt,UserName,Action,EntityName,EntityId,Description,Category,Severity,IpAddress,Success");

                foreach (var log in auditLogs)
                {
                    csv.AppendLine($"{log.Id},{log.CreatedAt:yyyy-MM-dd HH:mm:ss},{log.UserName},{log.Action},{log.EntityName},{log.EntityId},\"{log.Description?.Replace("\"", "\"\"")}\",{log.Category},{log.Severity},{log.IpAddress},{log.Success}");
                }

                return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "audit_logs.csv");
            }
        }

        // GET: Audit/CreateTestData - Criar dados de teste
        [AllowAnonymous]
        public async Task<IActionResult> CreateTestData()
        {
            try
            {
                Console.WriteLine("=== CRIANDO DADOS DE TESTE ===");
                
                var testLogs = new List<AuditLog>
                {
                    new AuditLog
                    {
                        Action = "VIEW",
                        EntityName = "Ativo",
                        EntityId = 1,
                        Description = "Visualização de ativo ID 1",
                        Category = "DATA_ACCESS",
                        Severity = "INFO",
                        UserName = "admin@matmob.com",
                        IpAddress = "127.0.0.1",
                        CreatedAt = DateTime.UtcNow.AddHours(-1),
                        Success = true
                    },
                    new AuditLog
                    {
                        Action = "CREATE",
                        EntityName = "OrdemServico",
                        EntityId = 123,
                        Description = "Criação de nova ordem de serviço",
                        Category = "DATA_MODIFICATION",
                        Severity = "INFO",
                        UserName = "admin@matmob.com",
                        IpAddress = "127.0.0.1",
                        CreatedAt = DateTime.UtcNow.AddHours(-2),
                        Success = true
                    },
                    new AuditLog
                    {
                        Action = "UPDATE",
                        EntityName = "Tecnico",
                        EntityId = 5,
                        Description = "Atualização de dados do técnico",
                        Category = "DATA_MODIFICATION",
                        Severity = "WARNING",
                        UserName = "gestor@matmob.com",
                        IpAddress = "192.168.1.100",
                        CreatedAt = DateTime.UtcNow.AddHours(-3),
                        Success = true
                    },
                    new AuditLog
                    {
                        Action = "DELETE",
                        EntityName = "Peca",
                        EntityId = 99,
                        Description = "Exclusão de peça obsoleta",
                        Category = "DATA_MODIFICATION",
                        Severity = "ERROR",
                        UserName = "admin@matmob.com",
                        IpAddress = "127.0.0.1",
                        CreatedAt = DateTime.UtcNow.AddHours(-4),
                        Success = false,
                        ErrorMessage = "Peça possui dependências"
                    },
                    new AuditLog
                    {
                        Action = "LOGIN",
                        EntityName = "User",
                        Description = "Login no sistema",
                        Category = "AUTHENTICATION",
                        Severity = "INFO",
                        UserName = "tecnico@matmob.com",
                        IpAddress = "192.168.1.50",
                        CreatedAt = DateTime.UtcNow.AddHours(-5),
                        Success = true
                    }
                };
                
                _context.AuditLogs.AddRange(testLogs);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"SUCESSO: {testLogs.Count} logs criados!");
                
                return Json(new { 
                    Success = true, 
                    Message = $"{testLogs.Count} logs de teste criados com sucesso!",
                    LogsCreated = testLogs.Count 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO: {ex.Message}");
                return Json(new { 
                    Success = false, 
                    Error = ex.Message, 
                    StackTrace = ex.StackTrace 
                });
            }
        }

        // GET: Audit/TestData - Action para testar sem autenticação
        [AllowAnonymous]
        public async Task<IActionResult> TestData()
        {
            try
            {
                Console.WriteLine("=== TESTDATA: Iniciando teste direto ===");
                
                var totalCount = await _context.AuditLogs.CountAsync();
                Console.WriteLine($"TESTDATA: Total de registros na base: {totalCount}");
                
                var logs = await _context.AuditLogs
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(10)
                    .ToListAsync();
                    
                Console.WriteLine($"TESTDATA: Retornando {logs.Count} registros");
                
                var result = new
                {
                    TotalCount = totalCount,
                    SampleLogs = logs.Select(l => new {
                        l.Id,
                        l.Action,
                        l.EntityName,
                        l.EntityId,
                        l.Description,
                        l.Category,
                        l.Severity,
                        l.UserName,
                        CreatedAt = l.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss"),
                        l.Success
                    })
                };
                
                return Json(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TESTDATA: Erro - {ex.Message}");
                return Json(new { Error = ex.Message, StackTrace = ex.StackTrace });
            }
        }

        // GET: Audit/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);

            var totalLogs = await _context.AuditLogs.CountAsync();
            var recentLogs = await _context.AuditLogs
                .Where(a => a.CreatedAt >= thirtyDaysAgo)
                .CountAsync();

            var logsByAction = await _context.AuditLogs
                .GroupBy(a => a.Action)
                .Select(g => new { Action = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(10)
                .ToListAsync();

            var logsByUser = await _context.AuditLogs
                .Where(a => a.UserName != null)
                .GroupBy(a => a.UserName)
                .Select(g => new { UserName = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(10)
                .ToListAsync();

            var viewModel = new
            {
                TotalLogs = totalLogs,
                RecentLogs = recentLogs,
                LogsByAction = logsByAction,
                LogsByUser = logsByUser
            };

            return View(viewModel);
        }

        // POST: Audit/GenerateTestLogs
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateTestLogs()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var userName = currentUser?.UserName ?? "admin@matmob.com";

            // Generate comprehensive test audit logs with various scenarios
            var testLogs = new List<AuditLog>
            {
                new AuditLog
                {
                    Action = "VIEW",
                    EntityName = "Ativo",
                    EntityId = 1,
                    Description = "Visualização de ativo ID 1",
                    Category = "DATA_ACCESS",
                    Severity = "INFO",
                    UserName = userName,
                    UserId = currentUser?.Id,
                    IpAddress = "127.0.0.1",
                    CreatedAt = DateTime.UtcNow.AddHours(-1),
                    Success = true
                },
                new AuditLog
                {
                    Action = "CREATE",
                    EntityName = "OrdemServico",
                    EntityId = 123,
                    Description = "Criação de nova ordem de serviço",
                    Category = "DATA_MODIFICATION",
                    Severity = "INFO",
                    UserName = userName,
                    UserId = currentUser?.Id,
                    IpAddress = "127.0.0.1",
                    CreatedAt = DateTime.UtcNow.AddHours(-2),
                    Success = true,
                    NewData = "{\"NumeroOS\":\"OS-2025-001\",\"Status\":1,\"Prioridade\":\"Alta\"}"
                },
                new AuditLog
                {
                    Action = "UPDATE",
                    EntityName = "Tecnico",
                    EntityId = 5,
                    Description = "Atualização de dados do técnico",
                    Category = "DATA_MODIFICATION",
                    Severity = "INFO",
                    UserName = userName,
                    UserId = currentUser?.Id,
                    IpAddress = "127.0.0.1",
                    CreatedAt = DateTime.UtcNow.AddHours(-3),
                    Success = true,
                    OldData = "{\"Status\":\"Ativo\",\"Especialidade\":\"Mecânica\"}",
                    NewData = "{\"Status\":\"Inativo\",\"Especialidade\":\"Elétrica\"}"
                },
                new AuditLog
                {
                    Action = "DELETE",
                    EntityName = "Peca",
                    EntityId = 99,
                    Description = "Exclusão de peça obsoleta",
                    Category = "DATA_MODIFICATION",
                    Severity = "WARNING",
                    UserName = userName,
                    UserId = currentUser?.Id,
                    IpAddress = "127.0.0.1",
                    CreatedAt = DateTime.UtcNow.AddHours(-4),
                    Success = true,
                    OldData = "{\"Nome\":\"Peça Obsoleta\",\"Codigo\":\"P99\",\"Ativa\":false}"
                },
                new AuditLog
                {
                    Action = "LOGIN",
                    EntityName = "User",
                    Description = "Login no sistema",
                    Category = "AUTHENTICATION",
                    Severity = "INFO",
                    UserName = userName,
                    UserId = currentUser?.Id,
                    IpAddress = "127.0.0.1",
                    CreatedAt = DateTime.UtcNow.AddHours(-5),
                    Success = true
                },
                new AuditLog
                {
                    Action = "EXPORT",
                    EntityName = "AuditLog",
                    Description = "Exportação de logs de auditoria",
                    Category = "REPORTING",
                    Severity = "INFO",
                    UserName = userName,
                    UserId = currentUser?.Id,
                    IpAddress = "127.0.0.1",
                    CreatedAt = DateTime.UtcNow.AddHours(-6),
                    Success = true,
                    AdditionalData = "{\"ExportType\":\"CSV\",\"RecordCount\":150}"
                },
                new AuditLog
                {
                    Action = "UPDATE",
                    EntityName = "OrdemServico",
                    EntityId = 123,
                    Description = "Erro ao atualizar ordem de serviço",
                    Category = "DATA_MODIFICATION",
                    Severity = "ERROR",
                    UserName = userName,
                    UserId = currentUser?.Id,
                    IpAddress = "127.0.0.1",
                    CreatedAt = DateTime.UtcNow.AddHours(-7),
                    Success = false,
                    ErrorMessage = "Violação de constraint: Status inválido"
                }
            };

            _context.AuditLogs.AddRange(testLogs);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Logs de teste gerados com sucesso! {testLogs.Count} registros criados.";
            return RedirectToAction(nameof(Index));
        }
    }
}
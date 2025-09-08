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
            string? entityType,
            string? entityId,
            DateTime? startDate,
            DateTime? endDate,
            int pageNumber = 1,
            int pageSize = 20)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(a => a.Details.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(userName))
            {
                query = query.Where(a => a.UserName.Contains(userName));
            }

            if (!string.IsNullOrEmpty(action))
            {
                query = query.Where(a => a.Action == action);
            }

            if (!string.IsNullOrEmpty(entityType))
            {
                query = query.Where(a => a.EntityType == entityType);
            }

            if (!string.IsNullOrEmpty(entityId))
            {
                query = query.Where(a => a.EntityId == entityId);
            }

            if (startDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(a => a.Timestamp <= endDate.Value);
            }

            var totalRecords = await query.CountAsync();
            var auditLogs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            ViewData["UserNameFilter"] = userName;
            ViewData["ActionFilter"] = action;
            ViewData["EntityTypeFilter"] = entityType;
            ViewData["EntityIdFilter"] = entityId;
            ViewData["StartDateFilter"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDateFilter"] = endDate?.ToString("yyyy-MM-dd");
            ViewData["PageNumber"] = pageNumber;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalRecords / (double)pageSize);

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
            string? entityType,
            string? entityId,
            DateTime? startDate,
            DateTime? endDate,
            string format = "csv")
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(a => a.Details.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(userName))
            {
                query = query.Where(a => a.UserName.Contains(userName));
            }

            if (!string.IsNullOrEmpty(action))
            {
                query = query.Where(a => a.Action == action);
            }

            if (!string.IsNullOrEmpty(entityType))
            {
                query = query.Where(a => a.EntityType == entityType);
            }

            if (!string.IsNullOrEmpty(entityId))
            {
                query = query.Where(a => a.EntityId == entityId);
            }

            if (startDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(a => a.Timestamp <= endDate.Value);
            }

            var auditLogs = await query
                .OrderByDescending(a => a.Timestamp)
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
                csv.AppendLine("ID,Timestamp,UserName,Action,EntityType,EntityId,Details,IPAddress");

                foreach (var log in auditLogs)
                {
                    csv.AppendLine($"{log.Id},{log.Timestamp:yyyy-MM-dd HH:mm:ss},{log.UserName},{log.Action},{log.EntityType},{log.EntityId},\"{log.Details?.Replace("\"", "\"\"")}\",{log.IPAddress}");
                }

                return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "audit_logs.csv");
            }
        }

        // GET: Audit/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);

            var totalLogs = await _context.AuditLogs.CountAsync();
            var recentLogs = await _context.AuditLogs
                .Where(a => a.Timestamp >= thirtyDaysAgo)
                .CountAsync();

            var logsByAction = await _context.AuditLogs
                .GroupBy(a => a.Action)
                .Select(g => new { Action = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(10)
                .ToListAsync();

            var logsByUser = await _context.AuditLogs
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
            var userName = currentUser?.UserName ?? "System";

            // Generate some test audit logs
            await _auditService.LogCreateAsync("TestEntity", "123", "Test creation", userName, "127.0.0.1");
            await _auditService.LogUpdateAsync("TestEntity", "123", "Test update", userName, "127.0.0.1");
            await _auditService.LogDeleteAsync("TestEntity", "123", "Test deletion", userName, "127.0.0.1");

            TempData["Success"] = "Test audit logs generated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
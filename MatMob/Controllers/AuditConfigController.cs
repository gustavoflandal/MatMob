using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MatMob.Data;
using MatMob.Models.Entities;
using MatMob.Models.ViewModels;
using MatMob.Services;
using System.Reflection;

namespace MatMob.Controllers
{
    [Authorize(Roles = "Administrador,Gestor")] 
    public class AuditConfigController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditConfigController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /AuditConfig
        public async Task<IActionResult> Index()
        {
            try
            {
                var items = await _context.AuditModuleConfigs
                    .OrderBy(a => a.Module)
                    .ThenBy(a => a.Process)
                    .ToListAsync();
                return View(items);
            }
            catch
            {
                // Caso a tabela ainda não exista, exibir mensagem amigável orientando a aplicar migrações
                TempData["Error"] = "Módulo de Configuração de Auditoria ainda não está instalado. Execute as migrações do banco (dotnet ef database update) e tente novamente.";
                return View(Enumerable.Empty<AuditModuleConfig>());
            }
        }

        // GET: /AuditConfig/Manage - UI de matriz de módulos x processos
        public async Task<IActionResult> Manage()
        {
            var model = new AuditConfigMatrixViewModel();

            // Obter módulos (categorias) pelos campos const de AuditCategory
            model.Modules = typeof(MatMob.Services.AuditCategory)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                .Select(f => (string)f.GetRawConstantValue()!)
                .OrderBy(s => s)
                .ToList();

            // Obter processos (ações) pelos campos const de AuditActions
            model.Processes = typeof(MatMob.Services.AuditActions)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                .Select(f => (string)f.GetRawConstantValue()!)
                .OrderBy(s => s)
                .ToList();

            // Carregar pares habilitados da base
            try
            {
                var rules = await _context.AuditModuleConfigs
                    .Where(r => r.Enabled)
                    .ToListAsync();

                foreach (var r in rules)
                {
                    if (!string.IsNullOrEmpty(r.Module))
                    {
                        if (string.IsNullOrEmpty(r.Process))
                        {
                            // Regra genérica por módulo: marca todos processos deste módulo
                            foreach (var p in model.Processes)
                            {
                                model.EnabledPairs.Add($"{r.Module}|{p}");
                            }
                        }
                        else
                        {
                            model.EnabledPairs.Add($"{r.Module}|{r.Process}");
                        }
                    }
                }
            }
            catch
            {
                // Se a tabela não existir ainda, a tela abre sem nada marcado
                TempData["Warning"] = "A tabela de configuração não foi encontrada. Aplique as migrações do banco antes de salvar.";
            }

            return View(model);
        }

        // POST: /AuditConfig/Manage - Salvar matriz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(AuditConfigMatrixViewModel model)
        {
            // Normalizar null
            model.EnabledPairs ??= new HashSet<string>();

            // Carregar tudo da base e sincronizar com seleção
            using var trx = await _context.Database.BeginTransactionAsync();
            try
            {
                // Estratégia simples: limpar regras existentes e inserir as selecionadas
                var allExisting = _context.AuditModuleConfigs.AsQueryable();
                _context.AuditModuleConfigs.RemoveRange(allExisting);
                await _context.SaveChangesAsync();

                var toInsert = new List<AuditModuleConfig>();
                foreach (var pair in model.EnabledPairs)
                {
                    var parts = pair.Split('|');
                    if (parts.Length == 2)
                    {
                        toInsert.Add(new AuditModuleConfig
                        {
                            Module = parts[0],
                            Process = parts[1],
                            Enabled = true,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                if (toInsert.Count > 0)
                {
                    await _context.AuditModuleConfigs.AddRangeAsync(toInsert);
                    await _context.SaveChangesAsync();
                }

                await trx.CommitAsync();
                TempData["Success"] = "Configurações de auditoria atualizadas com sucesso.";
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                TempData["Error"] = $"Falha ao salvar configurações: {ex.Message}";
            }

            return RedirectToAction(nameof(Manage));
        }

        // GET: /AuditConfig/Create
        public IActionResult Create()
        {
            return View(new AuditModuleConfig { Enabled = true });
        }

        // POST: /AuditConfig/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Module,Process,Enabled")] AuditModuleConfig model)
        {
            if (ModelState.IsValid)
            {
                _context.AuditModuleConfigs.Add(model);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Regra de auditoria criada com sucesso.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: /AuditConfig/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.AuditModuleConfigs.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
            
        }

        // POST: /AuditConfig/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Module,Process,Enabled")] AuditModuleConfig model)
        {
            if (id != model.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                try
                {
                    model.UpdatedAt = System.DateTime.UtcNow;
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Regra de auditoria atualizada com sucesso.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.AuditModuleConfigs.AnyAsync(e => e.Id == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: /AuditConfig/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.AuditModuleConfigs.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        // POST: /AuditConfig/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.AuditModuleConfigs.FindAsync(id);
            if (item == null) return NotFound();
            _context.AuditModuleConfigs.Remove(item);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Regra de auditoria excluída.";
            return RedirectToAction(nameof(Index));
        }
    }
}

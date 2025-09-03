using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MatMob.Data;
using MatMob.Models.Entities;

namespace MatMob.Controllers
{
    [Authorize]
    public class AtivosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AtivosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Ativos
        public async Task<IActionResult> Index(string? searchString, string? tipoFilter, StatusAtivo? statusFilter)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["TipoFilter"] = tipoFilter;
            ViewData["StatusFilter"] = statusFilter;

            var ativos = _context.Ativos.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                ativos = ativos.Where(a => a.Nome.Contains(searchString) ||
                                          a.Localizacao.Contains(searchString) ||
                                          a.NumeroSerie!.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(tipoFilter))
            {
                ativos = ativos.Where(a => a.Tipo == tipoFilter);
            }

            if (statusFilter.HasValue)
            {
                ativos = ativos.Where(a => a.Status == statusFilter.Value);
            }

            // Buscar tipos únicos para o filtro
            ViewBag.Tipos = await _context.Ativos
                .Select(a => a.Tipo)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            return View(await ativos.OrderBy(a => a.Nome).ToListAsync());
        }

        // GET: Ativos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ativo = await _context.Ativos
                .Include(a => a.OrdensServico)
                .Include(a => a.PlanosManutencao)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ativo == null)
            {
                return NotFound();
            }

            return View(ativo);
        }

        // GET: Ativos/Create
        [Authorize(Roles = "Administrador,Gestor")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Ativos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<IActionResult> Create([Bind("Nome,Tipo,NumeroSerie,Localizacao,DataInstalacao,Status,Descricao")] Ativo ativo)
        {
            if (ModelState.IsValid)
            {
                ativo.DataCadastro = DateTime.Now;
                _context.Add(ativo);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Ativo cadastrado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            return View(ativo);
        }

        // GET: Ativos/Edit/5
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ativo = await _context.Ativos.FindAsync(id);
            if (ativo == null)
            {
                return NotFound();
            }
            return View(ativo);
        }

        // POST: Ativos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Tipo,NumeroSerie,Localizacao,DataInstalacao,Status,Descricao,DataCadastro")] Ativo ativo)
        {
            if (id != ativo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ativo.UltimaAtualizacao = DateTime.Now;
                    _context.Update(ativo);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Ativo atualizado com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AtivoExists(ativo.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(ativo);
        }

        // GET: Ativos/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ativo = await _context.Ativos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ativo == null)
            {
                return NotFound();
            }

            return View(ativo);
        }

        // POST: Ativos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ativo = await _context.Ativos.FindAsync(id);
            if (ativo != null)
            {
                // Verificar se o ativo tem ordens de serviço
                var temOS = await _context.OrdensServico.AnyAsync(os => os.AtivoId == id);
                if (temOS)
                {
                    TempData["Error"] = "Não é possível excluir este ativo pois ele possui ordens de serviço associadas.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Ativos.Remove(ativo);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Ativo excluído com sucesso!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AtivoExists(int id)
        {
            return _context.Ativos.Any(e => e.Id == id);
        }

        // Ação para gerar número de série automaticamente
        [HttpGet]
        public JsonResult GerarNumeroSerie()
        {
            var numeroSerie = $"AT{DateTime.Now:yyyyMMdd}{Random.Shared.Next(1000, 9999)}";
            return Json(numeroSerie);
        }
    }
}

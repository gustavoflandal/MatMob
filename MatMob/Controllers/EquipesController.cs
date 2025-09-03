using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MatMob.Data;
using MatMob.Models.Entities;

namespace MatMob.Controllers
{
    [Authorize(Roles = "Administrador,Gestor")]
    public class EquipesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EquipesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Equipes
        public async Task<IActionResult> Index(string? searchString, StatusEquipe? statusFilter)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["StatusFilter"] = statusFilter;

            var equipes = _context.Equipes
                .Include(e => e.EquipesTecnico.Where(et => et.Ativo))
                    .ThenInclude(et => et.Tecnico)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                equipes = equipes.Where(e => e.Nome.Contains(searchString) ||
                                            e.Descricao!.Contains(searchString));
            }

            if (statusFilter.HasValue)
            {
                equipes = equipes.Where(e => e.Status == statusFilter.Value);
            }

            return View(await equipes.OrderBy(e => e.Nome).ToListAsync());
        }

        // GET: Equipes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

                        var equipe = await _context.Equipes
                .Include(e => e.EquipesTecnico.Where(et => et.Ativo))
                    .ThenInclude(et => et.Tecnico)
                .Include(e => e.OrdensServico)
                    .ThenInclude(os => os.Ativo)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (equipe == null)
            {
                return NotFound();
            }

            return View(equipe);
        }

        // GET: Equipes/Create
        public async Task<IActionResult> Create()
        {
            await PopulateTecnicosDropdown();
            return View();
        }

        // POST: Equipes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nome,Descricao,Status")] Equipe equipe, int[]? tecnicoIds)
        {
            if (ModelState.IsValid)
            {
                equipe.DataCriacao = DateTime.Now;
                _context.Add(equipe);
                await _context.SaveChangesAsync();

                // Adicionar técnicos à equipe
                if (tecnicoIds != null && tecnicoIds.Length > 0)
                {
                    foreach (var tecnicoId in tecnicoIds)
                    {
                        var equipeTecnico = new EquipeTecnico
                        {
                            EquipeId = equipe.Id,
                            TecnicoId = tecnicoId,
                            DataEntrada = DateTime.Now,
                            Ativo = true
                        };
                        _context.EquipesTecnico.Add(equipeTecnico);
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Equipe criada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateTecnicosDropdown();
            return View(equipe);
        }

        // GET: Equipes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipe = await _context.Equipes
                .Include(e => e.EquipesTecnico.Where(et => et.Ativo))
                    .ThenInclude(et => et.Tecnico)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (equipe == null)
            {
                return NotFound();
            }

            await PopulateTecnicosDropdown();
            ViewBag.TecnicosSelecionados = equipe.EquipesTecnico.Select(et => et.TecnicoId).ToArray();

            return View(equipe);
        }

        // POST: Equipes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Descricao,Status,DataCriacao")] Equipe equipe, int[]? tecnicoIds)
        {
            if (id != equipe.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(equipe);

                    // Remover técnicos atuais da equipe
                    var equipeTecnicosAtuais = await _context.EquipesTecnico
                        .Where(et => et.EquipeId == id && et.Ativo)
                        .ToListAsync();

                    foreach (var equipeTecnico in equipeTecnicosAtuais)
                    {
                        equipeTecnico.Ativo = false;
                        equipeTecnico.DataSaida = DateTime.Now;
                    }

                    // Adicionar novos técnicos
                    if (tecnicoIds != null && tecnicoIds.Length > 0)
                    {
                        foreach (var tecnicoId in tecnicoIds)
                        {
                            var equipeTecnicoExistente = equipeTecnicosAtuais
                                .FirstOrDefault(et => et.TecnicoId == tecnicoId);

                            if (equipeTecnicoExistente != null)
                            {
                                // Reativar se já existia
                                equipeTecnicoExistente.Ativo = true;
                                equipeTecnicoExistente.DataSaida = null;
                            }
                            else
                            {
                                // Criar novo
                                var novoEquipeTecnico = new EquipeTecnico
                                {
                                    EquipeId = id,
                                    TecnicoId = tecnicoId,
                                    DataEntrada = DateTime.Now,
                                    Ativo = true
                                };
                                _context.EquipesTecnico.Add(novoEquipeTecnico);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Equipe atualizada com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EquipeExists(equipe.Id))
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

            await PopulateTecnicosDropdown();
            return View(equipe);
        }

        // GET: Equipes/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipe = await _context.Equipes
                .Include(e => e.EquipesTecnico.Where(et => et.Ativo))
                    .ThenInclude(et => et.Tecnico)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (equipe == null)
            {
                return NotFound();
            }

            return View(equipe);
        }

        // POST: Equipes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var equipe = await _context.Equipes.FindAsync(id);
            if (equipe != null)
            {
                // Verificar se a equipe tem ordens de serviço associadas
                var temOS = await _context.OrdensServico.AnyAsync(os => os.EquipeId == id);
                if (temOS)
                {
                    TempData["Error"] = "Não é possível excluir esta equipe pois ela possui ordens de serviço associadas.";
                    return RedirectToAction(nameof(Index));
                }

                // Remover associações com técnicos
                var equipeTecnicos = await _context.EquipesTecnico
                    .Where(et => et.EquipeId == id)
                    .ToListAsync();
                _context.EquipesTecnico.RemoveRange(equipeTecnicos);

                _context.Equipes.Remove(equipe);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Equipe excluída com sucesso!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Equipes/AdicionarTecnico/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarTecnico(int equipeId, int tecnicoId)
        {
            // Verificar se o técnico já está na equipe
            var equipeTecnicoExistente = await _context.EquipesTecnico
                .FirstOrDefaultAsync(et => et.EquipeId == equipeId && et.TecnicoId == tecnicoId && et.Ativo);

            if (equipeTecnicoExistente != null)
            {
                TempData["Error"] = "Este técnico já faz parte da equipe.";
                return RedirectToAction(nameof(Details), new { id = equipeId });
            }

            var equipeTecnico = new EquipeTecnico
            {
                EquipeId = equipeId,
                TecnicoId = tecnicoId,
                DataEntrada = DateTime.Now,
                Ativo = true
            };

            _context.EquipesTecnico.Add(equipeTecnico);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Técnico adicionado à equipe com sucesso!";
            return RedirectToAction(nameof(Details), new { id = equipeId });
        }

        // POST: Equipes/RemoverTecnico/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoverTecnico(int equipeId, int tecnicoId)
        {
            var equipeTecnico = await _context.EquipesTecnico
                .FirstOrDefaultAsync(et => et.EquipeId == equipeId && et.TecnicoId == tecnicoId && et.Ativo);

            if (equipeTecnico == null)
            {
                TempData["Error"] = "Técnico não encontrado na equipe.";
                return RedirectToAction(nameof(Details), new { id = equipeId });
            }

            equipeTecnico.Ativo = false;
            equipeTecnico.DataSaida = DateTime.Now;
            
            _context.Update(equipeTecnico);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Técnico removido da equipe com sucesso!";
            return RedirectToAction(nameof(Details), new { id = equipeId });
        }

        private bool EquipeExists(int id)
        {
            return _context.Equipes.Any(e => e.Id == id);
        }

        private async Task PopulateTecnicosDropdown()
        {
            ViewBag.Tecnicos = await _context.Tecnicos
                .Where(t => t.Status == StatusTecnico.Ativo)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = $"{t.Nome} - {(t.Especialidade ?? t.Especializacao ?? "Sem especialidade")}"
                })
                .ToListAsync();
        }

        // GET: Equipes/Agenda/5
        public async Task<IActionResult> Agenda(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipe = await _context.Equipes
                .Include(e => e.OrdensServico)
                    .ThenInclude(os => os.Ativo)
                .Include(e => e.EquipesTecnico.Where(et => et.Ativo))
                    .ThenInclude(et => et.Tecnico)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (equipe == null)
            {
                return NotFound();
            }

            return View(equipe);
        }
    }
}

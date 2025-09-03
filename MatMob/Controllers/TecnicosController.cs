using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MatMob.Data;
using MatMob.Models.Entities;

namespace MatMob.Controllers
{
    [Authorize(Roles = "Administrador,Gestor")]
    public class TecnicosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TecnicosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Tecnicos
        public async Task<IActionResult> Index(string? searchString, StatusTecnico? statusFilter)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["StatusFilter"] = statusFilter;

            var tecnicos = _context.Tecnicos.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                tecnicos = tecnicos.Where(t => t.Nome.Contains(searchString) ||
                                              t.Email.Contains(searchString) ||
                                              t.Especialidade.Contains(searchString));
            }

            if (statusFilter.HasValue)
            {
                tecnicos = tecnicos.Where(t => t.Status == statusFilter.Value);
            }

            return View(await tecnicos.OrderBy(t => t.Nome).ToListAsync());
        }

        // GET: Tecnicos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tecnico = await _context.Tecnicos
                .Include(t => t.EquipesTecnico)
                    .ThenInclude(et => et.Equipe)
                .Include(t => t.OrdensServicoResponsavel)
                    .ThenInclude(os => os.Ativo)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (tecnico == null)
            {
                return NotFound();
            }

            return View(tecnico);
        }

        // GET: Tecnicos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tecnicos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nome,Email,Telefone,CPF,DataNascimento,Especialidade,DataAdmissao,Status,Observacoes")] Tecnico tecnico)
        {
            if (ModelState.IsValid)
            {
                // Verificar se o email já existe
                var emailExiste = await _context.Tecnicos.AnyAsync(t => t.Email == tecnico.Email);
                if (emailExiste)
                {
                    ModelState.AddModelError("Email", "Este email já está cadastrado para outro técnico.");
                    return View(tecnico);
                }

                tecnico.DataCadastro = DateTime.Now;
                _context.Add(tecnico);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Técnico cadastrado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            return View(tecnico);
        }

        // GET: Tecnicos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tecnico = await _context.Tecnicos.FindAsync(id);
            if (tecnico == null)
            {
                return NotFound();
            }
            return View(tecnico);
        }

        // POST: Tecnicos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Email,Telefone,CPF,DataNascimento,Especialidade,DataAdmissao,Status,Observacoes,DataCadastro")] Tecnico tecnico)
        {
            if (id != tecnico.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar se o email já existe para outro técnico
                    var emailExiste = await _context.Tecnicos
                        .AnyAsync(t => t.Email == tecnico.Email && t.Id != tecnico.Id);
                    if (emailExiste)
                    {
                        ModelState.AddModelError("Email", "Este email já está cadastrado para outro técnico.");
                        return View(tecnico);
                    }

                    _context.Update(tecnico);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Técnico atualizado com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TecnicoExists(tecnico.Id))
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
            return View(tecnico);
        }

        // GET: Tecnicos/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tecnico = await _context.Tecnicos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tecnico == null)
            {
                return NotFound();
            }

            return View(tecnico);
        }

        // POST: Tecnicos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tecnico = await _context.Tecnicos.FindAsync(id);
            if (tecnico != null)
            {
                // Verificar se o técnico tem ordens de serviço associadas
                var temOS = await _context.OrdensServico.AnyAsync(os => os.TecnicoResponsavelId == id);
                if (temOS)
                {
                    TempData["Error"] = "Não é possível excluir este técnico pois ele possui ordens de serviço associadas.";
                    return RedirectToAction(nameof(Index));
                }

                // Remover associações com equipes
                var equipeTecnicos = await _context.EquipesTecnico
                    .Where(et => et.TecnicoId == id)
                    .ToListAsync();
                _context.EquipesTecnico.RemoveRange(equipeTecnicos);

                _context.Tecnicos.Remove(tecnico);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Técnico excluído com sucesso!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TecnicoExists(int id)
        {
            return _context.Tecnicos.Any(e => e.Id == id);
        }

        // GET: Tecnicos/Agenda/5
        public async Task<IActionResult> Agenda(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tecnico = await _context.Tecnicos
                .Include(t => t.OrdensServicoResponsavel)
                    .ThenInclude(os => os.Ativo)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tecnico == null)
            {
                return NotFound();
            }

            return View(tecnico);
        }
    }
}

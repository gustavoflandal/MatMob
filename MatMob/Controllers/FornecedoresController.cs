using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MatMob.Data;
using MatMob.Models.Entities;
using MatMob.Extensions;

namespace MatMob.Controllers
{
    public class FornecedoresController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FornecedoresController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Fornecedores
        public async Task<IActionResult> Index(string? searchString, StatusFornecedor? statusFilter)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["StatusFilter"] = statusFilter;

            var fornecedores = _context.Fornecedores.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                fornecedores = fornecedores.Where(f => f.Nome.Contains(searchString) ||
                                                      f.NomeFantasia.Contains(searchString) ||
                                                      f.CNPJ.Contains(searchString) ||
                                                      f.Email.Contains(searchString));
            }

            if (statusFilter.HasValue)
            {
                fornecedores = fornecedores.Where(f => f.Status == statusFilter.Value);
            }

            return View(await fornecedores.OrderBy(f => f.Nome).ToListAsync());
        }

        // GET: Fornecedores/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fornecedor = await _context.Fornecedores
                .Include(f => f.ProdutosFornecidos)
                    .ThenInclude(pf => pf.Produto)
                .Include(f => f.PedidosCompra)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (fornecedor == null)
            {
                return NotFound();
            }

            return View(fornecedor);
        }

        // GET: Fornecedores/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Fornecedores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nome,NomeFantasia,CNPJ,InscricaoEstadual,Endereco,Bairro,Cidade,Estado,CEP,Telefone,Celular,Email,NomeContato,Status,Observacoes")] Fornecedor fornecedor)
        {
            if (ModelState.IsValid)
            {
                fornecedor.DataCadastro = DateTime.Now;
                _context.Add(fornecedor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Fornecedor cadastrado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            return View(fornecedor);
        }

        // GET: Fornecedores/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fornecedor = await _context.Fornecedores.FindAsync(id);
            if (fornecedor == null)
            {
                return NotFound();
            }
            return View(fornecedor);
        }

        // POST: Fornecedores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,NomeFantasia,CNPJ,InscricaoEstadual,Endereco,Bairro,Cidade,Estado,CEP,Telefone,Celular,Email,NomeContato,Status,Observacoes")] Fornecedor fornecedor)
        {
            if (id != fornecedor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    fornecedor.UltimaAtualizacao = DateTime.Now;
                    _context.Update(fornecedor);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Fornecedor atualizado com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FornecedorExists(fornecedor.Id))
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
            return View(fornecedor);
        }

        // GET: Fornecedores/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fornecedor = await _context.Fornecedores
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fornecedor == null)
            {
                return NotFound();
            }

            return View(fornecedor);
        }

        // POST: Fornecedores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fornecedor = await _context.Fornecedores.FindAsync(id);
            if (fornecedor != null)
            {
                _context.Fornecedores.Remove(fornecedor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Fornecedor excluído com sucesso!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool FornecedorExists(int id)
        {
            return _context.Fornecedores.Any(e => e.Id == id);
        }
    }
}
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
    public class ProdutosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProdutosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Produtos
        public async Task<IActionResult> Index(string? searchString, StatusProduto? statusFilter)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["StatusFilter"] = statusFilter;

            var produtos = _context.Produtos
                .Include(p => p.Fornecedores)
                    .ThenInclude(pf => pf.Fornecedor)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                produtos = produtos.Where(p => p.Nome.Contains(searchString) ||
                                             p.Codigo.Contains(searchString) ||
                                             (p.Descricao != null && p.Descricao.Contains(searchString)));
            }

            if (statusFilter.HasValue)
            {
                produtos = produtos.Where(p => p.Status == statusFilter.Value);
            }

            return View(await produtos.OrderBy(p => p.Nome).ToListAsync());
        }

        // GET: Produtos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produto = await _context.Produtos
                .Include(p => p.Fornecedores)
                    .ThenInclude(pf => pf.Fornecedor)
                .Include(p => p.ItensPedidoCompra)
                .Include(p => p.ItensNotaFiscal)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (produto == null)
            {
                return NotFound();
            }

            return View(produto);
        }

        // GET: Produtos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Produtos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Codigo,Nome,Descricao,UnidadeMedida,EstoqueMinimo,Status,Observacoes")] Produto produto)
        {
            if (ModelState.IsValid)
            {
                produto.DataCadastro = DateTime.Now;
                _context.Add(produto);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Produto cadastrado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            return View(produto);
        }

        // GET: Produtos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null)
            {
                return NotFound();
            }
            return View(produto);
        }

        // POST: Produtos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Codigo,Nome,Descricao,UnidadeMedida,EstoqueMinimo,Status,Observacoes")] Produto produto)
        {
            if (id != produto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    produto.UltimaAtualizacao = DateTime.Now;
                    _context.Update(produto);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Produto atualizado com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProdutoExists(produto.Id))
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
            return View(produto);
        }

        // GET: Produtos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produto = await _context.Produtos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (produto == null)
            {
                return NotFound();
            }

            return View(produto);
        }

        // POST: Produtos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto != null)
            {
                _context.Produtos.Remove(produto);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Produto excluído com sucesso!";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Produtos/GerenciarFornecedores/5
        public async Task<IActionResult> GerenciarFornecedores(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produto = await _context.Produtos
                .Include(p => p.Fornecedores)
                    .ThenInclude(pf => pf.Fornecedor)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (produto == null)
            {
                return NotFound();
            }

            ViewBag.FornecedoresDisponiveis = await _context.Fornecedores
                .Where(f => f.Status == StatusFornecedor.Ativo)
                .OrderBy(f => f.Nome)
                .ToListAsync();

            return View(produto);
        }

        // POST: Produtos/AdicionarFornecedor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarFornecedor(int produtoId, int fornecedorId, decimal preco, string? codigoFornecedor, decimal? quantidadeEmbalagem, string? modoFaturamento, DateTime? dataValidade, string? condicaoPagamento, int? prazoEntregaDias, string? observacoes)
        {
            var produtoFornecedor = new ProdutoFornecedor
            {
                ProdutoId = produtoId,
                FornecedorId = fornecedorId,
                Preco = preco,
                CodigoFornecedor = codigoFornecedor,
                QuantidadeEmbalagem = quantidadeEmbalagem,
                ModoFaturamento = modoFaturamento,
                DataAtualizacao = DateTime.Now,
                DataValidade = dataValidade,
                CondicaoPagamento = condicaoPagamento,
                PrazoEntregaDias = prazoEntregaDias,
                Observacoes = observacoes,
                Status = StatusProdutoFornecedor.Ativo
            };

            _context.ProdutosFornecedores.Add(produtoFornecedor);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Fornecedor adicionado ao produto com sucesso!";
            return RedirectToAction("GerenciarFornecedores", new { id = produtoId });
        }

        // POST: Produtos/RemoverFornecedor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoverFornecedor(int produtoFornecedorId, int produtoId)
        {
            var produtoFornecedor = await _context.ProdutosFornecedores.FindAsync(produtoFornecedorId);
            if (produtoFornecedor != null)
            {
                _context.ProdutosFornecedores.Remove(produtoFornecedor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Fornecedor removido do produto com sucesso!";
            }
            return RedirectToAction("GerenciarFornecedores", new { id = produtoId });
        }

        private bool ProdutoExists(int id)
        {
            return _context.Produtos.Any(e => e.Id == id);
        }
    }
}
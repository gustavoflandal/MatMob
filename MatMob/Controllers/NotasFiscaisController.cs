using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MatMob.Data;
using MatMob.Models.Entities;
using MatMob.ViewModels;
using MatMob.Extensions;
using MatMob.Services;

namespace MatMob.Controllers
{
    public class NotasFiscaisController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EstoqueService _estoqueService;
        private readonly IAuditService _auditService;

        public NotasFiscaisController(ApplicationDbContext context, EstoqueService estoqueService, IAuditService auditService)
        {
            _context = context;
            _estoqueService = estoqueService;
            _auditService = auditService;
        }

        // GET: NotasFiscais
        public async Task<IActionResult> Index(string? searchString, DateTime? dataInicio, DateTime? dataFim)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["DataInicio"] = dataInicio?.ToString("yyyy-MM-dd");
            ViewData["DataFim"] = dataFim?.ToString("yyyy-MM-dd");

            var notasFiscais = _context.NotasFiscais
                .Include(n => n.Fornecedor)
                .Include(n => n.PedidoCompra)
                .Include(n => n.Itens)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                notasFiscais = notasFiscais.Where(n =>
                    n.NumeroNF.Contains(searchString) ||
                    n.Serie.Contains(searchString) ||
                    (n.Fornecedor != null && n.Fornecedor.Nome.Contains(searchString)));
            }

            if (dataInicio.HasValue)
            {
                notasFiscais = notasFiscais.Where(n => n.DataEmissao >= dataInicio.Value);
            }

            if (dataFim.HasValue)
            {
                notasFiscais = notasFiscais.Where(n => n.DataEmissao <= dataFim.Value);
            }

            var notasList = await notasFiscais
                .OrderByDescending(n => n.DataEmissao)
                .ToListAsync();

            // Estatísticas
            ViewBag.TotalNotas = notasList.Count;
            ViewBag.ValorTotal = notasList.Sum(n => n.ValorTotal);
            ViewBag.TotalItens = notasList.Sum(n => n.Itens?.Count ?? 0);

            // Registrar auditoria
            await _auditService.LogViewAsync(notasList, $"Visualizou lista de notas fiscais - Filtros: {(searchString ?? "Nenhum")}, Período: {(dataInicio?.ToString("dd/MM/yyyy") ?? "N/A")} a {(dataFim?.ToString("dd/MM/yyyy") ?? "N/A")}");

            return View(notasList);
        }

        // GET: NotasFiscais/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notaFiscal = await _context.NotasFiscais
                .Include(n => n.Fornecedor)
                .Include(n => n.PedidoCompra)
                .Include(n => n.Itens)
                    .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (notaFiscal == null)
            {
                return NotFound();
            }

            var viewModel = new NotaFiscalViewModel
            {
                Id = notaFiscal.Id,
                NumeroNF = notaFiscal.NumeroNF,
                Serie = notaFiscal.Serie,
                DataEmissao = notaFiscal.DataEmissao,
                ChaveAcesso = notaFiscal.ChaveAcesso ?? "",
                PedidoCompraId = notaFiscal.PedidoCompraId,
                FornecedorId = notaFiscal.FornecedorId,
                ValorProdutos = notaFiscal.ValorProdutos ?? 0,
                ValorICMS = notaFiscal.ValorICMS ?? 0,
                ValorIPI = notaFiscal.ValorIPI ?? 0,
                ValorPIS = notaFiscal.ValorPIS ?? 0,
                ValorCOFINS = notaFiscal.ValorCOFINS ?? 0,
                Observacoes = notaFiscal.Observacoes ?? "",
                Fornecedor = notaFiscal.Fornecedor ?? new Fornecedor(),
                PedidoCompra = notaFiscal.PedidoCompra ?? new PedidoCompra(),
                Itens = notaFiscal.Itens?.Select(i => new ItemNotaFiscalViewModel
                {
                    Id = i.Id,
                    ProdutoId = i.ProdutoId,
                    ItemPedidoCompraId = i.ItemPedidoCompraId,
                    Quantidade = i.QuantidadeRecebida,
                    ValorUnitario = i.PrecoUnitario,
                    Observacoes = i.Observacoes ?? "",
                    Produto = i.Produto ?? new Produto()
                }).ToList() ?? new List<ItemNotaFiscalViewModel>()
            };

            // Registrar auditoria
            await _auditService.LogViewAsync(notaFiscal, $"Visualizou detalhes da nota fiscal {notaFiscal.NumeroNF}");

            return View(viewModel);
        }

        // GET: NotasFiscais/GetPedidosPorFornecedor/5
        public async Task<IActionResult> GetPedidosPorFornecedor(int fornecedorId)
        {
            var pedidos = await _context.PedidosCompra
                .Where(p => p.FornecedorId == fornecedorId &&
                           (p.Status == StatusPedidoCompra.Aprovado ||
                            p.Status == StatusPedidoCompra.ParcialmenteRecebido))
                .Include(p => p.Fornecedor)
                .Include(p => p.Itens)
                .OrderBy(p => p.NumeroPedido)
                .Select(p => new {
                    p.Id,
                    p.NumeroPedido,
                    FornecedorNome = p.Fornecedor != null ? p.Fornecedor.Nome : "Fornecedor não informado",
                    ItensPendentes = p.Itens.Count(i => i.QuantidadeRecebida < i.QuantidadeSolicitada),
                    ValorTotal = p.ValorTotal ?? 0m
                })
                .ToListAsync();

            return Json(pedidos);
        }

        // GET: NotasFiscais/Create
        public async Task<IActionResult> Create(int? pedidoCompraId = null, int? fornecedorId = null)
        {
            await CarregarViewData(pedidoCompraId, fornecedorId);
            var viewModel = new NotaFiscalViewModel
            {
                DataEmissao = DateTime.Now
            };

            if (pedidoCompraId.HasValue)
            {
                var pedido = await _context.PedidosCompra
                    .Include(p => p.Fornecedor)
                    .Include(p => p.Itens)
                        .ThenInclude(i => i.Produto)
                    .FirstOrDefaultAsync(p => p.Id == pedidoCompraId.Value);

                if (pedido != null)
                {
                    viewModel.FornecedorId = pedido.FornecedorId;
                    viewModel.Fornecedor = pedido.Fornecedor ?? new Fornecedor();
                    viewModel.PedidoCompraId = pedido.Id;
                    viewModel.PedidoCompra = pedido;

                    viewModel.Itens = pedido.Itens
                        .Where(i => i.QuantidadeRecebida < i.QuantidadeSolicitada)
                        .Select(i => new ItemNotaFiscalViewModel
                        {
                            ProdutoId = i.ProdutoId,
                            Produto = i.Produto ?? new Produto(),
                            ItemPedidoCompraId = i.Id,
                            Quantidade = (int)(i.QuantidadeSolicitada - i.QuantidadeRecebida),
                            ValorUnitario = i.PrecoUnitario
                        }).ToList();
                }
            }

            return View(viewModel);
        }

        // POST: NotasFiscais/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NotaFiscalViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    var notaFiscal = new NotaFiscal
                    {
                        NumeroNF = viewModel.NumeroNF,
                        Serie = viewModel.Serie,
                        DataEmissao = viewModel.DataEmissao,
                        DataEntrada = DateTime.Now,
                        ChaveAcesso = viewModel.ChaveAcesso,
                        FornecedorId = viewModel.FornecedorId,
                        PedidoCompraId = viewModel.PedidoCompraId.GetValueOrDefault(),
                        ValorProdutos = viewModel.ValorProdutos,
                        ValorICMS = viewModel.ValorICMS,
                        ValorIPI = viewModel.ValorIPI,
                        ValorPIS = viewModel.ValorPIS,
                        ValorCOFINS = viewModel.ValorCOFINS,
                        Observacoes = viewModel.Observacoes
                    };

                    _context.Add(notaFiscal);
                    await _context.SaveChangesAsync();

                    // Adicionar itens da nota fiscal
                    foreach (var item in viewModel.Itens ?? Enumerable.Empty<ItemNotaFiscalViewModel>())
                    {
                        if (item.Quantidade > 0)
                        {
                            var itemNotaFiscal = new ItemNotaFiscal
                            {
                                NotaFiscalId = notaFiscal.Id,
                                ProdutoId = item.ProdutoId,
                                ItemPedidoCompraId = item.ItemPedidoCompraId,
                                QuantidadeRecebida = item.Quantidade,
                                PrecoUnitario = item.ValorUnitario,
                                Observacoes = item.Observacoes
                            };

                            _context.Add(itemNotaFiscal);
                            await _context.SaveChangesAsync();

                            // Atualizar estoque
                            var sucesso = await _estoqueService.RegistrarEntradaEstoqueAsync(itemNotaFiscal);
                            if (!sucesso)
                            {
                                await transaction.RollbackAsync();
                                ModelState.AddModelError(string.Empty, "Erro ao registrar entrada no estoque.");
                                await CarregarViewData(notaFiscal.PedidoCompraId, notaFiscal.FornecedorId);
                                return View(viewModel);
                            }
                        }
                    }

                    await transaction.CommitAsync();

                    // Registrar auditoria
                    await _auditService.LogCreateAsync(notaFiscal, $"Criada nota fiscal {notaFiscal.NumeroNF}");

                    TempData["Success"] = "Nota fiscal cadastrada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, "Ocorreu um erro ao salvar a nota fiscal: " + ex.Message);
                }
            }
            
            await CarregarViewData(viewModel.PedidoCompraId, viewModel.FornecedorId);
            return View(viewModel);
        }

        // GET: NotasFiscais/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notaFiscal = await _context.NotasFiscais
                .Include(n => n.Fornecedor)
                .Include(n => n.PedidoCompra)
                .Include(n => n.Itens)
                    .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (notaFiscal == null)
            {
                return NotFound();
            }

            var viewModel = new NotaFiscalViewModel
            {
                Id = notaFiscal.Id,
                NumeroNF = notaFiscal.NumeroNF,
                Serie = notaFiscal.Serie,
                DataEmissao = notaFiscal.DataEmissao,
                ChaveAcesso = notaFiscal.ChaveAcesso ?? "",
                FornecedorId = notaFiscal.FornecedorId,
                Fornecedor = notaFiscal.Fornecedor ?? new Fornecedor(),
                PedidoCompraId = notaFiscal.PedidoCompraId,
                PedidoCompra = notaFiscal.PedidoCompra ?? new PedidoCompra(),
                ValorProdutos = notaFiscal.ValorProdutos ?? 0,
                ValorICMS = notaFiscal.ValorICMS ?? 0,
                ValorIPI = notaFiscal.ValorIPI ?? 0,
                ValorPIS = notaFiscal.ValorPIS ?? 0,
                ValorCOFINS = notaFiscal.ValorCOFINS ?? 0,
                Observacoes = notaFiscal.Observacoes ?? "",
                Itens = notaFiscal.Itens?.Select(i => new ItemNotaFiscalViewModel
                {
                    Id = i.Id,
                    ProdutoId = i.ProdutoId,
                    Produto = i.Produto ?? new Produto(),
                    ItemPedidoCompraId = i.ItemPedidoCompraId,
                    Quantidade = i.QuantidadeRecebida,
                    ValorUnitario = i.PrecoUnitario,
                    Observacoes = i.Observacoes ?? ""
                }).ToList() ?? new List<ItemNotaFiscalViewModel>()
            };

            await CarregarViewData(notaFiscal.PedidoCompraId, notaFiscal.FornecedorId);
            return View(viewModel);
        }

        // POST: NotasFiscais/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NotaFiscalViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    var notaFiscal = await _context.NotasFiscais
                        .Include(n => n.Itens)
                        .FirstOrDefaultAsync(n => n.Id == id);

                    if (notaFiscal == null)
                    {
                        return NotFound();
                    }

                    // Capturar estado antigo para auditoria
                    var oldNotaFiscal = new NotaFiscal
                    {
                        Id = notaFiscal.Id,
                        NumeroNF = notaFiscal.NumeroNF,
                        Serie = notaFiscal.Serie,
                        DataEmissao = notaFiscal.DataEmissao,
                        ChaveAcesso = notaFiscal.ChaveAcesso,
                        FornecedorId = notaFiscal.FornecedorId,
                        PedidoCompraId = notaFiscal.PedidoCompraId,
                        ValorProdutos = notaFiscal.ValorProdutos,
                        ValorICMS = notaFiscal.ValorICMS,
                        ValorIPI = notaFiscal.ValorIPI,
                        ValorPIS = notaFiscal.ValorPIS,
                        ValorCOFINS = notaFiscal.ValorCOFINS,
                        Observacoes = notaFiscal.Observacoes
                    };

                    // Reverter o estoque para os itens antigos
                    foreach (var item in notaFiscal.Itens)
                    {
                        var produto = await _context.Produtos.FindAsync(item.ProdutoId);
                        if (produto != null)
                        {
                            produto.EstoqueAtual -= item.QuantidadeRecebida;
                            if (produto.EstoqueAtual < 0) produto.EstoqueAtual = 0;
                        }

                        // Reverter quantidade recebida no item do pedido, se existir
                        if (item.ItemPedidoCompraId > 0)
                        {
                            var itemPedido = await _context.ItensPedidoCompra.FindAsync(item.ItemPedidoCompraId);
                            if (itemPedido != null)
                            {
                                itemPedido.QuantidadeRecebida = Math.Max(0, itemPedido.QuantidadeRecebida - item.QuantidadeRecebida);
                            }
                        }
                    }

                    // Remover itens antigos
                    _context.ItensNotaFiscal.RemoveRange(notaFiscal.Itens);
                    await _context.SaveChangesAsync();

                    // Atualizar dados da nota fiscal
                    notaFiscal.NumeroNF = viewModel.NumeroNF;
                    notaFiscal.Serie = viewModel.Serie;
                    notaFiscal.DataEmissao = viewModel.DataEmissao;
                    notaFiscal.ChaveAcesso = viewModel.ChaveAcesso;
                    notaFiscal.FornecedorId = viewModel.FornecedorId;
                    notaFiscal.PedidoCompraId = viewModel.PedidoCompraId.GetValueOrDefault();
                    notaFiscal.ValorProdutos = viewModel.ValorProdutos;
                    notaFiscal.ValorICMS = viewModel.ValorICMS;
                    notaFiscal.ValorIPI = viewModel.ValorIPI;
                    notaFiscal.ValorPIS = viewModel.ValorPIS;
                    notaFiscal.ValorCOFINS = viewModel.ValorCOFINS;
                    notaFiscal.Observacoes = viewModel.Observacoes;

                    _context.Update(notaFiscal);

                    // Adicionar novos itens
                    foreach (var item in viewModel.Itens ?? Enumerable.Empty<ItemNotaFiscalViewModel>())
                    {
                        if (item.Quantidade > 0)
                        {
                            var itemNotaFiscal = new ItemNotaFiscal
                            {
                                NotaFiscalId = notaFiscal.Id,
                                ProdutoId = item.ProdutoId,
                                ItemPedidoCompraId = item.ItemPedidoCompraId,
                                QuantidadeRecebida = item.Quantidade,
                                PrecoUnitario = item.ValorUnitario,
                                Observacoes = item.Observacoes
                            };

                            _context.Add(itemNotaFiscal);
                            await _context.SaveChangesAsync();

                            // Atualizar estoque
                            var sucesso = await _estoqueService.RegistrarEntradaEstoqueAsync(itemNotaFiscal);
                            if (!sucesso)
                            {
                                await transaction.RollbackAsync();
                                ModelState.AddModelError(string.Empty, "Erro ao atualizar o estoque.");
                                await CarregarViewData(notaFiscal.PedidoCompraId, notaFiscal.FornecedorId);
                                return View(viewModel);
                            }

                            // Atualizar quantidade recebida no item do pedido, se existir
                            if (item.ItemPedidoCompraId > 0)
                            {
                                var itemPedido = await _context.ItensPedidoCompra.FindAsync(item.ItemPedidoCompraId);
                                if (itemPedido != null)
                                {
                                    itemPedido.QuantidadeRecebida = itemPedido.QuantidadeRecebida + item.Quantidade;
                                    if (itemPedido.QuantidadeRecebida < 0) itemPedido.QuantidadeRecebida = 0;
                                }
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Registrar auditoria
                    await _auditService.LogUpdateAsync(oldNotaFiscal, notaFiscal, $"Atualizada nota fiscal {notaFiscal.NumeroNF}");
                    
                    TempData["Success"] = "Nota fiscal atualizada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, "Ocorreu um erro ao atualizar a nota fiscal: " + ex.Message);
                }
            }
            
            await CarregarViewData(viewModel.PedidoCompraId, viewModel.FornecedorId);
            return View(viewModel);
        }

        // GET: NotasFiscais/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notaFiscal = await _context.NotasFiscais
                .Include(n => n.Fornecedor)
                .Include(n => n.PedidoCompra)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (notaFiscal == null)
            {
                return NotFound();
            }

            return View(notaFiscal);
        }

        // POST: NotasFiscais/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var notaFiscal = await _context.NotasFiscais
                    .Include(n => n.Itens)
                    .FirstOrDefaultAsync(n => n.Id == id);

                if (notaFiscal == null)
                {
                    return NotFound();
                }

                // Reverter o estoque para cada item da nota fiscal
                foreach (var item in notaFiscal.Itens)
                {
                    var produto = await _context.Produtos.FindAsync(item.ProdutoId);
                    if (produto != null)
                    {
                        produto.EstoqueAtual -= item.QuantidadeRecebida;
                        if (produto.EstoqueAtual < 0) produto.EstoqueAtual = 0;

                        // Atualizar a quantidade recebida no item do pedido
                        if (item.ItemPedidoCompraId > 0)
                        {
                            var itemPedido = await _context.ItensPedidoCompra.FindAsync(item.ItemPedidoCompraId);
                            if (itemPedido != null)
                            {
                                itemPedido.QuantidadeRecebida -= item.QuantidadeRecebida;
                                if (itemPedido.QuantidadeRecebida < 0) itemPedido.QuantidadeRecebida = 0;

                                // Atualizar status do pedido se necessário
                                var pedido = await _context.PedidosCompra
                                    .Include(p => p.Itens)
                                    .FirstOrDefaultAsync(p => p.Id == itemPedido.PedidoCompraId);
                                    
                                if (pedido != null)
                                {
                                    if (pedido.Itens.All(i => i.QuantidadeRecebida <= 0))
                                    {
                                        pedido.Status = StatusPedidoCompra.Aprovado;
                                        pedido.DataEntrega = null;
                                    }
                                    else if (pedido.Itens.All(i => i.QuantidadeRecebida >= i.QuantidadeSolicitada))
                                    {
                                        pedido.Status = StatusPedidoCompra.TotalmenteRecebido;
                                        pedido.DataEntrega = DateTime.Now;
                                    }
                                    else
                                    {
                                        pedido.Status = StatusPedidoCompra.ParcialmenteRecebido;
                                    }
                                }
                            }
                        }
                    }
                }

                // Remover a nota fiscal e seus itens
                _context.NotasFiscais.Remove(notaFiscal);
                await _context.SaveChangesAsync();
                
                await transaction.CommitAsync();

                // Registrar auditoria
                await _auditService.LogDeleteAsync(notaFiscal, $"Excluída nota fiscal {notaFiscal.NumeroNF}");

                TempData["Success"] = "Nota fiscal excluída com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Ocorreu um erro ao excluir a nota fiscal: " + ex.Message);
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        private async Task CarregarViewData(int? pedidoCompraId = null, int? fornecedorId = null)
        {
            ViewBag.Fornecedores = await _context.Fornecedores
                .OrderBy(f => f.Nome)
                .ToListAsync();

            if (fornecedorId.HasValue)
            {
                ViewBag.PedidosCompra = await _context.PedidosCompra
                    .Where(p => p.FornecedorId == fornecedorId.Value && 
                              (p.Status == StatusPedidoCompra.Aprovado || 
                               p.Status == StatusPedidoCompra.ParcialmenteRecebido))
                    .Include(p => p.Itens)
                    .ThenInclude(i => i.Produto)
                    .OrderBy(p => p.NumeroPedido)
                    .ToListAsync();

                if (pedidoCompraId.HasValue)
                {
                    var pedido = await _context.PedidosCompra
                        .Include(p => p.Itens)
                        .ThenInclude(i => i.Produto)
                        .FirstOrDefaultAsync(p => p.Id == pedidoCompraId.Value);

                    if (pedido != null)
                    {
                        ViewBag.ItensPedido = pedido.Itens
                            .Where(i => i.QuantidadeRecebida < i.QuantidadeSolicitada)
                            .ToList();
                    }
                }
            }

            ViewBag.Produtos = await _context.Produtos
                .OrderBy(p => p.Nome)
                .ToListAsync();
        }

        private bool NotaFiscalExists(int id)
        {
            return _context.NotasFiscais.Any(e => e.Id == id);
        }
    }
}

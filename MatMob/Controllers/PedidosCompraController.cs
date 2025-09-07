using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MatMob.Data;
using MatMob.Models.Entities;
using MatMob.Extensions;
using MatMob.Models.ViewModels;
using Microsoft.Extensions.Logging;
using MatMob.Services;

namespace MatMob.Controllers
{
    public class PedidosCompraController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PedidosCompraController> _logger;
        private readonly IAuditService _auditService;

        public PedidosCompraController(ApplicationDbContext context, ILogger<PedidosCompraController> logger, IAuditService auditService)
        {
            _context = context;
            _logger = logger;
            _auditService = auditService;
        }

        // GET: PedidosCompra
        public async Task<IActionResult> Index(string? searchString, StatusPedidoCompra? statusFilter, PrioridadePedidoCompra? prioridadeFilter)
        {
            await _auditService.LogAsync(
                action: AuditActions.VIEW,
                entityName: "PedidoCompra",
                description: "Visualização da lista de pedidos de compra"
            );

            ViewData["CurrentFilter"] = searchString;
            ViewData["StatusFilter"] = statusFilter;
            ViewData["PrioridadeFilter"] = prioridadeFilter;

            // Carregar lista de produtos para o modal
            ViewBag.Produtos = await _context.Produtos
                .Where(p => p.Status == StatusProduto.Ativo)
                .OrderBy(p => p.Nome)
                .Select(p => new
                {
                    id = p.Id,
                    nome = p.Nome,
                    codigo = p.Codigo,
                    unidadeMedida = p.UnidadeMedida
                })
                .ToListAsync();

            var pedidosCompra = _context.PedidosCompra
                .Include(p => p.Fornecedor)
                .Include(p => p.Itens)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                pedidosCompra = pedidosCompra.Where(p => p.NumeroPedido.Contains(searchString) ||
                                                        (p.Fornecedor != null && p.Fornecedor.Nome.Contains(searchString)));
            }

            if (statusFilter.HasValue)
            {
                pedidosCompra = pedidosCompra.Where(p => p.Status == statusFilter.Value);
            }

            if (prioridadeFilter.HasValue)
            {
                pedidosCompra = pedidosCompra.Where(p => p.Prioridade == prioridadeFilter.Value);
            }

            return View(await pedidosCompra.OrderByDescending(p => p.DataPedido).ToListAsync());
        }

        // GET: PedidosCompra/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pedidoCompra = await _context.PedidosCompra
                .Include(p => p.Fornecedor)
                .Include(p => p.Itens)
                    .ThenInclude(i => i.Produto)
                .Include(p => p.NotasFiscais)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (pedidoCompra == null)
            {
                return NotFound();
            }

            // Carregar lista de produtos para o modal de adicionar item
            ViewBag.Produtos = await _context.Produtos
                .Where(p => p.Status == StatusProduto.Ativo)
                .OrderBy(p => p.Nome)
                .Select(p => new
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    Codigo = p.Codigo,
                    UnidadeMedida = p.UnidadeMedida,
                    Descricao = p.Descricao
                })
                .ToListAsync();

            return View(pedidoCompra);
        }

        // GET: PedidosCompra/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Fornecedores = await _context.Fornecedores
                .Where(f => f.Status == StatusFornecedor.Ativo)
                .OrderBy(f => f.Nome)
                .Select(f => new SelectListItem
                {
                    Value = f.Id.ToString(),
                    Text = $"{f.Nome} - {f.CNPJ}"
                })
                .ToListAsync();

            ViewBag.Produtos = await _context.Produtos
                .Where(p => p.Status == StatusProduto.Ativo)
                .OrderBy(p => p.Nome)
                .Select(p => new
                {
                    id = p.Id,
                    nome = p.Nome,
                    codigo = p.Codigo,
                    unidadeMedida = p.UnidadeMedida
                })
                .ToListAsync();

            var viewModel = new PedidoCompraViewModel();
            return View(viewModel);
        }

        // POST: PedidosCompra/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PedidoCompraViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var pedidoCompra = new PedidoCompra
                {
                    FornecedorId = viewModel.FornecedorId,
                    Prioridade = viewModel.Prioridade,
                    DataPrevistaEntrega = viewModel.DataPrevistaEntrega,
                    CondicaoPagamento = viewModel.CondicaoPagamento,
                    Observacoes = viewModel.Observacoes,
                    NumeroPedido = await GerarNumeroPedido(),
                    DataPedido = DateTime.Now,
                    Status = StatusPedidoCompra.Aberto,
                    DataCadastro = DateTime.Now
                };

                _context.Add(pedidoCompra);
                await _context.SaveChangesAsync();

                // Processar itens do pedido se houver
                if (viewModel.Itens != null && viewModel.Itens.Any())
                {
                    foreach (var item in viewModel.Itens)
                    {
                        if (item.ProdutoId > 0 && item.QuantidadeSolicitada > 0 && item.PrecoUnitario > 0)
                        {
                            var itemPedidoCompra = new ItemPedidoCompra
                            {
                                PedidoCompraId = pedidoCompra.Id,
                                ProdutoId = item.ProdutoId,
                                QuantidadeSolicitada = item.QuantidadeSolicitada,
                                QuantidadeRecebida = 0,
                                PrecoUnitario = item.PrecoUnitario,
                                Observacoes = item.Observacoes
                            };

                            _context.ItensPedidoCompra.Add(itemPedidoCompra);
                        }
                    }
                    await _context.SaveChangesAsync();

                    // Atualizar valor total do pedido
                    await AtualizarValorTotalPedido(pedidoCompra.Id);
                }

                // Registrar auditoria
                await _auditService.LogCreateAsync(pedidoCompra, $"Criado pedido de compra {pedidoCompra.NumeroPedido}");

                TempData["Success"] = "Pedido de compra criado com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Fornecedores = await _context.Fornecedores
                .Where(f => f.Status == StatusFornecedor.Ativo)
                .OrderBy(f => f.Nome)
                .Select(f => new SelectListItem
                {
                    Value = f.Id.ToString(),
                    Text = $"{f.Nome} - {f.CNPJ}"
                })
                .ToListAsync();

            ViewBag.Produtos = await _context.Produtos
                .Where(p => p.Status == StatusProduto.Ativo)
                .OrderBy(p => p.Nome)
                .Select(p => new
                {
                    id = p.Id,
                    nome = p.Nome,
                    codigo = p.Codigo,
                    unidadeMedida = p.UnidadeMedida
                })
                .ToListAsync();

            return View(viewModel);
        }

        // GET: PedidosCompra/AdicionarItens/5
        public async Task<IActionResult> AdicionarItens(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pedidoCompra = await _context.PedidosCompra
                .Include(p => p.Fornecedor)
                .Include(p => p.Itens)
                    .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedidoCompra == null)
            {
                return NotFound();
            }

            ViewBag.ProdutosDisponiveis = await _context.Produtos
                .Where(p => p.Status == StatusProduto.Ativo)
                .OrderBy(p => p.Nome)
                .ToListAsync();

            return View(pedidoCompra);
        }

        // POST: PedidosCompra/AdicionarItemAjax
        [HttpPost]
        public async Task<IActionResult> AdicionarItemAjax(int pedidoCompraId, int produtoId, decimal quantidadeSolicitada, decimal precoUnitario, string? observacoes)
        {
            try
            {
                var itemPedidoCompra = new ItemPedidoCompra
                {
                    PedidoCompraId = pedidoCompraId,
                    ProdutoId = produtoId,
                    QuantidadeSolicitada = quantidadeSolicitada,
                    QuantidadeRecebida = 0,
                    PrecoUnitario = precoUnitario,
                    Observacoes = observacoes
                };

                _context.ItensPedidoCompra.Add(itemPedidoCompra);
                await _context.SaveChangesAsync();

                // Atualizar valor total do pedido
                await AtualizarValorTotalPedido(pedidoCompraId);

                return Json(new { success = true, message = "Item adicionado com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro ao adicionar item: " + ex.Message });
            }
        }

        // POST: PedidosCompra/RemoverItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoverItem(int itemId, int pedidoCompraId)
        {
            var item = await _context.ItensPedidoCompra.FindAsync(itemId);
            if (item != null)
            {
                _context.ItensPedidoCompra.Remove(item);
                await _context.SaveChangesAsync();

                // Atualizar valor total do pedido
                await AtualizarValorTotalPedido(pedidoCompraId);

                TempData["Success"] = "Item removido do pedido com sucesso!";
            }
            return RedirectToAction("Details", new { id = pedidoCompraId });
        }

        // POST: PedidosCompra/ExcluirItem/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirItem(int id)
        {
            try
            {
                // Verificar se o token de verificação é válido
                if (!Request.Headers.ContainsKey("RequestVerificationToken") || 
                    !Request.Headers["RequestVerificationToken"].Any())
                {
                    return Json(new { success = false, message = "Token de segurança inválido." });
                }

                var item = await _context.ItensPedidoCompra
                    .Include(i => i.PedidoCompra)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (item == null)
                {
                    return Json(new { success = false, message = "Item não encontrado." });
                }

                // Verificar se o pedido permite exclusão
                if (item.PedidoCompra?.Status != StatusPedidoCompra.Aberto)
                {
                    return Json(new { 
                        success = false, 
                        message = "Não é possível excluir itens de um pedido que não está em aberto." 
                    });
                }

                int pedidoId = item.PedidoCompraId;
                
                // Iniciar transação para garantir a integridade dos dados
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    // Remover o item
                    _context.ItensPedidoCompra.Remove(item);
                    await _context.SaveChangesAsync();

                    // Atualizar o valor total do pedido
                    await AtualizarValorTotalPedido(pedidoId);
                    
                    // Commit da transação
                    await transaction.CommitAsync();

                    return Json(new { 
                        success = true, 
                        message = "Item excluído com sucesso!" 
                    });
                }
                catch (Exception)
                {
                    // Rollback em caso de erro
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                // Log do erro (você pode implementar um serviço de logging aqui)
                _logger?.LogError(ex, "Erro ao excluir item do pedido {ItemId}", id);
                
                return Json(new { 
                    success = false, 
                    message = "Ocorreu um erro ao excluir o item. Por favor, tente novamente." 
                });
            }
        }

        // POST: PedidosCompra/AprovarPedido
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AprovarPedido(int id)
        {
            var pedidoCompra = await _context.PedidosCompra.FindAsync(id);
            if (pedidoCompra != null)
            {
                // Capturar estado antigo para auditoria
                var oldPedidoCompra = new PedidoCompra
                {
                    Id = pedidoCompra.Id,
                    FornecedorId = pedidoCompra.FornecedorId,
                    NumeroPedido = pedidoCompra.NumeroPedido,
                    Prioridade = pedidoCompra.Prioridade,
                    DataPrevistaEntrega = pedidoCompra.DataPrevistaEntrega,
                    CondicaoPagamento = pedidoCompra.CondicaoPagamento,
                    Observacoes = pedidoCompra.Observacoes,
                    ValorTotal = pedidoCompra.ValorTotal,
                    DataPedido = pedidoCompra.DataPedido,
                    Status = pedidoCompra.Status,
                    DataCadastro = pedidoCompra.DataCadastro,
                    UltimaAtualizacao = pedidoCompra.UltimaAtualizacao
                };

                pedidoCompra.Status = StatusPedidoCompra.Aprovado;
                pedidoCompra.DataAprovacao = DateTime.Now;
                pedidoCompra.UltimaAtualizacao = DateTime.Now;

                _context.Update(pedidoCompra);
                await _context.SaveChangesAsync();

                // Registrar auditoria
                await _auditService.LogUpdateAsync(oldPedidoCompra, pedidoCompra, $"Aprovado pedido de compra {pedidoCompra.NumeroPedido}");

                TempData["Success"] = "Pedido de compra aprovado com sucesso!";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: PedidosCompra/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pedidoCompra = await _context.PedidosCompra
                .Include(p => p.Fornecedor)
                .Include(p => p.Itens)
                    .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedidoCompra == null)
            {
                return NotFound();
            }

            ViewBag.Fornecedores = await _context.Fornecedores
                .Where(f => f.Status == StatusFornecedor.Ativo)
                .OrderBy(f => f.Nome)
                .Select(f => new SelectListItem
                {
                    Value = f.Id.ToString(),
                    Text = $"{f.Nome} - {f.CNPJ}"
                })
                .ToListAsync();

            var viewModel = new PedidoCompraViewModel
            {
                Id = pedidoCompra.Id,
                FornecedorId = pedidoCompra.FornecedorId,
                NumeroPedido = pedidoCompra.NumeroPedido,
                Prioridade = pedidoCompra.Prioridade,
                DataPrevistaEntrega = pedidoCompra.DataPrevistaEntrega,
                CondicaoPagamento = pedidoCompra.CondicaoPagamento,
                Observacoes = pedidoCompra.Observacoes,
                ValorTotal = pedidoCompra.ValorTotal ?? 0,
                DataPedido = pedidoCompra.DataPedido,
                Status = pedidoCompra.Status,
                DataCadastro = pedidoCompra.DataCadastro,
                UltimaAtualizacao = pedidoCompra.UltimaAtualizacao
            };

            return View(viewModel);
        }

        // POST: PedidosCompra/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PedidoCompraViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var pedidoCompra = await _context.PedidosCompra.FindAsync(id);
                    if (pedidoCompra == null)
                    {
                        return NotFound();
                    }

                    // Capturar estado antigo para auditoria
                    var oldPedidoCompra = new PedidoCompra
                    {
                        Id = pedidoCompra.Id,
                        FornecedorId = pedidoCompra.FornecedorId,
                        NumeroPedido = pedidoCompra.NumeroPedido,
                        Prioridade = pedidoCompra.Prioridade,
                        DataPrevistaEntrega = pedidoCompra.DataPrevistaEntrega,
                        CondicaoPagamento = pedidoCompra.CondicaoPagamento,
                        Observacoes = pedidoCompra.Observacoes,
                        ValorTotal = pedidoCompra.ValorTotal,
                        DataPedido = pedidoCompra.DataPedido,
                        Status = pedidoCompra.Status,
                        DataCadastro = pedidoCompra.DataCadastro,
                        UltimaAtualizacao = pedidoCompra.UltimaAtualizacao
                    };

                    pedidoCompra.FornecedorId = viewModel.FornecedorId;
                    pedidoCompra.Prioridade = viewModel.Prioridade;
                    pedidoCompra.DataPrevistaEntrega = viewModel.DataPrevistaEntrega;
                    pedidoCompra.CondicaoPagamento = viewModel.CondicaoPagamento;
                    pedidoCompra.Observacoes = viewModel.Observacoes;
                    pedidoCompra.UltimaAtualizacao = DateTime.Now;

                    _context.Update(pedidoCompra);
                    await _context.SaveChangesAsync();

                    // Registrar auditoria
                    await _auditService.LogUpdateAsync(oldPedidoCompra, pedidoCompra, $"Atualizado pedido de compra {pedidoCompra.NumeroPedido}");

                    TempData["Success"] = "Pedido de compra atualizado com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PedidoCompraExists(viewModel.Id))
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

            ViewBag.Fornecedores = await _context.Fornecedores
                .Where(f => f.Status == StatusFornecedor.Ativo)
                .OrderBy(f => f.Nome)
                .Select(f => new SelectListItem
                {
                    Value = f.Id.ToString(),
                    Text = $"{f.Nome} - {f.CNPJ}"
                })
                .ToListAsync();

            return View(viewModel);
        }

        // POST: PedidosCompra/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pedidoCompra = await _context.PedidosCompra.FindAsync(id);
            if (pedidoCompra != null)
            {
                // Registrar auditoria antes da exclusão
                await _auditService.LogDeleteAsync(pedidoCompra, $"Excluído pedido de compra {pedidoCompra.NumeroPedido}");

                _context.PedidosCompra.Remove(pedidoCompra);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Pedido de compra excluído com sucesso!";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> GerarNumeroPedido()
        {
            var anoAtual = DateTime.Now.Year;
            var ultimoPedido = await _context.PedidosCompra
                .Where(p => p.NumeroPedido.StartsWith($"PC{anoAtual}"))
                .OrderByDescending(p => p.NumeroPedido)
                .FirstOrDefaultAsync();

            int sequencial = 1;
            if (ultimoPedido != null)
            {
                var numeroStr = ultimoPedido.NumeroPedido.Replace($"PC{anoAtual}", "");
                if (int.TryParse(numeroStr, out int ultimoSequencial))
                {
                    sequencial = ultimoSequencial + 1;
                }
            }

            return $"PC{anoAtual}{sequencial:D4}";
        }

        private async Task AtualizarValorTotalPedido(int pedidoCompraId)
        {
            var valorTotal = await _context.ItensPedidoCompra
                .Where(i => i.PedidoCompraId == pedidoCompraId)
                .SumAsync(i => i.QuantidadeSolicitada * i.PrecoUnitario);

            var pedidoCompra = await _context.PedidosCompra.FindAsync(pedidoCompraId);
            if (pedidoCompra != null)
            {
                pedidoCompra.ValorTotal = valorTotal;
                pedidoCompra.UltimaAtualizacao = DateTime.Now;
                _context.Update(pedidoCompra);
                await _context.SaveChangesAsync();
            }
        }

        private bool PedidoCompraExists(int id)
        {
            return _context.PedidosCompra.Any(e => e.Id == id);
        }

        // GET: PedidosCompra/ObterItem/5
        [HttpGet]
        public async Task<IActionResult> ObterItem(int id)
        {
            try
            {
                var item = await _context.ItensPedidoCompra
                    .Include(i => i.Produto)
                    .Include(i => i.PedidoCompra)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (item == null)
                {
                    return Json(new { success = false, message = "Item não encontrado." });
                }

                return Json(new
                {
                    success = true,
                    item = new
                    {
                        id = item.Id,
                        produtoNome = item.Produto?.Nome,
                        quantidadeSolicitada = item.QuantidadeSolicitada,
                        precoUnitario = item.PrecoUnitario,
                        observacoes = item.Observacoes,
                        pedidoCompraId = item.PedidoCompraId
                    }
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Erro ao obter item {ItemId}", id);
                return Json(new { success = false, message = "Erro ao carregar dados do item." });
            }
        }

        // POST: PedidosCompra/EditarItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarItem(int itemId, int pedidoCompraId, decimal quantidadeSolicitada, decimal precoUnitario, string? observacoes)
        {
            try
            {
                _logger?.LogInformation("Iniciando edição do item {ItemId} do pedido {PedidoCompraId} - Quantidade: {Quantidade}, Preço: {Preco}", 
                    itemId, pedidoCompraId, quantidadeSolicitada, precoUnitario);

                // Validar parâmetros de entrada
                if (itemId <= 0)
                {
                    _logger?.LogWarning("ItemId inválido: {ItemId}", itemId);
                    return Json(new { success = false, message = "ID do item é inválido." });
                }

                if (pedidoCompraId <= 0)
                {
                    _logger?.LogWarning("PedidoCompraId inválido: {PedidoCompraId}", pedidoCompraId);
                    return Json(new { success = false, message = "ID do pedido de compra é inválido." });
                }

                var item = await _context.ItensPedidoCompra
                    .Include(i => i.PedidoCompra)
                    .FirstOrDefaultAsync(i => i.Id == itemId);

                if (item == null)
                {
                    _logger?.LogWarning("Item {ItemId} não encontrado", itemId);
                    return Json(new { success = false, message = "Item não encontrado." });
                }

                // Verificar se o pedido existe
                if (item.PedidoCompra == null)
                {
                    _logger?.LogWarning("Pedido de compra associado ao item {ItemId} não encontrado", itemId);
                    return Json(new { success = false, message = "Pedido de compra associado não encontrado." });
                }

                // Verificar se o pedido permite edição
                if (item.PedidoCompra?.Status != StatusPedidoCompra.Aberto)
                {
                    _logger?.LogWarning("Tentativa de editar item {ItemId} de pedido não aberto. Status: {Status}", itemId, item.PedidoCompra?.Status);
                    return Json(new {
                        success = false,
                        message = "Não é possível editar itens de um pedido que não está em aberto."
                    });
                }

                // Validar dados
                if (quantidadeSolicitada <= 0)
                {
                    _logger?.LogWarning("Quantidade inválida para item {ItemId}: {Quantidade}", itemId, quantidadeSolicitada);
                    return Json(new { success = false, message = "A quantidade solicitada deve ser maior que zero." });
                }

                if (precoUnitario <= 0)
                {
                    _logger?.LogWarning("Preço unitário inválido para item {ItemId}: {Preco}", itemId, precoUnitario);
                    return Json(new { success = false, message = "O preço unitário deve ser maior que zero." });
                }

                _logger?.LogInformation("Atualizando item {ItemId}: Quantidade={Quantidade}, Preco={Preco}", itemId, quantidadeSolicitada, precoUnitario);

                // Atualizar item
                item.QuantidadeSolicitada = quantidadeSolicitada;
                item.PrecoUnitario = precoUnitario;
                item.Observacoes = observacoes ?? string.Empty;

                _context.Update(item);
                var saveResult = await _context.SaveChangesAsync();
                
                _logger?.LogInformation("SaveChanges resultado: {SaveResult} para item {ItemId}", saveResult, itemId);

                if (saveResult > 0)
                {
                    _logger?.LogInformation("Item {ItemId} atualizado com sucesso. Atualizando valor total do pedido {PedidoCompraId}", itemId, pedidoCompraId);

                    // Atualizar valor total do pedido
                    await AtualizarValorTotalPedido(pedidoCompraId);

                    // Registrar auditoria
                    await _auditService.LogUpdateAsync(item, item, $"Atualizado item do pedido de compra {item.PedidoCompra?.NumeroPedido}");

                    _logger?.LogInformation("Edição do item {ItemId} concluída com sucesso", itemId);

                    return Json(new { 
                        success = true, 
                        message = "Item atualizado com sucesso!",
                        data = new {
                            itemId = item.Id,
                            quantidadeSolicitada = item.QuantidadeSolicitada,
                            precoUnitario = item.PrecoUnitario,
                            valorTotal = item.QuantidadeSolicitada * item.PrecoUnitario
                        }
                    });
                }
                else
                {
                    _logger?.LogWarning("Nenhuma alteração foi salva para o item {ItemId}", itemId);
                    return Json(new { success = false, message = "Nenhuma alteração foi detectada." });
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Erro ao editar item {ItemId}. Exception: {Exception}", itemId, ex.ToString());
                return Json(new { 
                    success = false, 
                    message = "Erro interno do servidor. Por favor, tente novamente.",
                    error = ex.Message // Apenas em desenvolvimento
                });
            }
        }
    }
}
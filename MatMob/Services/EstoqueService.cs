using System;
using System.Linq;
using System.Threading.Tasks;
using MatMob.Data;
using MatMob.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace MatMob.Services
{
    public class EstoqueService
    {
        private readonly ApplicationDbContext _context;

        public EstoqueService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RegistrarEntradaEstoqueAsync(ItemNotaFiscal itemNotaFiscal)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Atualizar o estoque do produto
                var produto = await _context.Produtos.FindAsync(itemNotaFiscal.ProdutoId);
                if (produto == null)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                // Atualizar o estoque
                produto.EstoqueAtual += itemNotaFiscal.QuantidadeRecebida;
                produto.UltimaAtualizacao = DateTime.Now;

                // Atualizar a quantidade recebida no item do pedido de compra
                var itemPedido = await _context.ItensPedidoCompra
                    .Include(i => i.PedidoCompra)
                    .FirstOrDefaultAsync(i => i.Id == itemNotaFiscal.ItemPedidoCompraId);

                if (itemPedido != null)
                {
                    // Verificar se a quantidade recebida não ultrapassa a quantidade solicitada
                    decimal novaQuantidadeRecebida = itemPedido.QuantidadeRecebida + itemNotaFiscal.QuantidadeRecebida;
                    if (novaQuantidadeRecebida > itemPedido.QuantidadeSolicitada)
                    {
                        await transaction.RollbackAsync();
                        return false; // Quantidade recebida excede a solicitada
                    }

                    itemPedido.QuantidadeRecebida = novaQuantidadeRecebida;

                    // Verificar se o pedido foi totalmente atendido
                    var pedido = itemPedido.PedidoCompra;
                    if (pedido != null)
                    {
                        var itensPedido = await _context.ItensPedidoCompra
                            .Where(i => i.PedidoCompraId == pedido.Id)
                            .ToListAsync();

                        bool pedidoCompleto = itensPedido.All(i => i.QuantidadeRecebida >= i.QuantidadeSolicitada);
                        
                        if (pedidoCompleto)
                        {
                            pedido.Status = StatusPedidoCompra.TotalmenteRecebido;
                            pedido.DataEntrega = DateTime.Now;
                        }
                        else if (itensPedido.Any(i => i.QuantidadeRecebida > 0))
                        {
                            pedido.Status = StatusPedidoCompra.ParcialmenteRecebido;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Logar o erro (implementar um serviço de logging se necessário)
                Console.WriteLine($"Erro ao registrar entrada no estoque: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AtualizarEstoqueAposCancelamentoNotaFiscalAsync(int notaFiscalId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var itensNotaFiscal = await _context.ItensNotaFiscal
                    .Where(inf => inf.NotaFiscalId == notaFiscalId)
                    .ToListAsync();

                foreach (var item in itensNotaFiscal)
                {
                    var produto = await _context.Produtos.FindAsync(item.ProdutoId);
                    if (produto == null) continue;

                    // Reverter o estoque
                    produto.EstoqueAtual -= item.QuantidadeRecebida;
                    if (produto.EstoqueAtual < 0) produto.EstoqueAtual = 0;

                    // Atualizar o item do pedido de compra
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
                                pedido.Status = StatusPedidoCompra.Aprovado; // Volta para aprovado se não houver itens recebidos
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

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Erro ao reverter estoque: {ex.Message}");
                return false;
            }
        }
    }
}

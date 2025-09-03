using Microsoft.EntityFrameworkCore;
using MatMob.Data;
using MatMob.Models.Entities;
using MatMob.Models.ViewModels;

namespace MatMob.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var dashboardData = new DashboardViewModel();

            // Contadores gerais
            dashboardData.TotalAtivos = await _context.Ativos.CountAsync();
            dashboardData.AtivosAtivos = await _context.Ativos.CountAsync(a => a.Status == StatusAtivo.Ativo);
            dashboardData.AtivosEmManutencao = await _context.Ativos.CountAsync(a => a.Status == StatusAtivo.EmManutencao);
            dashboardData.AtivosInativos = await _context.Ativos.CountAsync(a => a.Status == StatusAtivo.Inativo);

            // Ordens de Serviço
            dashboardData.TotalOrdensServico = await _context.OrdensServico.CountAsync();
            dashboardData.OSAbertas = await _context.OrdensServico.CountAsync(os => os.Status == StatusOS.Aberta);
            dashboardData.OSEmAndamento = await _context.OrdensServico.CountAsync(os => os.Status == StatusOS.EmAndamento);
            dashboardData.OSConcluidas = await _context.OrdensServico.CountAsync(os => os.Status == StatusOS.Concluida);

            // Custos do mês atual
            var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var fimMes = inicioMes.AddMonths(1).AddDays(-1);
            
            dashboardData.CustoMesAtual = await _context.OrdensServico
                .Where(os => os.DataConclusao >= inicioMes && os.DataConclusao <= fimMes && os.CustoReal.HasValue)
                .SumAsync(os => os.CustoReal ?? 0);

            // Estoque baixo
            dashboardData.PecasEstoqueBaixo = await _context.Pecas
                .CountAsync(p => p.QuantidadeEstoque <= p.EstoqueMinimo && p.Ativa);

            // Técnicos ativos
            dashboardData.TecnicosAtivos = await _context.Tecnicos.CountAsync(t => t.Status == StatusTecnico.Ativo);

            // Equipes ativas
            dashboardData.EquipesAtivas = await _context.Equipes.CountAsync(e => e.Status == StatusEquipe.Ativa);

            // OS por status para gráfico
            dashboardData.OSPorStatus = await _context.OrdensServico
                .GroupBy(os => os.Status)
                .Select(g => new StatusCount
                {
                    Status = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToListAsync();

            // Ativos por tipo
            dashboardData.AtivosPorTipo = await _context.Ativos
                .GroupBy(a => a.Tipo)
                .Select(g => new TipoCount
                {
                    Tipo = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            // Custos dos últimos 6 meses
            dashboardData.CustosUltimosMeses = new List<CustoMensal>();
            for (int i = 5; i >= 0; i--)
            {
                var mes = DateTime.Now.AddMonths(-i);
                var inicioMesPeriodo = new DateTime(mes.Year, mes.Month, 1);
                var fimMesPeriodo = inicioMesPeriodo.AddMonths(1).AddDays(-1);

                var custoMes = await _context.OrdensServico
                    .Where(os => os.DataConclusao >= inicioMesPeriodo && 
                                os.DataConclusao <= fimMesPeriodo && 
                                os.CustoReal.HasValue)
                    .SumAsync(os => os.CustoReal ?? 0);

                dashboardData.CustosUltimosMeses.Add(new CustoMensal
                {
                    Mes = mes.ToString("MMM/yyyy"),
                    Custo = custoMes
                });
            }

            return dashboardData;
        }

        public async Task<List<AlertaViewModel>> GetAlertasAsync()
        {
            var alertas = new List<AlertaViewModel>();

            // Alertas de estoque baixo
            var pecasEstoqueBaixo = await _context.Pecas
                .Where(p => p.QuantidadeEstoque <= p.EstoqueMinimo && p.Ativa)
                .ToListAsync();

            foreach (var peca in pecasEstoqueBaixo)
            {
                alertas.Add(new AlertaViewModel
                {
                    Tipo = TipoAlerta.EstoqueBaixo,
                    Titulo = "Estoque Baixo",
                    Mensagem = $"A peça '{peca.Nome}' está com estoque baixo ({peca.QuantidadeEstoque} unidades)",
                    DataCriacao = DateTime.Now,
                    Prioridade = peca.QuantidadeEstoque == 0 ? PrioridadeAlerta.Alta : PrioridadeAlerta.Media,
                    Link = $"/Pecas/Details/{peca.Id}"
                });
            }

            // Alertas de manutenção preventiva próxima
            var dataLimite = DateTime.Now.AddDays(7); // Próximos 7 dias
            var planosVencendo = await _context.PlanosManutencao
                .Include(p => p.Ativo)
                .Where(p => p.Habilitado && p.ProximaManutencao <= dataLimite && p.ProximaManutencao >= DateTime.Now)
                .ToListAsync();

            foreach (var plano in planosVencendo)
            {
                var diasRestantes = (plano.ProximaManutencao!.Value - DateTime.Now).Days;
                alertas.Add(new AlertaViewModel
                {
                    Tipo = TipoAlerta.ManutencaoPreventiva,
                    Titulo = "Manutenção Preventiva Próxima",
                    Mensagem = $"O ativo '{plano.Ativo.Nome}' tem manutenção preventiva em {diasRestantes} dia(s)",
                    DataCriacao = DateTime.Now,
                    Prioridade = diasRestantes <= 1 ? PrioridadeAlerta.Alta : PrioridadeAlerta.Media,
                    Link = $"/Ativos/Details/{plano.AtivoId}"
                });
            }

            // Alertas de OS em atraso
            var osEmAtraso = await _context.OrdensServico
                .Include(os => os.Ativo)
                .Where(os => os.Status != StatusOS.Concluida && 
                            os.Status != StatusOS.Cancelada && 
                            os.DataAbertura < DateTime.Now.AddDays(-7)) // Mais de 7 dias em aberto
                .ToListAsync();

            foreach (var os in osEmAtraso)
            {
                var diasAtraso = (DateTime.Now - os.DataAbertura).Days;
                alertas.Add(new AlertaViewModel
                {
                    Tipo = TipoAlerta.OSEmAtraso,
                    Titulo = "OS em Atraso",
                    Mensagem = $"A OS #{os.NumeroOS} do ativo '{os.Ativo.Nome}' está há {diasAtraso} dias em aberto",
                    DataCriacao = DateTime.Now,
                    Prioridade = diasAtraso > 15 ? PrioridadeAlerta.Alta : PrioridadeAlerta.Media,
                    Link = $"/OrdensServico/Details/{os.Id}"
                });
            }

            return alertas.OrderByDescending(a => a.Prioridade).ThenByDescending(a => a.DataCriacao).ToList();
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MatMob.Data;
using MatMob.Models.Entities;
using MatMob.Models.ViewModels;
using MatMob.Extensions;
using MatMob.Services;

namespace MatMob.Controllers
{
    [Authorize]
    public class OrdensServicoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public OrdensServicoController(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // GET: OrdensServico
        public async Task<IActionResult> Index(string? searchString, StatusOS? statusFilter, TipoServico? tipoFilter)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["StatusFilter"] = statusFilter;
            ViewData["TipoFilter"] = tipoFilter;

            var ordensServico = _context.OrdensServico
                .Include(o => o.Ativo)
                .Include(o => o.Equipe)
                .Include(o => o.TecnicoResponsavel)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                ordensServico = ordensServico.Where(os => os.NumeroOS.Contains(searchString) ||
                                                         os.DescricaoProblema.Contains(searchString) ||
                                                         (os.Ativo != null && os.Ativo.Nome.Contains(searchString)));
            }

            if (statusFilter.HasValue)
            {
                ordensServico = ordensServico.Where(os => os.Status == statusFilter.Value);
            }

            if (tipoFilter.HasValue)
            {
                ordensServico = ordensServico.Where(os => os.TipoServico == tipoFilter.Value);
            }

            return View(await ordensServico.OrderByDescending(os => os.DataAbertura).ToListAsync());
        }

        // GET: OrdensServico/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordemServico = await _context.OrdensServico
                .Include(o => o.Ativo)
                .Include(o => o.Equipe)
                .Include(o => o.TecnicoResponsavel)
                .Include(o => o.ItensUtilizados)
                    .ThenInclude(i => i.Peca)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ordemServico == null)
            {
                return NotFound();
            }

            // Populate dropdown lists for the scheduling modal
            ViewBag.Equipes = new SelectList(
                await _context.Equipes.ToListAsync(),
                "Id", "Nome", ordemServico.EquipeId);
            
            ViewBag.Tecnicos = new SelectList(
                await _context.Tecnicos.ToListAsync(),
                "Id", "Nome", ordemServico.TecnicoResponsavelId);

            // Registrar auditoria de visualização
            await _auditService.LogViewAsync(
                entity: ordemServico,
                description: $"Visualização da OS: {ordemServico.NumeroOS} (Status: {ordemServico.Status})"
            );

            return View(ordemServico);
        }

        // GET: OrdensServico/Create
        [Authorize(Roles = "Administrador,Gestor,Tecnico")]
        public async Task<IActionResult> Create()
        {
            // Debug: Verificar se há ativos disponíveis
            var ativosCount = await _context.Ativos.Where(a => a.Status == StatusAtivo.Ativo).CountAsync();
            Console.WriteLine($"=== DEBUG CREATE GET ===");
            Console.WriteLine($"Ativos disponíveis: {ativosCount}");
            
            if (ativosCount == 0)
            {
                TempData["Error"] = "Não há ativos disponíveis. Cadastre um ativo antes de criar uma OS.";
                return RedirectToAction("Index", "Ativos");
            }
            
            await PopulateDropdownLists();
            
            var ordemServico = new OrdemServico
            {
                NumeroOS = await GerarNumeroOS(),
                DataAbertura = DateTime.Now,
                TipoServico = TipoServico.ManutencaoCorretiva,
                Prioridade = PrioridadeOS.Media,
                Status = StatusOS.Aberta
            };

            return View(ordemServico);
        }

        // POST: OrdensServico/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Gestor,Tecnico")]
        public async Task<IActionResult> Create([Bind("NumeroOS,TipoServico,Prioridade,DescricaoProblema,CustoEstimado,AtivoId,EquipeId,TecnicoResponsavelId")] OrdemServico ordemServico)
        {
            // Debug detalhado
            Console.WriteLine($"=== DEBUG CRIAR OS ===");
            Console.WriteLine($"AtivoId recebido: {ordemServico.AtivoId}");
            Console.WriteLine($"TipoServico: {ordemServico.TipoServico}");
            Console.WriteLine($"Prioridade: {ordemServico.Prioridade}");
            Console.WriteLine($"DescricaoProblema: '{ordemServico.DescricaoProblema}'");
            Console.WriteLine($"NumeroOS: '{ordemServico.NumeroOS}'");
            Console.WriteLine($"CustoEstimado: {ordemServico.CustoEstimado}");
            Console.WriteLine($"EquipeId: {ordemServico.EquipeId}");
            Console.WriteLine($"TecnicoResponsavelId: {ordemServico.TecnicoResponsavelId}");
            
            // Verificar se há ativos disponíveis
            var ativosDisponiveis = await _context.Ativos.Where(a => a.Status == StatusAtivo.Ativo).CountAsync();
            Console.WriteLine($"Ativos disponíveis no banco: {ativosDisponiveis}");
            
            // Verificar se o ativo específico existe
            if (ordemServico.AtivoId > 0)
            {
                var ativoExiste = await _context.Ativos.AnyAsync(a => a.Id == ordemServico.AtivoId && a.Status == StatusAtivo.Ativo);
                Console.WriteLine($"Ativo ID {ordemServico.AtivoId} existe e está ativo: {ativoExiste}");
            }
            
            // Debug: Log model state errors
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors?.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value?.Errors?.Select(e => e.ErrorMessage) ?? new List<string>() });
                    
                Console.WriteLine($"Total de campos com erro: {errors.Count()}");
                foreach (var error in errors)
                {
                    Console.WriteLine($"Campo: {error.Field}, Erros: {string.Join(", ", error.Errors)}");
                }
                
                // Verificar especificamente o AtivoId
                if (ModelState.ContainsKey("AtivoId"))
                {
                    var ativoState = ModelState["AtivoId"];
                    Console.WriteLine($"Estado do AtivoId no ModelState: {ativoState?.ValidationState}");
                    var ativoErrors = ativoState?.Errors?.Select(e => e.ErrorMessage) ?? new List<string>();
                    Console.WriteLine($"Erros do AtivoId: {string.Join(", ", ativoErrors)}");
                }
                
                // Verificar se há erro relacionado ao relacionamento Ativo
                if (ModelState.ContainsKey("Ativo"))
                {
                    var ativoNavState = ModelState["Ativo"];
                    Console.WriteLine($"Estado do Ativo no ModelState: {ativoNavState?.ValidationState}");
                    var ativoNavErrors = ativoNavState?.Errors?.Select(e => e.ErrorMessage) ?? new List<string>();
                    Console.WriteLine($"Erros do Ativo (navigation): {string.Join(", ", ativoNavErrors)}");
                }
                
                // Log de todos os estados do ModelState
                Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
                Console.WriteLine($"Todos os campos no ModelState:");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    Console.WriteLine($"  {key}: {state?.ValidationState} (Errors: {state?.Errors?.Count ?? 0})");
                }
                
                // Mensagem mais específica
                if (ordemServico.AtivoId == 0)
                {
                    TempData["Error"] = "Por favor, selecione um ativo para a ordem de serviço.";
                }
                else
                {
                    TempData["Error"] = "Por favor, corrija os erros no formulário.";
                }
                
                await PopulateDropdownLists(ordemServico);
                return View(ordemServico);
            }

            ordemServico.DataAbertura = DateTime.Now;
            ordemServico.Status = StatusOS.Aberta;
            
            _context.Add(ordemServico);
            await _context.SaveChangesAsync();
            
            // Registrar auditoria de criação
            await _auditService.LogCreateAsync(
                entity: ordemServico,
                description: $"Criação da OS: {ordemServico.NumeroOS} (Tipo: {ordemServico.TipoServico})"
            );
            
            TempData["Success"] = "Ordem de Serviço criada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        // GET: OrdensServico/Edit/5
        [Authorize(Roles = "Administrador,Gestor,Tecnico")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordemServico = await _context.OrdensServico.FindAsync(id);
            if (ordemServico == null)
            {
                return NotFound();
            }

            await PopulateDropdownLists(ordemServico);
            return View(ordemServico);
        }

        // POST: OrdensServico/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Gestor,Tecnico")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NumeroOS,TipoServico,Prioridade,Status,DescricaoProblema,SolucaoAplicada,DataAbertura,DataInicio,DataConclusao,TempoGastoHoras,CustoEstimado,CustoReal,AtivoId,EquipeId,TecnicoResponsavelId")] OrdemServico ordemServico)
        {
            if (id != ordemServico.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Capturar o estado antigo para auditoria
                    var ordemAntiga = await _context.OrdensServico.AsNoTracking().FirstOrDefaultAsync(os => os.Id == id);
                    
                    // Lógica para atualizar datas baseado no status
                    switch (ordemServico.Status)
                    {
                        case StatusOS.EmAndamento:
                            if (ordemServico.DataInicio == null)
                                ordemServico.DataInicio = DateTime.Now;
                            break;
                        case StatusOS.Concluida:
                            if (ordemServico.DataConclusao == null)
                                ordemServico.DataConclusao = DateTime.Now;
                            break;
                    }

                    _context.Update(ordemServico);
                    await _context.SaveChangesAsync();
                    
                    // Registrar auditoria de atualização
                    if (ordemAntiga != null)
                    {
                        await _auditService.LogUpdateAsync(
                            oldEntity: ordemAntiga,
                            newEntity: ordemServico,
                            description: $"Atualização da OS: {ordemServico.NumeroOS}"
                        );
                    }
                    
                    TempData["Success"] = "Ordem de Serviço atualizada com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrdemServicoExists(ordemServico.Id))
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

            await PopulateDropdownLists(ordemServico);
            return View(ordemServico);
        }

        // POST: OrdensServico/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Gestor,Tecnico")]
        public async Task<IActionResult> UpdateStatus(int id, StatusOS novoStatus)
        {
            var ordemServico = await _context.OrdensServico.FindAsync(id);
            if (ordemServico == null)
            {
                return NotFound();
            }

            var statusAnterior = ordemServico.Status;
            ordemServico.Status = novoStatus;

            // Atualizar datas baseado no novo status
            switch (novoStatus)
            {
                case StatusOS.EmAndamento:
                    if (ordemServico.DataInicio == null)
                        ordemServico.DataInicio = DateTime.Now;
                    break;
                case StatusOS.Concluida:
                    if (ordemServico.DataConclusao == null)
                        ordemServico.DataConclusao = DateTime.Now;
                    break;
            }

            _context.Update(ordemServico);
            await _context.SaveChangesAsync();

            // Registrar auditoria de mudança de status
            await _auditService.LogAsync(
                action: Services.AuditActions.UPDATE,
                entityName: $"OS {ordemServico.NumeroOS}",
                entityId: ordemServico.Id,
                description: $"Mudança de status: {statusAnterior} → {novoStatus}",
                category: Services.AuditCategory.BUSINESS_PROCESS
            );

            TempData["Success"] = $"Status da OS #{ordemServico.NumeroOS} alterado de {statusAnterior} para {novoStatus}";
            
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: OrdensServico/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordemServico = await _context.OrdensServico
                .Include(o => o.Ativo)
                .Include(o => o.Equipe)
                .Include(o => o.TecnicoResponsavel)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ordemServico == null)
            {
                return NotFound();
            }

            return View(ordemServico);
        }

        // POST: OrdensServico/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ordemServico = await _context.OrdensServico.FindAsync(id);
            if (ordemServico != null)
            {
                // Registrar auditoria de exclusão antes de remover
                await _auditService.LogDeleteAsync(
                    entity: ordemServico,
                    description: $"Exclusão da OS: {ordemServico.NumeroOS}"
                );

                _context.OrdensServico.Remove(ordemServico);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Ordem de Serviço excluída com sucesso!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool OrdemServicoExists(int id)
        {
            return _context.OrdensServico.Any(e => e.Id == id);
        }
        
        // POST: OrdensServico/AgendarOS
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<IActionResult> AgendarOS(int Id, DateTime? DataProgramada, TimeSpan? HoraInicioProgramada, TimeSpan? HoraFimProgramada, int? EquipeId, int? TecnicoResponsavelId)
        {
            var ordemServico = await _context.OrdensServico.FindAsync(Id);
            if (ordemServico == null)
            {
                TempData["Error"] = "Ordem de Serviço não encontrada.";
                return RedirectToAction(nameof(Index));
            }

            // Atualizar informações de agendamento
            ordemServico.DataProgramada = DataProgramada;
            ordemServico.HoraInicioProgramada = HoraInicioProgramada;
            ordemServico.HoraFimProgramada = HoraFimProgramada;
            ordemServico.EquipeId = EquipeId;
            ordemServico.TecnicoResponsavelId = TecnicoResponsavelId;

            // Se foi agendada, criar entrada na agenda
            if (DataProgramada.HasValue && EquipeId.HasValue)
            {
                var agendaItem = new AgendaManutencao
                {
                    OrdemServicoId = ordemServico.Id,
                    EquipeId = EquipeId.Value,
                    DataPrevista = DataProgramada.Value,
                    Titulo = $"OS {ordemServico.NumeroOS}",
                    Descricao = ordemServico.DescricaoProblema,
                    Tipo = TipoAgenda.OrdemServico,
                    AtivoId = ordemServico.AtivoId,
                    ResponsavelId = TecnicoResponsavelId ?? 0
                };

                _context.AgendaManutencao.Add(agendaItem);
            }

            _context.Update(ordemServico);
            await _context.SaveChangesAsync();

            // Registrar auditoria de agendamento
            await _auditService.LogAsync(
                action: Services.AuditActions.SCHEDULE,
                entityName: $"OS {ordemServico.NumeroOS}",
                entityId: ordemServico.Id,
                description: $"Agendamento da OS para {DataProgramada?.ToString("dd/MM/yyyy")} - Equipe: {ordemServico.Equipe?.Nome ?? "N/A"}",
                category: Services.AuditCategory.BUSINESS_PROCESS
            );

            TempData["Success"] = $"Ordem de Serviço {ordemServico.NumeroOS} agendada com sucesso!";
            return RedirectToAction(nameof(Details), new { id = Id });
        }

        // GET: OrdensServico/GetTecnicosPorEquipe
        [HttpGet]
        public async Task<JsonResult> GetTecnicosPorEquipe(int equipeId)
        {
            var tecnicos = await _context.EquipesTecnico
                .Where(et => et.EquipeId == equipeId && et.Ativo)
                .Include(et => et.Tecnico)
                .Where(et => et.Tecnico != null)
                .Select(et => new { 
                    value = et.TecnicoId.ToString(), 
                    text = et.Tecnico.Nome 
                })
                .ToListAsync();

            return Json(tecnicos);
        }

        // Método de teste para criar OS diretamente
        [HttpGet]
        [Route("OrdensServico/TesteCriar")]
        public async Task<IActionResult> TesteCriar()
        {
            try
            {
                var primeiroAtivo = await _context.Ativos.Where(a => a.Status == StatusAtivo.Ativo).FirstOrDefaultAsync();
                if (primeiroAtivo == null)
                {
                    return Json(new { sucesso = false, erro = "Nenhum ativo encontrado" });
                }
                
                var novaOS = new OrdemServico
                {
                    NumeroOS = await GerarNumeroOS(),
                    TipoServico = TipoServico.ManutencaoCorretiva,
                    Prioridade = PrioridadeOS.Media,
                    Status = StatusOS.Aberta,
                    DescricaoProblema = "Teste de criação direta",
                    DataAbertura = DateTime.Now,
                    AtivoId = primeiroAtivo.Id
                };
                
                _context.OrdensServico.Add(novaOS);
                await _context.SaveChangesAsync();
                
                return Json(new { 
                    sucesso = true, 
                    osId = novaOS.Id, 
                    numeroOS = novaOS.NumeroOS,
                    ativoId = novaOS.AtivoId
                });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, erro = ex.Message });
            }
        }

        private async Task PopulateDropdownLists(OrdemServico? ordemServico = null)
        {
            ViewData["AtivoId"] = new SelectList(
                await _context.Ativos.Where(a => a.Status == StatusAtivo.Ativo).ToListAsync(),
                "Id", "Nome", ordemServico?.AtivoId);

            ViewData["EquipeId"] = new SelectList(
                await _context.Equipes.Where(e => e.Status == StatusEquipe.Ativa).ToListAsync(),
                "Id", "Nome", ordemServico?.EquipeId);

            ViewData["TecnicoResponsavelId"] = new SelectList(
                await _context.Tecnicos.Where(t => t.Status == StatusTecnico.Ativo).ToListAsync(),
                "Id", "Nome", ordemServico?.TecnicoResponsavelId);
        }

        private async Task<string> GerarNumeroOS()
        {
            var ano = DateTime.Now.Year;
            var ultimaOS = await _context.OrdensServico
                .Where(os => os.NumeroOS.StartsWith($"OS{ano}"))
                .OrderByDescending(os => os.NumeroOS)
                .FirstOrDefaultAsync();

            int proximoNumero = 1;
            if (ultimaOS != null)
            {
                var numeroAtual = ultimaOS.NumeroOS.Substring(6); // Remove "OS2024"
                if (int.TryParse(numeroAtual, out int numero))
                {
                    proximoNumero = numero + 1;
                }
            }

            return $"OS{ano}{proximoNumero:D4}";
        }

        // GET: OrdensServico/Detalhada/5
        public async Task<IActionResult> Detalhada(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordemServico = await _context.OrdensServico
                .Include(o => o.Ativo)
                .Include(o => o.Equipe)
                .Include(o => o.TecnicoResponsavel)
                .Include(o => o.MateriaisUtilizados)
                    .ThenInclude(m => m.Peca)
                .Include(o => o.ApontamentosHoras)
                    .ThenInclude(a => a.Tecnico)
                .Include(o => o.AgendaItens)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ordemServico == null)
            {
                return NotFound();
            }

            var viewModel = new OrdemServicoDetalhadaViewModel
            {
                OrdemServico = ordemServico,
                MateriaisUtilizados = ordemServico.MateriaisUtilizados.ToList(),
                ApontamentosHoras = ordemServico.ApontamentosHoras.OrderByDescending(a => a.DataInicio).ToList(),
                AgendaItens = ordemServico.AgendaItens.ToList(),
                PecasDisponiveis = await _context.Pecas.Where(p => p.Ativa && p.QuantidadeEstoque > 0).ToListAsync(),
                TecnicosDisponiveis = await _context.Tecnicos.ToListAsync()
            };

            return View(viewModel);
        }

        // POST: AdicionarMaterial
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarMaterial(int ordemServicoId, int pecaId, decimal quantidadeUtilizada, string? observacoesMaterial)
        {
            if (quantidadeUtilizada <= 0)
            {
                TempData["Error"] = "A quantidade deve ser maior que zero.";
                return RedirectToAction("Detalhada", new { id = ordemServicoId });
            }

            var peca = await _context.Pecas.FindAsync(pecaId);
            if (peca == null)
            {
                TempData["Error"] = "Peça não encontrada.";
                return RedirectToAction("Detalhada", new { id = ordemServicoId });
            }

            if (peca.QuantidadeEstoque < quantidadeUtilizada)
            {
                TempData["Error"] = $"Estoque insuficiente. Disponível: {peca.QuantidadeEstoque}";
                return RedirectToAction("Detalhada", new { id = ordemServicoId });
            }

            var material = new MaterialOrdemServico
            {
                OrdemServicoId = ordemServicoId,
                PecaId = pecaId,
                QuantidadeUtilizada = quantidadeUtilizada,
                PrecoUnitario = peca.PrecoUnitario,
                ValorTotal = quantidadeUtilizada * (peca.PrecoUnitario ?? 0),
                Observacoes = observacoesMaterial,
                UsuarioRegistro = User.Identity?.Name
            };

            _context.MateriaisOrdemServico.Add(material);

            // Atualizar estoque
            peca.QuantidadeEstoque -= (int)quantidadeUtilizada;

            // Criar movimentação de estoque
            var movimentacao = new MovimentacaoEstoque
            {
                PecaId = pecaId,
                TipoMovimentacao = TipoMovimentacao.Saida,
                Quantidade = (int)quantidadeUtilizada,
                Motivo = $"Utilizada na OS #{ordemServicoId}",
                OrdemServicoId = ordemServicoId
            };

            _context.MovimentacoesEstoque.Add(movimentacao);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Material adicionado com sucesso!";
            return RedirectToAction("Detalhada", new { id = ordemServicoId });
        }

        // POST: AdicionarApontamento
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarApontamento(int ordemServicoId, int tecnicoId, DateTime dataInicio, DateTime dataFim, string? descricaoAtividade, string? observacoesHoras)
        {
            if (dataFim <= dataInicio)
            {
                TempData["Error"] = "A data/hora de fim deve ser posterior à data/hora de início.";
                return RedirectToAction("Detalhada", new { id = ordemServicoId });
            }

            var horasTrabalhadas = (decimal)(dataFim - dataInicio).TotalHours;

            var apontamento = new ApontamentoHoras
            {
                OrdemServicoId = ordemServicoId,
                TecnicoId = tecnicoId,
                DataInicio = dataInicio,
                DataFim = dataFim,
                HorasTrabalhadas = horasTrabalhadas,
                DescricaoAtividade = descricaoAtividade,
                Observacoes = observacoesHoras,
                UsuarioRegistro = User.Identity?.Name
            };

            _context.ApontamentosHoras.Add(apontamento);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Apontamento de horas registrado com sucesso!";
            return RedirectToAction("Detalhada", new { id = ordemServicoId });
        }

        // GET: OrdensServico/Calendario
        public async Task<IActionResult> Calendario(int? mes, int? ano)
        {
            var dataAtual = DateTime.Today;
            var mesAtual = mes ?? dataAtual.Month;
            var anoAtual = ano ?? dataAtual.Year;

            var primeiroDiaMes = new DateTime(anoAtual, mesAtual, 1);
            var ultimoDiaMes = primeiroDiaMes.AddMonths(1).AddDays(-1);

            var eventosDoMes = await _context.AgendaManutencao
                .Include(a => a.Ativo)
                .Include(a => a.Responsavel)
                .Include(a => a.Equipe)
                .Where(a => a.DataPrevista >= primeiroDiaMes && a.DataPrevista <= ultimoDiaMes)
                .OrderBy(a => a.DataPrevista)
                .ToListAsync();

            var viewModel = new CalendarioManutencaoViewModel
            {
                MesAtual = mesAtual,
                AnoAtual = anoAtual,
                EventosDoMes = eventosDoMes,
                AtivosDisponiveis = await _context.Ativos.Where(a => a.Status == StatusAtivo.Ativo).ToListAsync(),
                EquipesDisponiveis = await _context.Equipes.ToListAsync(),
                TecnicosDisponiveis = await _context.Tecnicos.ToListAsync()
            };

            return View(viewModel);
        }

        // POST: CriarEventoAgenda
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CriarEventoAgenda(CalendarioManutencaoViewModel model)
        {
            var evento = new AgendaManutencao
            {
                Titulo = model.Titulo,
                Descricao = model.Descricao,
                DataPrevista = model.DataPrevista,
                Tipo = model.Tipo,
                AtivoId = model.AtivoId,
                EquipeId = model.EquipeId,
                ResponsavelId = model.ResponsavelId,
                Observacoes = model.Observacoes,
                UsuarioCriacao = User.Identity?.Name
            };

            _context.AgendaManutencao.Add(evento);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Evento adicionado ao calendário com sucesso!";
            return RedirectToAction("Calendario", new { mes = model.DataPrevista.Month, ano = model.DataPrevista.Year });
        }

        // GET: OrdensServico/Gantt
        public async Task<IActionResult> Gantt(string? status, string? prioridade)
        {
            var query = _context.OrdensServico
                .Include(o => o.Ativo)
                .Include(o => o.TecnicoResponsavel)
                .Include(o => o.Equipe)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && status != "Todas")
            {
                if (Enum.TryParse<StatusOS>(status, out var statusEnum))
                {
                    query = query.Where(o => o.Status == statusEnum);
                }
            }

            if (!string.IsNullOrEmpty(prioridade) && prioridade != "Todas")
            {
                if (Enum.TryParse<PrioridadeOS>(prioridade, out var prioridadeEnum))
                {
                    query = query.Where(o => o.Prioridade == prioridadeEnum);
                }
            }

            var ordensServico = await query.OrderBy(o => o.DataAbertura).ToListAsync();

            var ganttItens = ordensServico.Select(os => new GanttItem
            {
                Id = os.Id,
                NumeroOS = os.NumeroOS,
                Titulo = $"{os.NumeroOS} - {(os.Ativo?.Nome ?? "Desconhecido")}",
                Ativo = os.Ativo != null ? os.Ativo.Nome : "Desconhecido",
                Status = os.Status.GetDisplayName(),
                Prioridade = os.Prioridade.GetDisplayName(),
                Responsavel = os.TecnicoResponsavel?.Nome,
                Equipe = os.Equipe?.Nome,
                DataAbertura = os.DataAbertura,
                DataInicio = os.DataInicio,
                DataConclusao = os.DataConclusao,
                DataPrevistaConclusao = os.DataInicio?.AddDays(GetDuracaoPrevista(os.TipoServico)) ?? os.DataAbertura.AddDays(GetDuracaoPrevista(os.TipoServico)),
                DuracaoPrevista = GetDuracaoPrevista(os.TipoServico),
                DuracaoReal = os.DataConclusao.HasValue && os.DataInicio.HasValue ? 
                    (int)(os.DataConclusao.Value - os.DataInicio.Value).TotalDays : 0,
                PercentualConclusao = GetPercentualConclusao(os),
                CorStatus = GetCorStatus(os.Status),
                CorPrioridade = GetCorPrioridade(os.Prioridade)
            }).ToList();

            var viewModel = new GanttChartViewModel
            {
                Itens = ganttItens,
                FiltroStatus = status ?? "Todas",
                FiltroPrioridade = prioridade ?? "Todas"
            };

            return View(viewModel);
        }

        // GET: OrdensServico/CalendarioOS
        public async Task<IActionResult> CalendarioOS(int? mes, int? ano, int? equipe)
        {
            var dataAtual = DateTime.Today;
            var mesAtual = mes ?? dataAtual.Month;
            var anoAtual = ano ?? dataAtual.Year;

            var primeiroDiaMes = new DateTime(anoAtual, mesAtual, 1);
            var ultimoDiaMes = primeiroDiaMes.AddMonths(1).AddDays(-1);

            var query = _context.OrdensServico
                .Include(o => o.Ativo)
                .Include(o => o.Equipe)
                .Include(o => o.TecnicoResponsavel)
                .Where(o => o.DataProgramada.HasValue && 
                           o.DataProgramada.Value.Date >= primeiroDiaMes && 
                           o.DataProgramada.Value.Date <= ultimoDiaMes);

            if (equipe.HasValue && equipe.Value > 0)
            {
                query = query.Where(o => o.EquipeId == equipe.Value);
            }

            var ordensDoMes = await query
                .OrderBy(o => o.DataProgramada)
                .ThenBy(o => o.HoraInicioProgramada)
                .Select(o => new OrdemServicoAgendada
                {
                    Id = o.Id,
                    NumeroOS = o.NumeroOS,
                    DescricaoProblema = o.DescricaoProblema,
                    TipoServico = o.TipoServico,
                    Status = o.Status,
                    Prioridade = o.Prioridade,
                    DataProgramada = o.DataProgramada!.Value,
                    HoraInicioProgramada = o.HoraInicioProgramada,
                    HoraFimProgramada = o.HoraFimProgramada,
                    NomeAtivo = o.Ativo != null ? o.Ativo.Nome : "Desconhecido",
                    LocalizacaoAtivo = o.Ativo != null ? o.Ativo.Localizacao : "N/A",
                    NomeEquipe = o.Equipe != null ? o.Equipe.Nome : null,
                    NomeTecnico = o.TecnicoResponsavel != null ? o.TecnicoResponsavel.Nome : null,
                    DataAbertura = o.DataAbertura,
                    DataInicio = o.DataInicio,
                    DataConclusao = o.DataConclusao
                })
                .ToListAsync();

            var viewModel = new CalendarioOSViewModel
            {
                MesAtual = mesAtual,
                AnoAtual = anoAtual,
                EquipeFiltro = equipe,
                OrdensDoMes = ordensDoMes,
                AtivosDisponiveis = await _context.Ativos.Where(a => a.Status == StatusAtivo.Ativo).ToListAsync(),
                EquipesDisponiveis = await _context.Equipes.ToListAsync(),
                TecnicosDisponiveis = await _context.Tecnicos.ToListAsync()
            };

            return View(viewModel);
        }

        // GET: OrdensServico/GetOrdensDoDia
        [HttpGet]
        public async Task<IActionResult> GetOrdensDoDia(DateTime data, int? equipe)
        {
            var query = _context.OrdensServico
                .Include(o => o.Ativo)
                .Include(o => o.Equipe)
                .Include(o => o.TecnicoResponsavel)
                .Where(o => o.DataProgramada.HasValue && o.DataProgramada.Value.Date == data.Date);

            if (equipe.HasValue && equipe.Value > 0)
            {
                query = query.Where(o => o.EquipeId == equipe.Value);
            }

            var ordensDoDia = await query
                .OrderBy(o => o.HoraInicioProgramada)
                .Select(o => new OrdemServicoAgendada
                {
                    Id = o.Id,
                    NumeroOS = o.NumeroOS,
                    DescricaoProblema = o.DescricaoProblema,
                    TipoServico = o.TipoServico,
                    Status = o.Status,
                    Prioridade = o.Prioridade,
                    DataProgramada = o.DataProgramada!.Value,
                    HoraInicioProgramada = o.HoraInicioProgramada,
                    HoraFimProgramada = o.HoraFimProgramada,
                    NomeAtivo = o.Ativo != null ? o.Ativo.Nome : "Desconhecido",
                    LocalizacaoAtivo = o.Ativo != null ? o.Ativo.Localizacao : "N/A",
                    NomeEquipe = o.Equipe != null ? o.Equipe.Nome : null,
                    NomeTecnico = o.TecnicoResponsavel != null ? o.TecnicoResponsavel.Nome : null,
                    DataAbertura = o.DataAbertura,
                    DataInicio = o.DataInicio,
                    DataConclusao = o.DataConclusao
                })
                .ToListAsync();

            return PartialView("_OrdensDoDia", ordensDoDia);
        }

        // POST: AgendarOrdemServico
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgendarOrdemServico(CalendarioOSViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Recarregar listas para dropdown
                model.AtivosDisponiveis = await _context.Ativos.Where(a => a.Status == StatusAtivo.Ativo).ToListAsync();
                model.EquipesDisponiveis = await _context.Equipes.ToListAsync();
                model.TecnicosDisponiveis = await _context.Tecnicos.ToListAsync();
                
                return PartialView("_FormAgendarOS", model);
            }

            // Gerar número da OS
            var numeroOS = await GerarNumeroOS();

            var ordemServico = new OrdemServico
            {
                NumeroOS = numeroOS,
                TipoServico = model.TipoServico,
                Prioridade = model.Prioridade,
                Status = StatusOS.Aberta,
                DescricaoProblema = model.DescricaoProblema,
                AtivoId = model.AtivoId,
                EquipeId = model.EquipeId,
                TecnicoResponsavelId = model.TecnicoResponsavelId,
                DataProgramada = model.DataProgramada,
                HoraInicioProgramada = model.HoraInicioProgramada,
                HoraFimProgramada = model.HoraFimProgramada,
                DataAbertura = DateTime.Now
            };

            _context.OrdensServico.Add(ordemServico);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Ordem de Serviço {numeroOS} agendada com sucesso para {model.DataProgramada:dd/MM/yyyy} às {model.HoraInicioProgramada:hh\\:mm}!";

            return Json(new { success = true, message = TempData["Success"] });
        }

        private static int GetDuracaoPrevista(TipoServico tipo)
        {
            return tipo switch
            {
                TipoServico.ManutencaoPreventiva => 3,
                TipoServico.ManutencaoCorretiva => 5,
                TipoServico.Instalacao => 7,
                TipoServico.Inspecao => 1,
                TipoServico.Reparo => 4,
                _ => 3
            };
        }

        private static double GetPercentualConclusao(OrdemServico os)
        {
            return os.Status switch
            {
                StatusOS.Aberta => 0,
                StatusOS.EmAndamento => 50,
                StatusOS.AguardandoPecas => 25,
                StatusOS.Concluida => 100,
                StatusOS.Cancelada => 0,
                _ => 0
            };
        }

        private static string GetCorStatus(StatusOS status)
        {
            return status switch
            {
                StatusOS.Aberta => "#6c757d",
                StatusOS.EmAndamento => "#0d6efd",
                StatusOS.AguardandoPecas => "#ffc107",
                StatusOS.Concluida => "#198754",
                StatusOS.Cancelada => "#dc3545",
                _ => "#6c757d"
            };
        }

        private static string GetCorPrioridade(PrioridadeOS prioridade)
        {
            return prioridade switch
            {
                PrioridadeOS.Baixa => "#198754",
                PrioridadeOS.Media => "#ffc107",
                PrioridadeOS.Alta => "#fd7e14",
                PrioridadeOS.Critica => "#dc3545",
                _ => "#6c757d"
            };
        }

        // GET: OrdensServico/EditarEvento/5
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<IActionResult> EditarEvento(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var evento = await _context.AgendaManutencao.FindAsync(id);
            if (evento == null)
            {
                return NotFound();
            }

            // Verificar se é uma O.S. (não permitir edição)
            if (evento.Tipo == TipoAgenda.OrdemServico)
            {
                TempData["Error"] = "Não é possível editar eventos do tipo Ordem de Serviço diretamente pelo calendário.";
                return RedirectToAction(nameof(Calendario));
            }

            var viewModel = new CalendarioManutencaoViewModel
            {
                Id = evento.Id,
                Titulo = evento.Titulo,
                Descricao = evento.Descricao,
                DataPrevista = evento.DataPrevista,
                Tipo = evento.Tipo,
                AtivoId = evento.AtivoId,
                EquipeId = evento.EquipeId,
                ResponsavelId = evento.ResponsavelId,
                Observacoes = evento.Observacoes,
                AtivosDisponiveis = await _context.Ativos.Where(a => a.Status == StatusAtivo.Ativo).ToListAsync(),
                EquipesDisponiveis = await _context.Equipes.ToListAsync(),
                TecnicosDisponiveis = await _context.Tecnicos.ToListAsync()
            };

            return View(viewModel);
        }

        // POST: OrdensServico/EditarEvento/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<IActionResult> EditarEvento(int id, CalendarioManutencaoViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var evento = await _context.AgendaManutencao.FindAsync(id);
                    if (evento == null)
                    {
                        return NotFound();
                    }

                    // Verificar se é uma O.S. (não permitir edição)
                    if (evento.Tipo == TipoAgenda.OrdemServico)
                    {
                        TempData["Error"] = "Não é possível editar eventos do tipo Ordem de Serviço diretamente pelo calendário.";
                        return RedirectToAction(nameof(Calendario));
                    }

                    // Atualizar os dados do evento
                    evento.Titulo = model.Titulo;
                    evento.Descricao = model.Descricao;
                    evento.DataPrevista = model.DataPrevista;
                    evento.Tipo = model.Tipo;
                    evento.AtivoId = model.AtivoId;
                    evento.EquipeId = model.EquipeId;
                    evento.ResponsavelId = model.ResponsavelId;
                    evento.Observacoes = model.Observacoes;

                    _context.Update(evento);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Evento atualizado com sucesso!";
                    return RedirectToAction(nameof(Calendario), new { mes = evento.DataPrevista.Month, ano = evento.DataPrevista.Year });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventoExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Se houver erros de validação, recarregar as listas e retornar para a view
            model.AtivosDisponiveis = await _context.Ativos.Where(a => a.Status == StatusAtivo.Ativo).ToListAsync();
            model.EquipesDisponiveis = await _context.Equipes.ToListAsync();
            model.TecnicosDisponiveis = await _context.Tecnicos.ToListAsync();
            
            return View(model);
        }

        // GET: OrdensServico/ExcluirEvento/5
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<IActionResult> ExcluirEvento(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var evento = await _context.AgendaManutencao
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (evento == null)
            {
                return NotFound();
            }

            // Verificar se é uma O.S. (não permitir exclusão)
            if (evento.Tipo == TipoAgenda.OrdemServico)
            {
                TempData["Error"] = "Não é possível excluir eventos do tipo Ordem de Serviço diretamente pelo calendário.";
                return RedirectToAction(nameof(Calendario));
            }

            _context.AgendaManutencao.Remove(evento);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Evento excluído com sucesso!";
            return RedirectToAction(nameof(Calendario));
        }

        private bool EventoExists(int id)
        {
            return _context.AgendaManutencao.Any(e => e.Id == id);
        }
    }
}

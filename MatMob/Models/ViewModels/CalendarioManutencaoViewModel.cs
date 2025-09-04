using MatMob.Models.Entities;

namespace MatMob.Models.ViewModels
{
    public class CalendarioManutencaoViewModel
    {
        public int Id { get; set; }
        public DateTime DataSelecionada { get; set; } = DateTime.Today;
        public int MesAtual { get; set; } = DateTime.Today.Month;
        public int AnoAtual { get; set; } = DateTime.Today.Year;
        
        public List<AgendaManutencao> EventosDoMes { get; set; } = new();
        public List<AgendaManutencao> EventosDoDia { get; set; } = new();
        
        // Para o formulário de novo evento
        public string Titulo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public DateTime DataPrevista { get; set; } = DateTime.Now;
        public TipoAgenda Tipo { get; set; }
        public int? AtivoId { get; set; }
        public int? EquipeId { get; set; }
        public int ResponsavelId { get; set; }
        public string? Observacoes { get; set; }
        
        // Listas para dropdowns
        public List<Ativo> AtivosDisponiveis { get; set; } = new();
        public List<Equipe> EquipesDisponiveis { get; set; } = new();
        public List<Tecnico> TecnicosDisponiveis { get; set; } = new();
        
        // Estatísticas do mês
        public int TotalEventosDoMes => EventosDoMes.Count;
        public int EventosAgendados => EventosDoMes.Count(e => e.Status == StatusAgenda.Agendada);
        public int EventosEmAndamento => EventosDoMes.Count(e => e.Status == StatusAgenda.EmAndamento);
        public int EventosConcluidos => EventosDoMes.Count(e => e.Status == StatusAgenda.Concluida);
        
        // Eventos por tipo
        public int ManutencaoPreventivaCount => EventosDoMes.Count(e => e.Tipo == TipoAgenda.ManutencaoPreventiva);
        public int ManutencaoCorretivaCount => EventosDoMes.Count(e => e.Tipo == TipoAgenda.ManutencaoCorretiva);
        public int InspecaoCount => EventosDoMes.Count(e => e.Tipo == TipoAgenda.Inspecao);
    }
}

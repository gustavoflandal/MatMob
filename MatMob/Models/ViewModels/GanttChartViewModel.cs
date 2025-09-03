using MatMob.Models.Entities;

namespace MatMob.Models.ViewModels
{
    public class GanttChartViewModel
    {
        public List<GanttItem> Itens { get; set; } = new();
        public DateTime DataInicio { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime DataFim { get; set; } = DateTime.Today.AddDays(30);
        public string FiltroStatus { get; set; } = "Todas";
        public string FiltroPrioridade { get; set; } = "Todas";
        
        // Estatísticas para o dashboard
        public int TotalOrdens => Itens.Count;
        public int OrdensAtrasadas => Itens.Count(i => i.DataPrevistaConclusao < DateTime.Today && i.Status != "Concluída");
        public int OrdensEmAndamento => Itens.Count(i => i.Status == "Em Andamento");
        public int OrdensConcluidas => Itens.Count(i => i.Status == "Concluída");
        
        public double PercentualConclusao => TotalOrdens > 0 ? (double)OrdensConcluidas / TotalOrdens * 100 : 0;
        public double PercentualAtrasadas => TotalOrdens > 0 ? (double)OrdensAtrasadas / TotalOrdens * 100 : 0;
    }

    public class GanttItem
    {
        public int Id { get; set; }
        public string NumeroOS { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Ativo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Prioridade { get; set; } = string.Empty;
        public string? Responsavel { get; set; }
        public string? Equipe { get; set; }
        
        public DateTime DataAbertura { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataConclusao { get; set; }
        public DateTime DataPrevistaConclusao { get; set; }
        
        public int DuracaoPrevista { get; set; } // em dias
        public int DuracaoReal { get; set; } // em dias
        public double PercentualConclusao { get; set; }
        
        public string CorStatus { get; set; } = "#6c757d"; // cinza padrão
        public string CorPrioridade { get; set; } = "#6c757d";
        public bool EstaAtrasada => DataPrevistaConclusao < DateTime.Today && Status != "Concluída";
        
        public string CssClass => EstaAtrasada ? "gantt-item-atrasada" : 
                                 Status == "Concluída" ? "gantt-item-concluida" :
                                 Status == "Em Andamento" ? "gantt-item-andamento" : 
                                 "gantt-item-pendente";
    }
}

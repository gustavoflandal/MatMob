using System.ComponentModel.DataAnnotations;

namespace MatMob.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Contadores de Ativos
        [Display(Name = "Total de Ativos")]
        public int TotalAtivos { get; set; }

        [Display(Name = "Ativos Ativos")]
        public int AtivosAtivos { get; set; }

        [Display(Name = "Ativos em Manutenção")]
        public int AtivosEmManutencao { get; set; }

        [Display(Name = "Ativos Inativos")]
        public int AtivosInativos { get; set; }

        // Contadores de Ordens de Serviço
        [Display(Name = "Total de OS")]
        public int TotalOrdensServico { get; set; }

        [Display(Name = "OS Abertas")]
        public int OSAbertas { get; set; }

        [Display(Name = "OS em Andamento")]
        public int OSEmAndamento { get; set; }

        [Display(Name = "OS Concluídas")]
        public int OSConcluidas { get; set; }

        // Informações Financeiras
        [Display(Name = "Custo do Mês Atual")]
        public decimal CustoMesAtual { get; set; }

        // Estoque
        [Display(Name = "Peças com Estoque Baixo")]
        public int PecasEstoqueBaixo { get; set; }

        // Recursos Humanos
        [Display(Name = "Técnicos Ativos")]
        public int TecnicosAtivos { get; set; }

        [Display(Name = "Equipes Ativas")]
        public int EquipesAtivas { get; set; }

        // Dados para gráficos
        public List<StatusCount> OSPorStatus { get; set; } = new List<StatusCount>();
        public List<TipoCount> AtivosPorTipo { get; set; } = new List<TipoCount>();
        public List<CustoMensal> CustosUltimosMeses { get; set; } = new List<CustoMensal>();

        // Alertas
        public List<AlertaViewModel> Alertas { get; set; } = new List<AlertaViewModel>();
    }

    public class StatusCount
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class TipoCount
    {
        public string Tipo { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class CustoMensal
    {
        public string Mes { get; set; } = string.Empty;
        public decimal Custo { get; set; }
    }

    public class AlertaViewModel
    {
        public TipoAlerta Tipo { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Mensagem { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
        public PrioridadeAlerta Prioridade { get; set; }
        public string? Link { get; set; }

        public string GetIconClass()
        {
            return Tipo switch
            {
                TipoAlerta.EstoqueBaixo => "fas fa-box text-warning",
                TipoAlerta.ManutencaoPreventiva => "fas fa-tools text-info",
                TipoAlerta.OSEmAtraso => "fas fa-exclamation-triangle text-danger",
                _ => "fas fa-info-circle text-primary"
            };
        }

        public string GetPrioridadeClass()
        {
            return Prioridade switch
            {
                PrioridadeAlerta.Alta => "border-danger",
                PrioridadeAlerta.Media => "border-warning",
                PrioridadeAlerta.Baixa => "border-info",
                _ => "border-secondary"
            };
        }
    }

    public enum TipoAlerta
    {
        [Display(Name = "Estoque Baixo")]
        EstoqueBaixo = 1,

        [Display(Name = "Manutenção Preventiva")]
        ManutencaoPreventiva = 2,

        [Display(Name = "OS em Atraso")]
        OSEmAtraso = 3,

        [Display(Name = "Geral")]
        Geral = 4
    }

    public enum PrioridadeAlerta
    {
        [Display(Name = "Baixa")]
        Baixa = 1,

        [Display(Name = "Média")]
        Media = 2,

        [Display(Name = "Alta")]
        Alta = 3
    }
}

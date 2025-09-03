using MatMob.Models.Entities;

namespace MatMob.Models.ViewModels
{
    public class CalendarioOSViewModel
    {
        public DateTime DataSelecionada { get; set; } = DateTime.Today;
        public int MesAtual { get; set; } = DateTime.Today.Month;
        public int AnoAtual { get; set; } = DateTime.Today.Year;
        public int? EquipeFiltro { get; set; }
        
        public List<OrdemServicoAgendada> OrdensDoMes { get; set; } = new();
        public List<OrdemServicoAgendada> OrdensDoDia { get; set; } = new();
        
        // Para o formulário de nova OS
        public string NumeroOS { get; set; } = string.Empty;
        public TipoServico TipoServico { get; set; }
        public PrioridadeOS Prioridade { get; set; } = PrioridadeOS.Media;
        public string DescricaoProblema { get; set; } = string.Empty;
        public int AtivoId { get; set; }
        public int? EquipeId { get; set; }
        public int? TecnicoResponsavelId { get; set; }
        public DateTime DataProgramada { get; set; } = DateTime.Today;
        public TimeSpan HoraInicioProgramada { get; set; } = new TimeSpan(8, 0, 0);
        public TimeSpan HoraFimProgramada { get; set; } = new TimeSpan(17, 0, 0);
        
        // Listas para dropdowns
        public List<Ativo> AtivosDisponiveis { get; set; } = new();
        public List<Equipe> EquipesDisponiveis { get; set; } = new();
        public List<Tecnico> TecnicosDisponiveis { get; set; } = new();
        
        // Estatísticas
        public int TotalOrdensDoMes => OrdensDoMes.Count;
        public int OrdensAbertasDoMes => OrdensDoMes.Count(o => o.Status == StatusOS.Aberta);
        public int OrdensEmAndamentoDoMes => OrdensDoMes.Count(o => o.Status == StatusOS.EmAndamento);
        public int OrdensConcluidasDoMes => OrdensDoMes.Count(o => o.Status == StatusOS.Concluida);
        public int OrdensAtrasadasDoMes => OrdensDoMes.Count(o => o.DataProgramada < DateTime.Today && o.Status != StatusOS.Concluida);
    }

    public class OrdemServicoAgendada
    {
        public int Id { get; set; }
        public string NumeroOS { get; set; } = string.Empty;
        public string DescricaoProblema { get; set; } = string.Empty;
        public TipoServico TipoServico { get; set; }
        public StatusOS Status { get; set; }
        public PrioridadeOS Prioridade { get; set; }
        
        public DateTime DataProgramada { get; set; }
        public TimeSpan? HoraInicioProgramada { get; set; }
        public TimeSpan? HoraFimProgramada { get; set; }
        
        public string NomeAtivo { get; set; } = string.Empty;
        public string LocalizacaoAtivo { get; set; } = string.Empty;
        public string? NomeEquipe { get; set; }
        public string? NomeTecnico { get; set; }
        
        public DateTime DataAbertura { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataConclusao { get; set; }
        
        public bool EstaAtrasada => DataProgramada.Date < DateTime.Today && Status != StatusOS.Concluida;
        
        public string CorStatus => Status switch
        {
            StatusOS.Aberta => "#6c757d",
            StatusOS.EmAndamento => "#0d6efd",
            StatusOS.AguardandoPecas => "#ffc107",
            StatusOS.Concluida => "#198754",
            StatusOS.Cancelada => "#dc3545",
            _ => "#6c757d"
        };
        
        public string CorPrioridade => Prioridade switch
        {
            PrioridadeOS.Baixa => "#198754",
            PrioridadeOS.Media => "#ffc107",
            PrioridadeOS.Alta => "#fd7e14",
            PrioridadeOS.Critica => "#dc3545",
            _ => "#6c757d"
        };

        public string HorarioFormatado => HoraInicioProgramada.HasValue && HoraFimProgramada.HasValue
            ? $"{HoraInicioProgramada:hh\\:mm} - {HoraFimProgramada:hh\\:mm}"
            : "Horário não definido";
    }
}

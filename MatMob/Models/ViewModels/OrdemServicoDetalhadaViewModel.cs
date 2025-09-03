using MatMob.Models.Entities;

namespace MatMob.Models.ViewModels
{
    public class OrdemServicoDetalhadaViewModel
    {
        public OrdemServico OrdemServico { get; set; } = null!;
        public List<MaterialOrdemServico> MateriaisUtilizados { get; set; } = new();
        public List<ApontamentoHoras> ApontamentosHoras { get; set; } = new();
        public List<AgendaManutencao> AgendaItens { get; set; } = new();
        
        // Para o formulário de adição de materiais
        public int PecaId { get; set; }
        public decimal QuantidadeUtilizada { get; set; }
        public string? ObservacoesMaterial { get; set; }
        
        // Para o formulário de apontamento de horas
        public int TecnicoId { get; set; }
        public DateTime DataInicio { get; set; } = DateTime.Now;
        public DateTime DataFim { get; set; } = DateTime.Now.AddHours(1);
        public string? DescricaoAtividade { get; set; }
        public string? ObservacoesHoras { get; set; }
        
        // Listas para dropdowns
        public List<Peca> PecasDisponiveis { get; set; } = new();
        public List<Tecnico> TecnicosDisponiveis { get; set; } = new();
        
        // Totais calculados
        public decimal TotalCustoMateriais => MateriaisUtilizados.Sum(m => m.ValorTotal);
        public decimal TotalHorasTrabalhadas => ApontamentosHoras.Sum(a => a.HorasTrabalhadas);
        public int TotalMateriaisUtilizados => MateriaisUtilizados.Count;
        public int TotalApontamentos => ApontamentosHoras.Count;
    }
}

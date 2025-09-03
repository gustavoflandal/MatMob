using MatMob.Models.ViewModels;

namespace MatMob.Services
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardDataAsync();
        Task<List<AlertaViewModel>> GetAlertasAsync();
    }
}

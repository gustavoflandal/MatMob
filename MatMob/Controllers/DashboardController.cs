using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MatMob.Services;

namespace MatMob.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var dashboardData = await _dashboardService.GetDashboardDataAsync();
            dashboardData.Alertas = await _dashboardService.GetAlertasAsync();
            
            return View(dashboardData);
        }

        [HttpGet]
        public async Task<IActionResult> GetAlertas()
        {
            var alertas = await _dashboardService.GetAlertasAsync();
            return PartialView("_Alertas", alertas);
        }
    }
}

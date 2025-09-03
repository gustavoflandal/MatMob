using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MatMob.Areas.Identity.Pages.Account.Manage
{
    public class PersonalDataModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<PersonalDataModel> _logger;

        public PersonalDataModel(
            UserManager<IdentityUser> userManager,
            ILogger<PersonalDataModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public int ConfigurationCount { get; set; }
        public int ActivityCount { get; set; }
        public int LoginCount { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Não foi possível carregar o usuário com ID '{_userManager.GetUserId(User)}'.");
            }

            // Simular contagens de dados (em um sistema real, você consultaria o banco de dados)
            ConfigurationCount = 5; // Configurações do usuário
            ActivityCount = 47; // Atividades registradas
            LoginCount = 23; // Histórico de logins

            return Page();
        }

        public async Task<IActionResult> OnPostDownloadPersonalDataAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Não foi possível carregar o usuário com ID '{_userManager.GetUserId(User)}'.");
            }

            _logger.LogInformation("Usuário com ID '{UserId}' solicitou download dos dados pessoais.", _userManager.GetUserId(User));

            // Coletar dados do usuário
            var personalData = new Dictionary<string, object>();

            // Informações básicas da conta
            personalData.Add("Id", await _userManager.GetUserIdAsync(user));
            personalData.Add("UserName", await _userManager.GetUserNameAsync(user) ?? "N/A");
            personalData.Add("Email", await _userManager.GetEmailAsync(user) ?? "N/A");
            personalData.Add("PhoneNumber", await _userManager.GetPhoneNumberAsync(user) ?? "N/A");
            personalData.Add("EmailConfirmed", await _userManager.IsEmailConfirmedAsync(user));
            personalData.Add("PhoneNumberConfirmed", await _userManager.IsPhoneNumberConfirmedAsync(user));
            personalData.Add("TwoFactorEnabled", await _userManager.GetTwoFactorEnabledAsync(user));
            personalData.Add("LockoutEnabled", await _userManager.GetLockoutEnabledAsync(user));
            personalData.Add("AccessFailedCount", await _userManager.GetAccessFailedCountAsync(user));

            // Dados adicionais (simulados)
            personalData.Add("AccountCreated", DateTime.Now.AddMonths(-6).ToString("yyyy-MM-dd HH:mm:ss"));
            personalData.Add("LastLogin", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            personalData.Add("ProfileCompleteness", "85%");
            personalData.Add("PreferredLanguage", "pt-BR");
            personalData.Add("TimeZone", "America/Sao_Paulo");

            // Configurações de privacidade (simuladas)
            personalData.Add("EmailNotifications", true);
            personalData.Add("SecurityAlerts", true);
            personalData.Add("MarketingEmails", false);

            // Dados de atividade (simulados)
            personalData.Add("TotalLogins", LoginCount);
            personalData.Add("LastLoginIP", "192.168.1.100");
            personalData.Add("ActivitiesCount", ActivityCount);
            personalData.Add("ConfigurationsCount", ConfigurationCount);

            // Converter para JSON
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var jsonData = JsonSerializer.Serialize(personalData, options);
            var bytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

            // Retornar arquivo para download
            var fileName = $"MatMob_DadosPessoais_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            return File(bytes, "application/json", fileName);
        }

        public async Task<IActionResult> OnPostDeletePersonalDataAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Não foi possível carregar o usuário com ID '{_userManager.GetUserId(User)}'.");
            }

            // Em um sistema real, você implementaria a lógica de exclusão completa dos dados
            _logger.LogWarning("Usuário com ID '{UserId}' solicitou exclusão da conta e dados pessoais.", _userManager.GetUserId(User));

            // Por enquanto, apenas desabilitar a conta
            var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            if (result.Succeeded)
            {
                _logger.LogInformation("Conta do usuário com ID '{UserId}' foi desabilitada.", _userManager.GetUserId(User));
                StatusMessage = "Sua solicitação de exclusão foi processada. Entre em contato com o suporte para finalização.";
                
                // Fazer logout do usuário
                await HttpContext.SignOutAsync();
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            StatusMessage = "Erro ao processar a solicitação de exclusão. Tente novamente ou entre em contato com o suporte.";
            return RedirectToPage();
        }
    }
}

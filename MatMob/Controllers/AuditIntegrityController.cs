using Microsoft.AspNetCore.Mvc;
using MatMob.Services;
using MatMob.Models.Entities;
using Microsoft.AspNetCore.Authorization;

namespace MatMob.Controllers
{
    [Authorize]
    public class AuditIntegrityController : Controller
    {
        private readonly AuditImmutabilityService _immutabilityService;
        private readonly IAuditService _auditService;
        private readonly ILogger<AuditIntegrityController> _logger;

        public AuditIntegrityController(
            AuditImmutabilityService immutabilityService, 
            IAuditService auditService, 
            ILogger<AuditIntegrityController> logger)
        {
            _immutabilityService = immutabilityService;
            _auditService = auditService;
            _logger = logger;
        }

        /// <summary>
        /// Página principal de verificação de integridade
        /// </summary>
        public async Task<IActionResult> Index()
        {
            ViewBag.Title = "Verificação de Integridade dos Logs";
            return View();
        }

        /// <summary>
        /// Verifica a integridade da cadeia de logs
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> VerifyChain(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                await _auditService.LogAsync("INTEGRITY_VERIFICATION_START", description: "Início da verificação de integridade da cadeia de logs");

                var result = await _immutabilityService.VerifyChainIntegrityAsync(startDate, endDate);

                await _auditService.LogAsync("INTEGRITY_VERIFICATION_COMPLETE", 
                    description: $"Verificação concluída: {result.TotalLogsChecked} logs verificados, {result.CorruptedLogs.Count} problemas encontrados");

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        isValid = result.IsValid,
                        totalLogsChecked = result.TotalLogsChecked,
                        corruptedLogsCount = result.CorruptedLogs.Count,
                        verificationDate = result.VerificationDate,
                        issues = result.CorruptedLogs.Select(issue => new
                        {
                            logId = issue.LogId,
                            sequenceNumber = issue.SequenceNumber,
                            issueType = issue.IssueType,
                            description = issue.Description
                        })
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar integridade da cadeia de logs");
                await _auditService.LogErrorAsync(ex, "Verificação de integridade");
                
                return Json(new
                {
                    success = false,
                    message = "Erro ao verificar integridade: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Reconstrói a cadeia de hash (USE COM CUIDADO)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> RebuildHashChain()
        {
            try
            {
                await _auditService.LogAsync("HASH_CHAIN_REBUILD_START", 
                    description: "Início da reconstrução da cadeia de hash", 
                    severity: AuditSeverity.WARNING);

                await _immutabilityService.RebuildHashChainAsync();

                await _auditService.LogAsync("HASH_CHAIN_REBUILD_COMPLETE", 
                    description: "Reconstrução da cadeia de hash concluída", 
                    severity: AuditSeverity.WARNING);

                return Json(new
                {
                    success = true,
                    message = "Cadeia de hash reconstruída com sucesso"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao reconstruir cadeia de hash");
                await _auditService.LogErrorAsync(ex, "Reconstrução de cadeia de hash");
                
                return Json(new
                {
                    success = false,
                    message = "Erro ao reconstruir cadeia de hash: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Testa a criação de logs com imutabilidade
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> TestImmutability()
        {
            try
            {
                // Criar alguns logs de teste
                await _auditService.LogAsync("TEST_LOG_1", 
                    description: "Log de teste 1 para verificação de imutabilidade");

                await _auditService.LogAsync("TEST_LOG_2", 
                    description: "Log de teste 2 para verificação de imutabilidade");

                await _auditService.LogAsync("TEST_LOG_3", 
                    description: "Log de teste 3 para verificação de imutabilidade");

                return Json(new
                {
                    success = true,
                    message = "Logs de teste criados com sucesso"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar logs de teste");
                return Json(new
                {
                    success = false,
                    message = "Erro ao criar logs de teste: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Exibe estatísticas de integridade
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetIntegrityStats()
        {
            try
            {
                var recentVerification = await _immutabilityService.VerifyChainIntegrityAsync(
                    DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        recentLogsChecked = recentVerification.TotalLogsChecked,
                        recentIssuesFound = recentVerification.CorruptedLogs.Count,
                        lastVerification = recentVerification.VerificationDate,
                        systemHealth = recentVerification.IsValid ? "Healthy" : "Issues Found"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas de integridade");
                return Json(new
                {
                    success = false,
                    message = "Erro ao obter estatísticas: " + ex.Message
                });
            }
        }
    }
}
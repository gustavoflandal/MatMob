using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using MatMob.Services;
using MatMob.Models.Entities;

namespace MatMob.Services
{
    public class AuthenticationAuditService
    {
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticationAuditService(IAuditService auditService, IHttpContextAccessor httpContextAccessor)
        {
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogLoginSuccessAsync(string userId, string userName)
        {
            var auditLog = new AuditLog
            {
                Action = AuditActions.LOGIN,
                EntityName = "Authentication",
                EntityId = int.TryParse(userId, out var id) ? id : null,
                Description = $"Login bem-sucedido para o usuário: {userName}",
                Category = AuditCategory.AUTHENTICATION,
                Severity = AuditSeverity.INFO,
                UserId = userId,
                UserName = userName,
                Success = true,
                CreatedAt = DateTime.UtcNow
            };

            await _auditService.LogAsync(auditLog);
        }

        public async Task LogLoginFailureAsync(string userName, string reason)
        {
            var auditLog = new AuditLog
            {
                Action = AuditActions.LOGIN_FAILED,
                EntityName = "Authentication",
                Description = $"Tentativa de login falhada para o usuário: {userName}. Motivo: {reason}",
                Category = AuditCategory.AUTHENTICATION,
                Severity = AuditSeverity.WARNING,
                UserName = userName,
                Success = false,
                ErrorMessage = reason,
                CreatedAt = DateTime.UtcNow
            };

            await _auditService.LogAsync(auditLog);
        }

        public async Task LogLogoutAsync(string userId, string userName)
        {
            var auditLog = new AuditLog
            {
                Action = AuditActions.LOGOUT,
                EntityName = "Authentication",
                EntityId = int.TryParse(userId, out var id) ? id : null,
                Description = $"Logout para o usuário: {userName}",
                Category = AuditCategory.AUTHENTICATION,
                Severity = AuditSeverity.INFO,
                UserId = userId,
                UserName = userName,
                Success = true,
                CreatedAt = DateTime.UtcNow
            };

            await _auditService.LogAsync(auditLog);
        }

        public async Task LogPasswordChangeAsync(string userId, string userName)
        {
            var auditLog = new AuditLog
            {
                Action = AuditActions.PASSWORD_CHANGE,
                EntityName = "Authentication",
                EntityId = int.TryParse(userId, out var id) ? id : null,
                Description = $"Senha alterada para o usuário: {userName}",
                Category = AuditCategory.AUTHENTICATION,
                Severity = AuditSeverity.INFO,
                UserId = userId,
                UserName = userName,
                Success = true,
                CreatedAt = DateTime.UtcNow
            };

            await _auditService.LogAsync(auditLog);
        }

        public async Task LogAccountLockoutAsync(string userId, string userName)
        {
            var auditLog = new AuditLog
            {
                Action = AuditActions.ACCOUNT_LOCKOUT,
                EntityName = "Authentication",
                EntityId = int.TryParse(userId, out var id) ? id : null,
                Description = $"Conta bloqueada para o usuário: {userName}",
                Category = AuditCategory.AUTHENTICATION,
                Severity = AuditSeverity.WARNING,
                UserId = userId,
                UserName = userName,
                Success = true,
                CreatedAt = DateTime.UtcNow
            };

            await _auditService.LogAsync(auditLog);
        }

        public async Task LogPasswordResetAsync(string userId, string userName)
        {
            var auditLog = new AuditLog
            {
                Action = AuditActions.PASSWORD_RESET,
                EntityName = "Authentication",
                EntityId = int.TryParse(userId, out var id) ? id : null,
                Description = $"Reset de senha para o usuário: {userName}",
                Category = AuditCategory.AUTHENTICATION,
                Severity = AuditSeverity.INFO,
                UserId = userId,
                UserName = userName,
                Success = true,
                CreatedAt = DateTime.UtcNow
            };

            await _auditService.LogAsync(auditLog);
        }
    }
}
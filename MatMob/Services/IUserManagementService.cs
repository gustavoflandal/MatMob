using Microsoft.AspNetCore.Identity;
using MatMob.Models.Entities;

namespace MatMob.Services
{
    public interface IUserManagementService
    {
        // User CRUD
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null);
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
        Task<ApplicationUser?> GetUserByEmailAsync(string email);
        Task<(ApplicationUser User, IdentityResult Result)> CreateUserAsync(ApplicationUser user, string password, string? createdBy = null);
        Task<IdentityResult> UpdateUserAsync(ApplicationUser user, string? updatedBy = null);
        Task<IdentityResult> DeleteUserAsync(string userId);
        Task<IdentityResult> ActivateUserAsync(string userId, string? updatedBy = null);
        Task<IdentityResult> DeactivateUserAsync(string userId, string? updatedBy = null);
        
        // Password Management
        Task<IdentityResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<IdentityResult> ResetPasswordAsync(string userId, string newPassword, string? updatedBy = null);
        Task<string> GeneratePasswordResetTokenAsync(string userId);
        Task<IdentityResult> ResetPasswordWithTokenAsync(string userId, string token, string newPassword);
        
        // Role Management
        Task<IEnumerable<ApplicationRole>> GetAllRolesAsync();
        Task<ApplicationRole?> GetRoleByIdAsync(string roleId);
        Task<ApplicationRole?> GetRoleByNameAsync(string roleName);
        Task<IdentityResult> CreateRoleAsync(ApplicationRole role, string? createdBy = null);
        Task<IdentityResult> UpdateRoleAsync(ApplicationRole role, string? updatedBy = null);
        Task<IdentityResult> DeleteRoleAsync(string roleId);
        
        // User-Role Assignment
        Task<IEnumerable<ApplicationRole>> GetUserRolesAsync(string userId);
        Task<IEnumerable<ApplicationUser>> GetUsersInRoleAsync(string roleName);
        Task<IdentityResult> AssignUserToRoleAsync(string userId, string roleName, string? assignedBy = null);
        Task<IdentityResult> RemoveUserFromRoleAsync(string userId, string roleName, string? removedBy = null);
        Task<bool> IsUserInRoleAsync(string userId, string roleName);
        
        // Statistics and Reporting
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetActiveUsersCountAsync();
        Task<int> GetInactiveUsersCountAsync();
        Task<Dictionary<string, int>> GetUsersByRoleCountAsync();
        Task<IEnumerable<ApplicationUser>> GetRecentlyCreatedUsersAsync(int count = 10);
        Task<IEnumerable<ApplicationUser>> GetRecentlyLoggedInUsersAsync(int count = 10);
        
        // Login Tracking
        Task UpdateLastLoginAsync(string userId, string? ipAddress = null);
        Task RecordFailedLoginAsync(string userId);
        Task ResetFailedLoginAttemptsAsync(string userId);
        
        // Utility
        Task<bool> EmailExistsAsync(string email, string? excludeUserId = null);
        Task<bool> UserNameExistsAsync(string userName, string? excludeUserId = null);
    }
}
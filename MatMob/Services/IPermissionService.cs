using MatMob.Models.Entities;

namespace MatMob.Services
{
    public interface IPermissionService
    {
        Task<IEnumerable<Permission>> GetAllPermissionsAsync();
        Task<Permission?> GetPermissionByIdAsync(int id);
        Task<Permission?> GetPermissionByCodeAsync(string code);
        Task<IEnumerable<Permission>> GetPermissionsByCategoryAsync(string category);
        Task<Permission> CreatePermissionAsync(Permission permission, string? createdBy = null);
        Task<Permission> UpdatePermissionAsync(Permission permission, string? updatedBy = null);
        Task<bool> DeletePermissionAsync(int id);
        
        // Role Permissions
        Task<IEnumerable<Permission>> GetRolePermissionsAsync(string roleId);
        Task<bool> AssignPermissionToRoleAsync(string roleId, int permissionId, string? grantedBy = null);
        Task<bool> RemovePermissionFromRoleAsync(string roleId, int permissionId);
        Task<bool> HasRolePermissionAsync(string roleId, string permissionCode);
        
        // User Permissions
        Task<IEnumerable<Permission>> GetUserPermissionsAsync(string userId);
        Task<IEnumerable<Permission>> GetUserAllPermissionsAsync(string userId); // Includes role permissions
        Task<bool> AssignPermissionToUserAsync(string userId, int permissionId, string? grantedBy = null, string? reason = null, DateTime? expiresAt = null);
        Task<bool> RemovePermissionFromUserAsync(string userId, int permissionId);
        Task<bool> HasUserPermissionAsync(string userId, string permissionCode);
        
        // Utility
        Task SeedDefaultPermissionsAsync();
        Task<Dictionary<string, List<Permission>>> GetPermissionsByCategoriesAsync();
    }
}
using Microsoft.EntityFrameworkCore;
using MatMob.Data;
using MatMob.Models.Entities;

namespace MatMob.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _context;

        public PermissionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Permission>> GetAllPermissionsAsync()
        {
            return await _context.Permissions
                .Where(p => p.IsActive)
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Permission?> GetPermissionByIdAsync(int id)
        {
            return await _context.Permissions
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        }

        public async Task<Permission?> GetPermissionByCodeAsync(string code)
        {
            return await _context.Permissions
                .FirstOrDefaultAsync(p => p.Code == code && p.IsActive);
        }

        public async Task<IEnumerable<Permission>> GetPermissionsByCategoryAsync(string category)
        {
            return await _context.Permissions
                .Where(p => p.Category == category && p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Permission> CreatePermissionAsync(Permission permission, string? createdBy = null)
        {
            permission.CreatedAt = DateTime.UtcNow;
            permission.CreatedBy = createdBy;
            permission.IsActive = true;

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();
            return permission;
        }

        public async Task<Permission> UpdatePermissionAsync(Permission permission, string? updatedBy = null)
        {
            permission.UpdatedAt = DateTime.UtcNow;
            permission.UpdatedBy = updatedBy;

            _context.Permissions.Update(permission);
            await _context.SaveChangesAsync();
            return permission;
        }

        public async Task<bool> DeletePermissionAsync(int id)
        {
            var permission = await GetPermissionByIdAsync(id);
            if (permission == null) return false;

            permission.IsActive = false;
            permission.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Permission>> GetRolePermissionsAsync(string roleId)
        {
            return await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId && rp.IsActive)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<bool> AssignPermissionToRoleAsync(string roleId, int permissionId, string? grantedBy = null)
        {
            var existingAssignment = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

            if (existingAssignment != null)
            {
                if (!existingAssignment.IsActive)
                {
                    existingAssignment.IsActive = true;
                    existingAssignment.GrantedAt = DateTime.UtcNow;
                    existingAssignment.GrantedBy = grantedBy;
                    await _context.SaveChangesAsync();
                }
                return true;
            }

            var rolePermission = new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId,
                GrantedAt = DateTime.UtcNow,
                GrantedBy = grantedBy,
                IsActive = true
            };

            _context.RolePermissions.Add(rolePermission);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemovePermissionFromRoleAsync(string roleId, int permissionId)
        {
            var rolePermission = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && rp.IsActive);

            if (rolePermission == null) return false;

            rolePermission.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasRolePermissionAsync(string roleId, string permissionCode)
        {
            return await _context.RolePermissions
                .Include(rp => rp.Permission)
                .AnyAsync(rp => rp.RoleId == roleId && 
                              rp.Permission.Code == permissionCode && 
                              rp.IsActive && 
                              rp.Permission.IsActive);
        }

        public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(string userId)
        {
            return await _context.UserPermissions
                .Where(up => up.UserId == userId && up.IsActive && 
                           (up.ExpiresAt == null || up.ExpiresAt > DateTime.UtcNow))
                .Include(up => up.Permission)
                .Select(up => up.Permission)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Permission>> GetUserAllPermissionsAsync(string userId)
        {
            // Get direct user permissions
            var userPermissions = await GetUserPermissionsAsync(userId);

            // Get role permissions
            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            var rolePermissions = await _context.RolePermissions
                .Where(rp => userRoles.Contains(rp.RoleId) && rp.IsActive)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission)
                .Where(p => p.IsActive)
                .ToListAsync();

            // Combine and deduplicate
            var allPermissions = userPermissions.Concat(rolePermissions)
                .GroupBy(p => p.Id)
                .Select(g => g.First())
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name);

            return allPermissions;
        }

        public async Task<bool> AssignPermissionToUserAsync(string userId, int permissionId, string? grantedBy = null, string? reason = null, DateTime? expiresAt = null)
        {
            var existingAssignment = await _context.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId);

            if (existingAssignment != null)
            {
                if (!existingAssignment.IsActive || 
                    (existingAssignment.ExpiresAt != null && existingAssignment.ExpiresAt <= DateTime.UtcNow))
                {
                    existingAssignment.IsActive = true;
                    existingAssignment.GrantedAt = DateTime.UtcNow;
                    existingAssignment.GrantedBy = grantedBy;
                    existingAssignment.Reason = reason;
                    existingAssignment.ExpiresAt = expiresAt;
                    await _context.SaveChangesAsync();
                }
                return true;
            }

            var userPermission = new UserPermission
            {
                UserId = userId,
                PermissionId = permissionId,
                GrantedAt = DateTime.UtcNow,
                GrantedBy = grantedBy,
                Reason = reason,
                ExpiresAt = expiresAt,
                IsActive = true
            };

            _context.UserPermissions.Add(userPermission);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemovePermissionFromUserAsync(string userId, int permissionId)
        {
            var userPermission = await _context.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId && up.IsActive);

            if (userPermission == null) return false;

            userPermission.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasUserPermissionAsync(string userId, string permissionCode)
        {
            // Check direct user permissions
            var hasDirectPermission = await _context.UserPermissions
                .Include(up => up.Permission)
                .AnyAsync(up => up.UserId == userId && 
                              up.Permission.Code == permissionCode && 
                              up.IsActive && 
                              up.Permission.IsActive &&
                              (up.ExpiresAt == null || up.ExpiresAt > DateTime.UtcNow));

            if (hasDirectPermission) return true;

            // Check role permissions
            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            var hasRolePermission = await _context.RolePermissions
                .Include(rp => rp.Permission)
                .AnyAsync(rp => userRoles.Contains(rp.RoleId) && 
                              rp.Permission.Code == permissionCode && 
                              rp.IsActive && 
                              rp.Permission.IsActive);

            return hasRolePermission;
        }

        public async Task SeedDefaultPermissionsAsync()
        {
            var permissionsByCategory = Permissions.GetAllPermissions();

            foreach (var category in permissionsByCategory)
            {
                foreach (var (code, name, description) in category.Value)
                {
                    var existingPermission = await _context.Permissions
                        .FirstOrDefaultAsync(p => p.Code == code);

                    if (existingPermission == null)
                    {
                        var permission = new Permission
                        {
                            Name = name,
                            Code = code,
                            Description = description,
                            Category = category.Key,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = "System"
                        };

                        _context.Permissions.Add(permission);
                    }
                    else if (!existingPermission.IsActive)
                    {
                        existingPermission.IsActive = true;
                        existingPermission.UpdatedAt = DateTime.UtcNow;
                        existingPermission.UpdatedBy = "System";
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<Dictionary<string, List<Permission>>> GetPermissionsByCategoriesAsync()
        {
            var permissions = await GetAllPermissionsAsync();
            
            return permissions
                .GroupBy(p => p.Category ?? "Outras")
                .ToDictionary(g => g.Key, g => g.ToList());
        }
    }
}
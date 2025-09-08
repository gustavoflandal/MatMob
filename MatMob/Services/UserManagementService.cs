using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MatMob.Data;
using MatMob.Models.Entities;

namespace MatMob.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UserManagementService(
            UserManager<ApplicationUser> userManager, 
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(u => 
                    u.Email!.ToLower().Contains(searchTerm) ||
                    (u.FirstName != null && u.FirstName.ToLower().Contains(searchTerm)) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(searchTerm)) ||
                    u.UserName!.ToLower().Contains(searchTerm));
            }

            return await query
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<(ApplicationUser User, IdentityResult Result)> CreateUserAsync(ApplicationUser user, string password, string? createdBy = null)
        {
            user.CreatedAt = DateTime.UtcNow;
            user.CreatedBy = createdBy;
            user.IsActive = true;
            user.EmailConfirmed = true; // Auto-confirm for now

            var result = await _userManager.CreateAsync(user, password);
            return (user, result);
        }

        public async Task<IdentityResult> UpdateUserAsync(ApplicationUser user, string? updatedBy = null)
        {
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = updatedBy;

            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> DeleteUserAsync(string userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Usuário não encontrado." });

            return await _userManager.DeleteAsync(user);
        }

        public async Task<IdentityResult> ActivateUserAsync(string userId, string? updatedBy = null)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Usuário não encontrado." });

            user.IsActive = true;
            user.LockoutEnd = null;
            return await UpdateUserAsync(user, updatedBy);
        }

        public async Task<IdentityResult> DeactivateUserAsync(string userId, string? updatedBy = null)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Usuário não encontrado." });

            user.IsActive = false;
            user.LockoutEnd = DateTimeOffset.MaxValue; // Lock permanently
            return await UpdateUserAsync(user, updatedBy);
        }

        public async Task<IdentityResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Usuário não encontrado." });

            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        public async Task<IdentityResult> ResetPasswordAsync(string userId, string newPassword, string? updatedBy = null)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Usuário não encontrado." });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("Usuário não encontrado.", nameof(userId));

            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<IdentityResult> ResetPasswordWithTokenAsync(string userId, string token, string newPassword)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Usuário não encontrado." });

            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }

        public async Task<IEnumerable<ApplicationRole>> GetAllRolesAsync()
        {
            return await _roleManager.Roles
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<ApplicationRole?> GetRoleByIdAsync(string roleId)
        {
            return await _roleManager.FindByIdAsync(roleId);
        }

        public async Task<ApplicationRole?> GetRoleByNameAsync(string roleName)
        {
            return await _roleManager.FindByNameAsync(roleName);
        }

        public async Task<IdentityResult> CreateRoleAsync(ApplicationRole role, string? createdBy = null)
        {
            role.CreatedAt = DateTime.UtcNow;
            role.CreatedBy = createdBy;
            role.IsActive = true;

            return await _roleManager.CreateAsync(role);
        }

        public async Task<IdentityResult> UpdateRoleAsync(ApplicationRole role, string? updatedBy = null)
        {
            role.UpdatedAt = DateTime.UtcNow;
            role.UpdatedBy = updatedBy;

            return await _roleManager.UpdateAsync(role);
        }

        public async Task<IdentityResult> DeleteRoleAsync(string roleId)
        {
            var role = await GetRoleByIdAsync(roleId);
            if (role == null)
                return IdentityResult.Failed(new IdentityError { Description = "Role não encontrado." });

            return await _roleManager.DeleteAsync(role);
        }

        public async Task<IEnumerable<ApplicationRole>> GetUserRolesAsync(string userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null) return new List<ApplicationRole>();

            var roleNames = await _userManager.GetRolesAsync(user);
            var roles = new List<ApplicationRole>();

            foreach (var roleName in roleNames)
            {
                var role = await GetRoleByNameAsync(roleName);
                if (role != null) roles.Add(role);
            }

            return roles;
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsersInRoleAsync(string roleName)
        {
            return await _userManager.GetUsersInRoleAsync(roleName);
        }

        public async Task<IdentityResult> AssignUserToRoleAsync(string userId, string roleName, string? assignedBy = null)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Usuário não encontrado." });

            var isInRole = await _userManager.IsInRoleAsync(user, roleName);
            if (isInRole)
                return IdentityResult.Success;

            return await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task<IdentityResult> RemoveUserFromRoleAsync(string userId, string roleName, string? removedBy = null)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Usuário não encontrado." });

            return await _userManager.RemoveFromRoleAsync(user, roleName);
        }

        public async Task<bool> IsUserInRoleAsync(string userId, string roleName)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null) return false;

            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _userManager.Users.CountAsync();
        }

        public async Task<int> GetActiveUsersCountAsync()
        {
            return await _userManager.Users.CountAsync(u => u.IsActive);
        }

        public async Task<int> GetInactiveUsersCountAsync()
        {
            return await _userManager.Users.CountAsync(u => !u.IsActive);
        }

        public async Task<Dictionary<string, int>> GetUsersByRoleCountAsync()
        {
            var roles = await GetAllRolesAsync();
            var result = new Dictionary<string, int>();

            foreach (var role in roles)
            {
                var usersInRole = await GetUsersInRoleAsync(role.Name!);
                result[role.Name!] = usersInRole.Count();
            }

            return result;
        }

        public async Task<IEnumerable<ApplicationUser>> GetRecentlyCreatedUsersAsync(int count = 10)
        {
            return await _userManager.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<ApplicationUser>> GetRecentlyLoggedInUsersAsync(int count = 10)
        {
            return await _userManager.Users
                .Where(u => u.LastLoginAt != null)
                .OrderByDescending(u => u.LastLoginAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task UpdateLastLoginAsync(string userId, string? ipAddress = null)
        {
            var user = await GetUserByIdAsync(userId);
            if (user != null)
            {
                user.LastLoginAt = DateTime.UtcNow;
                user.LastLoginIp = ipAddress;
                user.LoginAttempts = 0; // Reset failed attempts on successful login
                await UpdateUserAsync(user, "System");
            }
        }

        public async Task RecordFailedLoginAsync(string userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user != null)
            {
                user.LoginAttempts++;
                user.LastFailedLoginAt = DateTime.UtcNow;
                await UpdateUserAsync(user, "System");
            }
        }

        public async Task ResetFailedLoginAttemptsAsync(string userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user != null)
            {
                user.LoginAttempts = 0;
                await UpdateUserAsync(user, "System");
            }
        }

        public async Task<bool> EmailExistsAsync(string email, string? excludeUserId = null)
        {
            var query = _userManager.Users.Where(u => u.Email == email);

            if (!string.IsNullOrEmpty(excludeUserId))
            {
                query = query.Where(u => u.Id != excludeUserId);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> UserNameExistsAsync(string userName, string? excludeUserId = null)
        {
            var query = _userManager.Users.Where(u => u.UserName == userName);

            if (!string.IsNullOrEmpty(excludeUserId))
            {
                query = query.Where(u => u.Id != excludeUserId);
            }

            return await query.AnyAsync();
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MatMob.Models.ViewModels;
using MatMob.Services;
using MatMob.Models.Entities;

namespace MatMob.Controllers
{
    [Authorize]
    public class PermissionController : Controller
    {
        private readonly IPermissionService _permissionService;
        private readonly IUserManagementService _userManagementService;

        public PermissionController(
            IPermissionService permissionService,
            IUserManagementService userManagementService)
        {
            _permissionService = permissionService;
            _userManagementService = userManagementService;
        }

        // GET: Permission
        public async Task<IActionResult> Index()
        {
            try
            {
                var permissions = await _permissionService.GetAllPermissionsAsync();
                var groupedPermissions = permissions
                    .GroupBy(p => p.Category)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var viewModel = new PermissionListViewModel
                {
                    PermissionsByCategory = groupedPermissions,
                    Categories = groupedPermissions.Keys.ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao carregar permissões: {ex.Message}";
                return View(new PermissionListViewModel());
            }
        }

        // GET: Permission/Matrix
        public async Task<IActionResult> Matrix()
        {
            try
            {
                var roles = await _userManagementService.GetRolesAsync();
                var permissions = await _permissionService.GetAllPermissionsAsync();
                var rolePermissions = new Dictionary<string, List<Permission>>();

                foreach (var role in roles)
                {
                    var perms = await _permissionService.GetRolePermissionsAsync(role.Id);
                    rolePermissions[role.Id] = perms.ToList();
                }

                var viewModel = new PermissionMatrixViewModel
                {
                    Roles = roles.ToList(),
                    Permissions = permissions.ToList(),
                    RolePermissions = rolePermissions,
                    PermissionsByCategory = permissions.GroupBy(p => p.Category).ToDictionary(g => g.Key, g => g.ToList())
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao carregar matriz de permissões: {ex.Message}";
                return View(new PermissionMatrixViewModel());
            }
        }

        // GET: Permission/RoleManagement
        public async Task<IActionResult> RoleManagement()
        {
            try
            {
                var roles = await _userManagementService.GetRolesAsync();
                var permissions = await _permissionService.GetAllPermissionsAsync();

                var viewModel = new RoleManagementViewModel
                {
                    Roles = roles.ToList(),
                    AllPermissions = permissions.ToList(),
                    PermissionsByCategory = permissions.GroupBy(p => p.Category).ToDictionary(g => g.Key, g => g.ToList())
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao carregar gerenciamento de roles: {ex.Message}";
                return View(new RoleManagementViewModel());
            }
        }

        // GET: Permission/CreateRole
        public async Task<IActionResult> CreateRole()
        {
            try
            {
                var permissions = await _permissionService.GetAllPermissionsAsync();

                var viewModel = new CreateRoleViewModel
                {
                    AvailablePermissions = permissions.Select(p => new SelectListItem 
                    { 
                        Value = p.Id.ToString(), 
                        Text = $"{p.Name} ({p.Category})" 
                    }).ToList(),
                    PermissionsByCategory = permissions.GroupBy(p => p.Category).ToDictionary(g => g.Key, g => g.ToList())
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao carregar formulário de criação de role: {ex.Message}";
                return RedirectToAction(nameof(RoleManagement));
            }
        }

        // POST: Permission/CreateRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCreateRoleFormData(model);
                return View(model);
            }

            try
            {
                var role = new ApplicationRole
                {
                    Name = model.Name,
                    NormalizedName = model.Name.ToUpper(),
                    Description = model.Description,
                    Category = model.Category,
                    IsActive = model.IsActive
                };

                var result = await _userManagementService.CreateRoleAsync(role);
                
                if (result.Succeeded)
                {
                    // Adicionar permissões selecionadas à role
                    if (model.SelectedPermissionIds?.Any() == true)
                    {
                        foreach (var permissionId in model.SelectedPermissionIds)
                        {
                            await _permissionService.AddPermissionToRoleAsync(role.Id, permissionId);
                        }
                    }

                    TempData["SuccessMessage"] = "Role criada com sucesso!";
                    return RedirectToAction(nameof(EditRole), new { id = role.Id });
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Erro ao criar role: {ex.Message}");
            }

            await LoadCreateRoleFormData(model);
            return View(model);
        }

        // GET: Permission/EditRole/5
        public async Task<IActionResult> EditRole(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var role = await _userManagementService.GetRoleByIdAsync(id);
                if (role == null)
                {
                    return NotFound();
                }

                var rolePermissions = await _permissionService.GetRolePermissionsAsync(id);
                var allPermissions = await _permissionService.GetAllPermissionsAsync();

                var viewModel = new EditRoleViewModel
                {
                    Id = role.Id,
                    Name = role.Name,
                    Description = role.Description,
                    Category = role.Category,
                    IsActive = role.IsActive,
                    SelectedPermissionIds = rolePermissions.Select(p => p.Id).ToList(),
                    AvailablePermissions = allPermissions.Select(p => new SelectListItem 
                    { 
                        Value = p.Id.ToString(), 
                        Text = $"{p.Name} ({p.Category})" 
                    }).ToList(),
                    PermissionsByCategory = allPermissions.GroupBy(p => p.Category).ToDictionary(g => g.Key, g => g.ToList())
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao carregar role para edição: {ex.Message}";
                return RedirectToAction(nameof(RoleManagement));
            }
        }

        // POST: Permission/EditRole/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(string id, EditRoleViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await LoadEditRoleFormData(model);
                return View(model);
            }

            try
            {
                var role = await _userManagementService.GetRoleByIdAsync(id);
                if (role == null)
                {
                    return NotFound();
                }

                // Atualizar dados da role
                role.Name = model.Name;
                role.NormalizedName = model.Name.ToUpper();
                role.Description = model.Description;
                role.Category = model.Category;
                role.IsActive = model.IsActive;

                var result = await _userManagementService.UpdateRoleAsync(role);
                
                if (result.Succeeded)
                {
                    // Atualizar permissões da role
                    await _permissionService.UpdateRolePermissionsAsync(role.Id, model.SelectedPermissionIds ?? new List<int>());

                    TempData["SuccessMessage"] = "Role atualizada com sucesso!";
                    return RedirectToAction(nameof(EditRole), new { id = role.Id });
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Erro ao atualizar role: {ex.Message}");
            }

            await LoadEditRoleFormData(model);
            return View(model);
        }

        // POST: Permission/DeleteRole/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var role = await _userManagementService.GetRoleByIdAsync(id);
                if (role == null)
                {
                    return NotFound();
                }

                var result = await _userManagementService.DeleteRoleAsync(role);
                
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Role excluída com sucesso!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Erro ao excluir role: " + string.Join(", ", result.Errors.Select(e => e.Description));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao excluir role: {ex.Message}";
            }

            return RedirectToAction(nameof(RoleManagement));
        }

        // POST: Permission/ToggleRolePermission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleRolePermission(string roleId, int permissionId)
        {
            try
            {
                var hasPermission = await _permissionService.RoleHasPermissionAsync(roleId, permissionId);
                
                if (hasPermission)
                {
                    await _permissionService.RemovePermissionFromRoleAsync(roleId, permissionId);
                }
                else
                {
                    await _permissionService.AddPermissionToRoleAsync(roleId, permissionId);
                }

                return Json(new { success = true, hasPermission = !hasPermission });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Permission/UserPermissions/5
        public async Task<IActionResult> UserPermissions(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            try
            {
                var user = await _userManagementService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound();
                }

                var userRoles = await _userManagementService.GetUserRolesAsync(user);
                var userDirectPermissions = await _permissionService.GetUserPermissionsAsync(userId);
                var allRolePermissions = new List<Permission>();

                foreach (var role in userRoles)
                {
                    var rolePerms = await _permissionService.GetRolePermissionsAsync(role.Id);
                    allRolePermissions.AddRange(rolePerms);
                }

                var allUserPermissions = userDirectPermissions.Union(allRolePermissions).Distinct().ToList();
                var allPermissions = await _permissionService.GetAllPermissionsAsync();

                var viewModel = new UserPermissionsViewModel
                {
                    User = user,
                    UserRoles = userRoles.ToList(),
                    DirectPermissions = userDirectPermissions.ToList(),
                    RolePermissions = allRolePermissions.Distinct().ToList(),
                    AllUserPermissions = allUserPermissions,
                    AllPermissions = allPermissions.ToList(),
                    PermissionsByCategory = allPermissions.GroupBy(p => p.Category).ToDictionary(g => g.Key, g => g.ToList())
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao carregar permissões do usuário: {ex.Message}";
                return RedirectToAction("Index", "UserManagement");
            }
        }

        // GET: Permission/Statistics
        public async Task<IActionResult> Statistics()
        {
            try
            {
                var permissions = await _permissionService.GetAllPermissionsAsync();
                var roles = await _userManagementService.GetRolesAsync();
                var users = await _userManagementService.GetUsersAsync("", "", null, 1, int.MaxValue);

                var stats = new PermissionStatisticsViewModel
                {
                    TotalPermissions = permissions.Count(),
                    PermissionsByCategory = permissions.GroupBy(p => p.Category).ToDictionary(g => g.Key, g => g.Count()),
                    TotalRoles = roles.Count(),
                    ActiveRoles = roles.Count(r => r.IsActive),
                    TotalUsers = users.Count(),
                    UsersWithDirectPermissions = 0, // Será calculado dinamicamente
                    UsersWithRolePermissions = 0,   // Será calculado dinamicamente
                    RolePermissionCounts = new Dictionary<string, int>(),
                    MostUsedPermissions = new List<PermissionUsageInfo>()
                };

                // Calcular estatísticas mais detalhadas
                foreach (var role in roles)
                {
                    var rolePerms = await _permissionService.GetRolePermissionsAsync(role.Id);
                    stats.RolePermissionCounts[role.Name] = rolePerms.Count();
                }

                // Calcular usuários com permissões
                int usersWithDirectPerms = 0;
                int usersWithRolePerms = 0;
                var permissionUsage = new Dictionary<int, int>();

                foreach (var user in users)
                {
                    var directPerms = await _permissionService.GetUserPermissionsAsync(user.Id);
                    if (directPerms.Any())
                    {
                        usersWithDirectPerms++;
                        foreach (var perm in directPerms)
                        {
                            permissionUsage[perm.Id] = permissionUsage.GetValueOrDefault(perm.Id, 0) + 1;
                        }
                    }

                    var userRoles = await _userManagementService.GetUserRolesAsync(user);
                    if (userRoles.Any())
                    {
                        usersWithRolePerms++;
                        foreach (var role in userRoles)
                        {
                            var rolePerms = await _permissionService.GetRolePermissionsAsync(role.Id);
                            foreach (var perm in rolePerms)
                            {
                                permissionUsage[perm.Id] = permissionUsage.GetValueOrDefault(perm.Id, 0) + 1;
                            }
                        }
                    }
                }

                stats.UsersWithDirectPermissions = usersWithDirectPerms;
                stats.UsersWithRolePermissions = usersWithRolePerms;

                // Top 10 permissões mais usadas
                stats.MostUsedPermissions = permissionUsage
                    .OrderByDescending(x => x.Value)
                    .Take(10)
                    .Select(x => new PermissionUsageInfo
                    {
                        Permission = permissions.First(p => p.Id == x.Key),
                        UsageCount = x.Value
                    })
                    .ToList();

                return View(stats);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao carregar estatísticas: {ex.Message}";
                return View(new PermissionStatisticsViewModel());
            }
        }

        #region Helper Methods

        private async Task LoadCreateRoleFormData(CreateRoleViewModel model)
        {
            var permissions = await _permissionService.GetAllPermissionsAsync();

            model.AvailablePermissions = permissions.Select(p => new SelectListItem 
            { 
                Value = p.Id.ToString(), 
                Text = $"{p.Name} ({p.Category})" 
            }).ToList();
            
            model.PermissionsByCategory = permissions.GroupBy(p => p.Category).ToDictionary(g => g.Key, g => g.ToList());
        }

        private async Task LoadEditRoleFormData(EditRoleViewModel model)
        {
            var permissions = await _permissionService.GetAllPermissionsAsync();

            model.AvailablePermissions = permissions.Select(p => new SelectListItem 
            { 
                Value = p.Id.ToString(), 
                Text = $"{p.Name} ({p.Category})" 
            }).ToList();
            
            model.PermissionsByCategory = permissions.GroupBy(p => p.Category).ToDictionary(g => g.Key, g => g.ToList());
        }

        #endregion
    }
}
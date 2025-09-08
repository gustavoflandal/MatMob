using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MatMob.Models.ViewModels;
using MatMob.Services;
using MatMob.Models.Entities;

namespace MatMob.Controllers
{
    [Authorize]
    public class UserManagementController : Controller
    {
        private readonly IUserManagementService _userManagementService;
        private readonly IPermissionService _permissionService;

        public UserManagementController(
            IUserManagementService userManagementService,
            IPermissionService permissionService)
        {
            _userManagementService = userManagementService;
            _permissionService = permissionService;
        }

        // GET: UserManagement
        public async Task<IActionResult> Index(string searchTerm = "", string roleFilter = "", bool? isActiveFilter = null, int page = 1, int pageSize = 10)
        {
            try
            {
                var users = await _userManagementService.GetUsersAsync(searchTerm, roleFilter, isActiveFilter, page, pageSize);
                var roles = await _userManagementService.GetRolesAsync();
                
                var viewModel = new UserManagementListViewModel
                {
                    Users = users.ToList(),
                    SearchTerm = searchTerm,
                    RoleFilter = roleFilter,
                    IsActiveFilter = isActiveFilter,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalPages = await _userManagementService.GetUserCountAsync(searchTerm, roleFilter, isActiveFilter) / pageSize + 1,
                    AvailableRoles = roles.Select(r => new SelectListItem { Value = r.Name, Text = r.Name }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao carregar usuários: {ex.Message}";
                return View(new UserManagementListViewModel());
            }
        }

        // GET: UserManagement/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var user = await _userManagementService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                var userRoles = await _userManagementService.GetUserRolesAsync(user);
                var userPermissions = await _permissionService.GetUserPermissionsAsync(id);
                var allPermissions = await _permissionService.GetAllPermissionsAsync();

                var viewModel = new UserDetailsViewModel
                {
                    User = user,
                    Roles = userRoles.ToList(),
                    DirectPermissions = userPermissions.ToList(),
                    AllPermissions = allPermissions.ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao carregar detalhes do usuário: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: UserManagement/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var roles = await _userManagementService.GetRolesAsync();
                var permissions = await _permissionService.GetAllPermissionsAsync();

                var viewModel = new CreateUserViewModel
                {
                    AvailableRoles = roles.Select(r => new SelectListItem { Value = r.Id, Text = r.Name }).ToList(),
                    AvailablePermissions = permissions.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = $"{p.Name} ({p.Category})" }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao carregar formulário de criação: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: UserManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Recarregar listas para o formulário
                await LoadCreateFormData(model);
                return View(model);
            }

            try
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    IsActive = model.IsActive,
                    EmailConfirmed = true
                };

                var result = await _userManagementService.CreateUserAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    // Adicionar roles selecionadas
                    if (model.SelectedRoleIds?.Any() == true)
                    {
                        var roles = await _userManagementService.GetRolesByIdsAsync(model.SelectedRoleIds);
                        foreach (var role in roles)
                        {
                            await _userManagementService.AddToRoleAsync(user, role.Name);
                        }
                    }

                    // Adicionar permissões diretas selecionadas
                    if (model.SelectedPermissionIds?.Any() == true)
                    {
                        foreach (var permissionId in model.SelectedPermissionIds)
                        {
                            await _permissionService.AddPermissionToUserAsync(user.Id, permissionId);
                        }
                    }

                    TempData["SuccessMessage"] = "Usuário criado com sucesso!";
                    return RedirectToAction(nameof(Details), new { id = user.Id });
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
                ModelState.AddModelError(string.Empty, $"Erro ao criar usuário: {ex.Message}");
            }

            await LoadCreateFormData(model);
            return View(model);
        }

        // GET: UserManagement/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var user = await _userManagementService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                var userRoles = await _userManagementService.GetUserRolesAsync(user);
                var userPermissions = await _permissionService.GetUserPermissionsAsync(id);
                var allRoles = await _userManagementService.GetRolesAsync();
                var allPermissions = await _permissionService.GetAllPermissionsAsync();

                var viewModel = new EditUserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    SelectedRoleIds = userRoles.Select(r => r.Id).ToList(),
                    SelectedPermissionIds = userPermissions.Select(p => p.Id).ToList(),
                    AvailableRoles = allRoles.Select(r => new SelectListItem { Value = r.Id, Text = r.Name }).ToList(),
                    AvailablePermissions = allPermissions.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = $"{p.Name} ({p.Category})" }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao carregar usuário para edição: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: UserManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await LoadEditFormData(model);
                return View(model);
            }

            try
            {
                var user = await _userManagementService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // Atualizar dados básicos do usuário
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.IsActive = model.IsActive;
                
                // Se o email mudou, atualizar
                if (user.Email != model.Email)
                {
                    user.Email = model.Email;
                    user.UserName = model.Email;
                }

                var result = await _userManagementService.UpdateUserAsync(user);
                
                if (result.Succeeded)
                {
                    // Atualizar roles
                    await _userManagementService.UpdateUserRolesAsync(user, model.SelectedRoleIds ?? new List<string>());

                    // Atualizar permissões diretas
                    await _permissionService.UpdateUserPermissionsAsync(user.Id, model.SelectedPermissionIds ?? new List<int>());

                    TempData["SuccessMessage"] = "Usuário atualizado com sucesso!";
                    return RedirectToAction(nameof(Details), new { id = user.Id });
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
                ModelState.AddModelError(string.Empty, $"Erro ao atualizar usuário: {ex.Message}");
            }

            await LoadEditFormData(model);
            return View(model);
        }

        // POST: UserManagement/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var user = await _userManagementService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                var result = await _userManagementService.DeleteUserAsync(user);
                
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Usuário excluído com sucesso!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Erro ao excluir usuário: " + string.Join(", ", result.Errors.Select(e => e.Description));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao excluir usuário: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: UserManagement/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var user = await _userManagementService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                user.IsActive = !user.IsActive;
                var result = await _userManagementService.UpdateUserAsync(user);
                
                if (result.Succeeded)
                {
                    var status = user.IsActive ? "ativado" : "desativado";
                    TempData["SuccessMessage"] = $"Usuário {status} com sucesso!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Erro ao alterar status do usuário: " + string.Join(", ", result.Errors.Select(e => e.Description));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao alterar status do usuário: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: UserManagement/ResetPassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id, string newPassword)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(newPassword))
            {
                TempData["ErrorMessage"] = "ID do usuário ou nova senha não fornecidos.";
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                var user = await _userManagementService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                var result = await _userManagementService.ResetPasswordAsync(user, newPassword);
                
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Senha redefinida com sucesso!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Erro ao redefinir senha: " + string.Join(", ", result.Errors.Select(e => e.Description));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao redefinir senha: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: UserManagement/UserStatistics
        public async Task<IActionResult> UserStatistics()
        {
            try
            {
                var stats = await _userManagementService.GetUserStatisticsAsync();
                return View(stats);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao carregar estatísticas: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        #region Helper Methods

        private async Task LoadCreateFormData(CreateUserViewModel model)
        {
            var roles = await _userManagementService.GetRolesAsync();
            var permissions = await _permissionService.GetAllPermissionsAsync();

            model.AvailableRoles = roles.Select(r => new SelectListItem { Value = r.Id, Text = r.Name }).ToList();
            model.AvailablePermissions = permissions.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = $"{p.Name} ({p.Category})" }).ToList();
        }

        private async Task LoadEditFormData(EditUserViewModel model)
        {
            var roles = await _userManagementService.GetRolesAsync();
            var permissions = await _permissionService.GetAllPermissionsAsync();

            model.AvailableRoles = roles.Select(r => new SelectListItem { Value = r.Id, Text = r.Name }).ToList();
            model.AvailablePermissions = permissions.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = $"{p.Name} ({p.Category})" }).ToList();
        }

        #endregion
    }
}
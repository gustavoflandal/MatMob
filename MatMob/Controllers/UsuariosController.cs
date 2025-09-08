using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MatMob.Models.Entities;
using MatMob.Models.ViewModels;
using MatMob.Data;

namespace MatMob.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext context,
            ILogger<UsuariosController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            var usuarios = await _userManager.Users
                .Select(u => new UsuarioViewModel
                {
                    Id = u.Id,
                    Email = u.Email ?? "",
                    UserName = u.UserName ?? "",
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    IsActive = u.IsActive,
                    EmailConfirmed = u.EmailConfirmed,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt
                })
                .ToListAsync();

            // Buscar roles para cada usuário
            foreach (var usuario in usuarios)
            {
                var user = await _userManager.FindByIdAsync(usuario.Id);
                if (user != null)
                {
                    usuario.Roles = await _userManager.GetRolesAsync(user);
                }
            }

            return View(usuarios);
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await GetUserPermissionsAsync(user);

            var viewModel = new UsuarioDetailsViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                TwoFactorEnabled = user.TwoFactorEnabled,
                LockoutEnabled = user.LockoutEnabled,
                AccessFailedCount = user.AccessFailedCount,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginAt = user.LastLoginAt,
                LastLoginIp = user.LastLoginIp,
                LoginAttempts = user.LoginAttempts,
                Roles = roles,
                Permissions = permissions
            };

            return View(viewModel);
        }

        // GET: Usuarios/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateUsuarioViewModel();
            await CarregarRolesDisponiveisAsync(viewModel);
            return View(viewModel);
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUsuarioViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = User.Identity?.Name
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    // Adicionar roles selecionadas
                    if (model.SelectedRoles != null && model.SelectedRoles.Any())
                    {
                        await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                    }

                    _logger.LogInformation($"Usuário {user.Email} criado com sucesso por {User.Identity?.Name}");
                    TempData["Success"] = "Usuário criado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            await CarregarRolesDisponiveisAsync(model);
            return View(model);
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            var viewModel = new EditUsuarioViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                LockoutEnabled = user.LockoutEnabled,
                SelectedRoles = userRoles.ToList()
            };

            await CarregarRolesDisponiveisAsync(viewModel);
            return View(viewModel);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUsuarioViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                user.Email = model.Email;
                user.UserName = model.Email;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.IsActive = model.IsActive;
                user.EmailConfirmed = model.EmailConfirmed;
                user.TwoFactorEnabled = model.TwoFactorEnabled;
                user.LockoutEnabled = model.LockoutEnabled;
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = User.Identity?.Name;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // Atualizar roles
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    var selectedRoles = model.SelectedRoles ?? new List<string>();

                    var rolesToRemove = currentRoles.Except(selectedRoles);
                    var rolesToAdd = selectedRoles.Except(currentRoles);

                    if (rolesToRemove.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    }

                    if (rolesToAdd.Any())
                    {
                        await _userManager.AddToRolesAsync(user, rolesToAdd);
                    }

                    // Atualizar senha se fornecida
                    if (!string.IsNullOrEmpty(model.NewPassword))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                    }

                    _logger.LogInformation($"Usuário {user.Email} atualizado por {User.Identity?.Name}");
                    TempData["Success"] = "Usuário atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            await CarregarRolesDisponiveisAsync(model);
            return View(model);
        }

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var viewModel = new UsuarioViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                Roles = roles
            };

            return View(viewModel);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                // Verificar se não é o próprio usuário logado
                if (user.Id == _userManager.GetUserId(User))
                {
                    TempData["Error"] = "Você não pode excluir sua própria conta!";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"Usuário {user.Email} excluído por {User.Identity?.Name}");
                    TempData["Success"] = "Usuário excluído com sucesso!";
                }
                else
                {
                    TempData["Error"] = "Erro ao excluir usuário: " + string.Join(", ", result.Errors.Select(e => e.Description));
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Usuarios/ToggleStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = User.Identity?.Name;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                var status = user.IsActive ? "ativado" : "desativado";
                _logger.LogInformation($"Usuário {user.Email} {status} por {User.Identity?.Name}");
                TempData["Success"] = $"Usuário {status} com sucesso!";
            }
            else
            {
                TempData["Error"] = "Erro ao alterar status do usuário.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task CarregarRolesDisponiveisAsync(dynamic viewModel)
        {
            var roles = await _roleManager.Roles
                .Where(r => r.IsActive)
                .Select(r => new { r.Name, r.Description })
                .ToListAsync();

            viewModel.RolesDisponiveis = roles.ToDictionary(r => r.Name!, r => r.Description ?? r.Name!);
        }

        private async Task<List<string>> GetUserPermissionsAsync(ApplicationUser user)
        {
            var permissions = new List<string>();
            var userRoles = await _userManager.GetRolesAsync(user);

            foreach (var roleName in userRoles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    var rolePermissions = await _context.RolePermissions
                        .Where(rp => rp.RoleId == role.Id)
                        .Include(rp => rp.Permission)
                        .Select(rp => rp.Permission.Name)
                        .ToListAsync();

                    permissions.AddRange(rolePermissions);
                }
            }

            return permissions.Distinct().ToList();
        }
    }
}
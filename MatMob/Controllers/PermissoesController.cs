using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MatMob.Models.Entities;
using MatMob.Models.ViewModels;
using MatMob.Data;

namespace MatMob.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class PermissoesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PermissoesController> _logger;

        public PermissoesController(ApplicationDbContext context, ILogger<PermissoesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Permissoes
        public async Task<IActionResult> Index()
        {
            var viewModel = new PermissoesIndexViewModel();

            // Carregar permissões
            viewModel.Permissoes = await _context.Permissions
                .Select(p => new PermissaoViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Category = p.Category,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                })
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();

            // Carregar roles
            viewModel.Roles = await _context.Roles
                .Select(r => new RoleViewModel
                {
                    Id = r.Id,
                    Name = r.Name!,
                    Description = r.Description,
                    Category = r.Category,
                    IsActive = r.IsActive,
                    CreatedAt = r.CreatedAt
                })
                .OrderBy(r => r.Category)
                .ThenBy(r => r.Name)
                .ToListAsync();

            return View(viewModel);
        }

        // GET: Permissoes/Roles
        public IActionResult Roles()
        {
            // Redirecionar para a view Index que já mostra roles em uma aba separada
            return RedirectToAction(nameof(Index));
        }

        // GET: Permissoes/CreatePermission
        public IActionResult CreatePermission()
        {
            var viewModel = new CreatePermissionViewModel();
            return View(viewModel);
        }

        // POST: Permissoes/CreatePermission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePermission(CreatePermissionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var permission = new Permission
                {
                    Name = model.Name,
                    Description = model.Description,
                    Category = model.Module, // Usar Module como Category
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = User.Identity?.Name
                };

                _context.Permissions.Add(permission);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Permissão {permission.Name} criada por {User.Identity?.Name}");
                TempData["Success"] = "Permissão criada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Permissoes/CreateRole
        public async Task<IActionResult> CreateRole()
        {
            var viewModel = new CreateRoleViewModel();
            await CarregarPermissoesDisponiveisAsync(viewModel);

            // Passar permissões via ViewBag para a view
            var permissions = await _context.Permissions
                .Where(p => p.IsActive)
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();

            ViewBag.Permissions = permissions;
            return View(viewModel);
        }

        // POST: Permissoes/CreateRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var role = new ApplicationRole
                {
                    Name = model.Name,
                    Description = model.Description,
                    Category = "Geral", // Categoria padrão
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = User.Identity?.Name
                };

                _context.Roles.Add(role);
                await _context.SaveChangesAsync();

                // Adicionar permissões selecionadas
                if (model.SelectedPermissions != null && model.SelectedPermissions.Any())
                {
                    var rolePermissions = model.SelectedPermissions.Select(permissionId => new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permissionId,
                        GrantedAt = DateTime.UtcNow,
                        GrantedBy = User.Identity?.Name
                    });

                    _context.RolePermissions.AddRange(rolePermissions);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"Role {role.Name} criada por {User.Identity?.Name}");
                TempData["Success"] = "Role criada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            await CarregarPermissoesDisponiveisAsync(model);

            // Passar permissões via ViewBag para a view em caso de erro
            var permissions = await _context.Permissions
                .Where(p => p.IsActive)
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();

            ViewBag.Permissions = permissions;
            return View(model);
        }

        // GET: Permissoes/EditRole/5
        public async Task<IActionResult> EditRole(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
            {
                return NotFound();
            }

            var viewModel = new EditRoleViewModel
            {
                Id = role.Id,
                Name = role.Name!,
                Description = role.Description,
                IsActive = role.IsActive,
                SelectedPermissions = role.RolePermissions.Select(rp => rp.PermissionId).ToList()
            };

            await CarregarPermissoesDisponiveisAsync(viewModel);

            // Passar permissões via ViewBag para a view
            var permissions = await _context.Permissions
                .Where(p => p.IsActive)
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();

            ViewBag.Permissions = permissions;
            return View(viewModel);
        }

        // POST: Permissoes/EditRole/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(string id, EditRoleViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var role = await _context.Roles
                    .Include(r => r.RolePermissions)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (role == null)
                {
                    return NotFound();
                }

                role.Name = model.Name;
                role.Description = model.Description;
                role.Category = "Geral"; // Categoria padrão
                role.IsActive = model.IsActive;
                role.UpdatedAt = DateTime.UtcNow;
                role.UpdatedBy = User.Identity?.Name;

                // Atualizar permissões
                var currentPermissionIds = role.RolePermissions.Select(rp => rp.PermissionId).ToList();
                var selectedPermissionIds = model.SelectedPermissions ?? new List<int>();

                var permissionsToRemove = role.RolePermissions
                    .Where(rp => !selectedPermissionIds.Contains(rp.PermissionId))
                    .ToList();

                var permissionsToAdd = selectedPermissionIds
                    .Where(permissionId => !currentPermissionIds.Contains(permissionId))
                    .Select(permissionId => new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permissionId,
                        GrantedAt = DateTime.UtcNow,
                        GrantedBy = User.Identity?.Name
                    })
                    .ToList();

                _context.RolePermissions.RemoveRange(permissionsToRemove);
                _context.RolePermissions.AddRange(permissionsToAdd);

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Role {role.Name} atualizada por {User.Identity?.Name}");
                TempData["Success"] = "Role atualizada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            await CarregarPermissoesDisponiveisAsync(model);

            // Passar permissões via ViewBag para a view em caso de erro
            var permissions = await _context.Permissions
                .Where(p => p.IsActive)
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();

            ViewBag.Permissions = permissions;
            return View(model);
        }

        // GET: Permissoes/EditPermission/5
        public async Task<IActionResult> EditPermission(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null)
            {
                return NotFound();
            }

            var viewModel = new EditPermissionViewModel
            {
                Id = permission.Id,
                Name = permission.Name,
                Description = permission.Description,
                Module = permission.Category ?? string.Empty, // Mapear Category para Module
                IsActive = permission.IsActive
            };

            return View(viewModel);
        }

        // POST: Permissoes/EditPermission/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPermission(int id, EditPermissionViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var permission = await _context.Permissions.FindAsync(id);
                if (permission == null)
                {
                    return NotFound();
                }

                permission.Name = model.Name;
                permission.Description = model.Description;
                permission.Category = model.Module; // Mapear Module para Category
                permission.IsActive = model.IsActive;
                permission.UpdatedAt = DateTime.UtcNow;
                permission.UpdatedBy = User.Identity?.Name;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Permissão {permission.Name} atualizada por {User.Identity?.Name}");
                TempData["Success"] = "Permissão atualizada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // POST: Permissoes/TogglePermissionStatus/5
        [HttpPost]
        public async Task<IActionResult> TogglePermissionStatus(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null)
            {
                return NotFound();
            }

            permission.IsActive = !permission.IsActive;
            permission.UpdatedAt = DateTime.UtcNow;
            permission.UpdatedBy = User.Identity?.Name;

            await _context.SaveChangesAsync();

            var status = permission.IsActive ? "ativada" : "desativada";
            _logger.LogInformation($"Permissão {permission.Name} {status} por {User.Identity?.Name}");
            TempData["Success"] = $"Permissão {status} com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        // POST: Permissoes/ToggleRoleStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleRoleStatus(string id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            role.IsActive = !role.IsActive;
            role.UpdatedAt = DateTime.UtcNow;
            role.UpdatedBy = User.Identity?.Name;

            await _context.SaveChangesAsync();

            var status = role.IsActive ? "ativada" : "desativada";
            _logger.LogInformation($"Role {role.Name} {status} por {User.Identity?.Name}");
            TempData["Success"] = $"Role {status} com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        private async Task CarregarPermissoesDisponiveisAsync(dynamic viewModel)
        {
            var permissions = await _context.Permissions
                .Where(p => p.IsActive)
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();

            viewModel.PermissoesDisponiveis = permissions.GroupBy(p => p.Category ?? "Geral")
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        private void CarregarModulosPermissoes(dynamic viewModel)
        {
            // Mapeamento dos módulos disponíveis
            var modulos = new List<string>
            {
                "Usuarios",
                "Tecnicos",
                "Equipes",
                "Ativos",
                "Pecas",
                "OrdensServico",
                "Dashboard",
                "Relatorios",
                "Configuracoes"
            };

            viewModel.ModulosDisponiveis = modulos;
        }
    }
}
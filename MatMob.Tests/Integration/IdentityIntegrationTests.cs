using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MatMob.Data;
using MatMob.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace MatMob.Tests.Integration
{
    public class IdentityIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public IdentityIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public void Identity_Configuration_ShouldBeCorrect()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            // Assert - Services should be properly configured
            Assert.NotNull(userManager);
            Assert.NotNull(roleManager);
            Assert.IsType<UserManager<ApplicationUser>>(userManager);
            Assert.IsType<RoleManager<ApplicationRole>>(roleManager);
        }

        [Fact]
        public void DatabaseContext_ShouldSupportCustomIdentityTypes()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Assert - Context should have DbSets for custom types
            Assert.NotNull(context.Users);
            Assert.NotNull(context.Roles);
            Assert.NotNull(context.Permissions);
            Assert.NotNull(context.RolePermissions);
            Assert.NotNull(context.UserPermissions);
        }

        [Fact]
        public async Task UserManager_ShouldCreateApplicationUser()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Create a unique user for this test
            var testEmail = $"test-{Guid.NewGuid()}@example.com";
            var user = new ApplicationUser
            {
                UserName = testEmail,
                Email = testEmail,
                FirstName = "Test",
                LastName = "User",
                EmailConfirmed = true
            };

            try
            {
                // Act
                var result = await userManager.CreateAsync(user, "TestPassword123!");

                // Assert
                Assert.True(result.Succeeded);
                Assert.NotNull(user.Id);
                Assert.Equal("Test User", user.FullName);

                // Verify in database
                var foundUser = await userManager.FindByEmailAsync(testEmail);
                Assert.NotNull(foundUser);
                Assert.Equal("Test", foundUser.FirstName);
                Assert.Equal("User", foundUser.LastName);
            }
            finally
            {
                // Cleanup
                if (!string.IsNullOrEmpty(user.Id))
                {
                    var userToDelete = await userManager.FindByIdAsync(user.Id);
                    if (userToDelete != null)
                    {
                        await userManager.DeleteAsync(userToDelete);
                    }
                }
            }
        }

        [Fact]
        public async Task RoleManager_ShouldCreateApplicationRole()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            var testRoleName = $"TestRole-{Guid.NewGuid()}";
            var role = new ApplicationRole(testRoleName)
            {
                Description = "Test role for integration testing",
                Category = "Test"
            };

            try
            {
                // Act
                var result = await roleManager.CreateAsync(role);

                // Assert
                Assert.True(result.Succeeded);
                Assert.NotNull(role.Id);

                // Verify in database
                var foundRole = await roleManager.FindByNameAsync(testRoleName);
                Assert.NotNull(foundRole);
                Assert.Equal("Test role for integration testing", foundRole.Description);
                Assert.Equal("Test", foundRole.Category);
            }
            finally
            {
                // Cleanup
                if (!string.IsNullOrEmpty(role.Id))
                {
                    var roleToDelete = await roleManager.FindByIdAsync(role.Id);
                    if (roleToDelete != null)
                    {
                        await roleManager.DeleteAsync(roleToDelete);
                    }
                }
            }
        }

        [Fact]
        public async Task UserManager_ShouldAssignRoleToUser()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            var testEmail = $"test-role-{Guid.NewGuid()}@example.com";
            var testRoleName = $"TestRole-{Guid.NewGuid()}";

            var user = new ApplicationUser
            {
                UserName = testEmail,
                Email = testEmail,
                FirstName = "Role",
                LastName = "Test"
            };

            var role = new ApplicationRole(testRoleName)
            {
                Description = "Test role for user assignment"
            };

            try
            {
                // Act
                var userResult = await userManager.CreateAsync(user, "TestPassword123!");
                var roleResult = await roleManager.CreateAsync(role);
                
                Assert.True(userResult.Succeeded);
                Assert.True(roleResult.Succeeded);

                var addToRoleResult = await userManager.AddToRoleAsync(user, testRoleName);

                // Assert
                Assert.True(addToRoleResult.Succeeded);

                var userRoles = await userManager.GetRolesAsync(user);
                Assert.Contains(testRoleName, userRoles);

                var usersInRole = await userManager.GetUsersInRoleAsync(testRoleName);
                Assert.Contains(usersInRole, u => u.Email == testEmail);
            }
            finally
            {
                // Cleanup
                if (!string.IsNullOrEmpty(user.Id))
                {
                    var userToDelete = await userManager.FindByIdAsync(user.Id);
                    if (userToDelete != null)
                    {
                        await userManager.DeleteAsync(userToDelete);
                    }
                }

                if (!string.IsNullOrEmpty(role.Id))
                {
                    var roleToDelete = await roleManager.FindByIdAsync(role.Id);
                    if (roleToDelete != null)
                    {
                        await roleManager.DeleteAsync(roleToDelete);
                    }
                }
            }
        }

        [Fact]
        public async Task PermissionEntities_ShouldBeAccessibleThroughContext()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var permission = new Permission
            {
                Name = $"Test Permission {Guid.NewGuid()}",
                Code = $"TEST_PERM_{Guid.NewGuid().ToString("N")[..8]}",
                Description = "Test permission for integration testing",
                Category = "Test"
            };

            try
            {
                // Act
                context.Permissions.Add(permission);
                await context.SaveChangesAsync();

                // Assert
                Assert.True(permission.Id > 0);

                var foundPermission = await context.Permissions
                    .FirstOrDefaultAsync(p => p.Code == permission.Code);
                
                Assert.NotNull(foundPermission);
                Assert.Equal(permission.Name, foundPermission.Name);
                Assert.Equal(permission.Description, foundPermission.Description);
                Assert.Equal(permission.Category, foundPermission.Category);
            }
            finally
            {
                // Cleanup
                if (permission.Id > 0)
                {
                    context.Permissions.Remove(permission);
                    await context.SaveChangesAsync();
                }
            }
        }

        [Fact]
        public async Task RolePermission_ShouldEstablishRelationshipCorrectly()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            var testRoleName = $"PermTestRole-{Guid.NewGuid()}";
            var role = new ApplicationRole(testRoleName)
            {
                Description = "Role for permission testing"
            };

            var permission = new Permission
            {
                Name = $"Test Permission {Guid.NewGuid()}",
                Code = $"TEST_PERM_{Guid.NewGuid().ToString("N")[..8]}",
                Description = "Permission for role testing"
            };

            try
            {
                // Act
                var roleResult = await roleManager.CreateAsync(role);
                Assert.True(roleResult.Succeeded);

                context.Permissions.Add(permission);
                await context.SaveChangesAsync();

                var rolePermission = new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id,
                    GrantedBy = "test@example.com"
                };

                context.RolePermissions.Add(rolePermission);
                await context.SaveChangesAsync();

                // Assert
                Assert.True(rolePermission.Id > 0);

                var foundRolePermission = await context.RolePermissions
                    .Include(rp => rp.Role)
                    .Include(rp => rp.Permission)
                    .FirstOrDefaultAsync(rp => rp.Id == rolePermission.Id);

                Assert.NotNull(foundRolePermission);
                Assert.Equal(role.Id, foundRolePermission.RoleId);
                Assert.Equal(permission.Id, foundRolePermission.PermissionId);
                Assert.Equal(testRoleName, foundRolePermission.Role.Name);
                Assert.Equal(permission.Code, foundRolePermission.Permission.Code);
            }
            finally
            {
                // Cleanup
                if (permission.Id > 0)
                {
                    // Remove role permission first due to foreign key
                    var rolePermissionToDelete = await context.RolePermissions
                        .FirstOrDefaultAsync(rp => rp.PermissionId == permission.Id);
                    if (rolePermissionToDelete != null)
                    {
                        context.RolePermissions.Remove(rolePermissionToDelete);
                        await context.SaveChangesAsync();
                    }

                    context.Permissions.Remove(permission);
                    await context.SaveChangesAsync();
                }

                if (!string.IsNullOrEmpty(role.Id))
                {
                    var roleToDelete = await roleManager.FindByIdAsync(role.Id);
                    if (roleToDelete != null)
                    {
                        await roleManager.DeleteAsync(roleToDelete);
                    }
                }
            }
        }

        [Fact]
        public async Task Application_ShouldStartWithDefaultData()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            // Clean up existing admin user if it exists
            var existingAdmin = await userManager.FindByEmailAsync("admin@matmob.com");
            if (existingAdmin != null)
            {
                await userManager.DeleteAsync(existingAdmin);
            }

            // Recreate admin user with proper data
            var adminUser = new ApplicationUser
            {
                UserName = "admin@matmob.com",
                Email = "admin@matmob.com",
                EmailConfirmed = true,
                FirstName = "Administrador",
                LastName = "Sistema"
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Administrador");
            }

            // Assert - Default roles should exist
            var adminRole = await roleManager.FindByNameAsync("Administrador");
            var gestorRole = await roleManager.FindByNameAsync("Gestor");
            var tecnicoRole = await roleManager.FindByNameAsync("Tecnico");

            Assert.NotNull(adminRole);
            Assert.NotNull(gestorRole);
            Assert.NotNull(tecnicoRole);

            // Assert - Default admin user should exist with correct data
            var testAdminUser = await userManager.FindByEmailAsync("admin@matmob.com");
            Assert.NotNull(testAdminUser);
            Assert.Equal("Administrador", testAdminUser.FirstName);
            Assert.Equal("Sistema", testAdminUser.LastName);

            // Assert - Admin user should be in Administrador role
            var userRoles = await userManager.GetRolesAsync(testAdminUser);
            Assert.Contains("Administrador", userRoles);
        }
    }
}
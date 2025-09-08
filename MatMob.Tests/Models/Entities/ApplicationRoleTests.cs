using Microsoft.AspNetCore.Identity;
using MatMob.Models.Entities;
using Xunit;

namespace MatMob.Tests.Models.Entities
{
    public class ApplicationRoleTests
    {
        [Fact]
        public void ApplicationRole_Constructor_ShouldInheritFromIdentityRole()
        {
            // Arrange & Act
            var role = new ApplicationRole();

            // Assert
            Assert.IsAssignableFrom<IdentityRole>(role);
        }

        [Fact]
        public void ApplicationRole_Constructor_WithRoleName_ShouldSetName()
        {
            // Arrange
            var roleName = "Administrador";

            // Act
            var role = new ApplicationRole(roleName);

            // Assert
            Assert.Equal(roleName, role.Name);
        }

        [Fact]
        public void ApplicationRole_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var role = new ApplicationRole();

            // Assert
            Assert.True(role.IsActive);
            Assert.NotEqual(default(DateTime), role.CreatedAt);
            Assert.True(role.CreatedAt <= DateTime.UtcNow);
            Assert.Empty(role.RolePermissions);
        }

        [Fact]
        public void ApplicationRole_SetProperties_ShouldWorkCorrectly()
        {
            // Arrange
            var role = new ApplicationRole();
            var description = "Administrador do sistema";
            var category = "Sistema";
            var createdBy = "admin@test.com";
            var updatedBy = "manager@test.com";

            // Act
            role.Description = description;
            role.Category = category;
            role.CreatedBy = createdBy;
            role.UpdatedBy = updatedBy;
            role.IsActive = false;

            // Assert
            Assert.Equal(description, role.Description);
            Assert.Equal(category, role.Category);
            Assert.Equal(createdBy, role.CreatedBy);
            Assert.Equal(updatedBy, role.UpdatedBy);
            Assert.False(role.IsActive);
        }

        [Fact]
        public void ApplicationRole_UpdatedAt_ShouldBeNullByDefault()
        {
            // Arrange & Act
            var role = new ApplicationRole();

            // Assert
            Assert.Null(role.UpdatedAt);
        }

        [Fact]
        public void ApplicationRole_RolePermissions_ShouldInitializeAsEmptyCollection()
        {
            // Arrange & Act
            var role = new ApplicationRole();

            // Assert
            Assert.NotNull(role.RolePermissions);
            Assert.Empty(role.RolePermissions);
            Assert.IsType<List<RolePermission>>(role.RolePermissions);
        }

        [Theory]
        [InlineData("Administrador", "Administrador do sistema", "Sistema")]
        [InlineData("Gestor", "Gestor de manutenção", "Operacional")]
        [InlineData("Tecnico", "Técnico de campo", "Operacional")]
        [InlineData("Supervisor", "Supervisor de equipes", "Gerencial")]
        public void ApplicationRole_SetMultipleProperties_ShouldWorkCorrectly(string name, string description, string category)
        {
            // Arrange
            var role = new ApplicationRole(name);

            // Act
            role.Description = description;
            role.Category = category;

            // Assert
            Assert.Equal(name, role.Name);
            Assert.Equal(description, role.Description);
            Assert.Equal(category, role.Category);
        }

        [Fact]
        public void ApplicationRole_StringLengthLimits_ShouldRespectMaxLengths()
        {
            // Arrange
            var role = new ApplicationRole();
            var longString50 = new string('A', 50);
            var longString450 = new string('B', 450);
            var longString500 = new string('C', 500);

            // Act
            role.Category = longString50;
            role.Description = longString500;
            role.CreatedBy = longString450;
            role.UpdatedBy = longString450;

            // Assert - These should not throw exceptions as they're within limits
            Assert.Equal(longString50, role.Category);
            Assert.Equal(longString500, role.Description);
            Assert.Equal(longString450, role.CreatedBy);
            Assert.Equal(longString450, role.UpdatedBy);
        }

        [Fact]
        public void ApplicationRole_CreatedAt_ShouldBeRecentTime()
        {
            // Arrange
            var beforeCreation = DateTime.UtcNow;

            // Act
            var role = new ApplicationRole();
            var afterCreation = DateTime.UtcNow;

            // Assert
            Assert.True(role.CreatedAt >= beforeCreation);
            Assert.True(role.CreatedAt <= afterCreation);
        }

        [Fact]
        public void ApplicationRole_UpdatedAt_CanBeSet()
        {
            // Arrange
            var role = new ApplicationRole();
            var updateTime = DateTime.UtcNow.AddMinutes(5);

            // Act
            role.UpdatedAt = updateTime;

            // Assert
            Assert.Equal(updateTime, role.UpdatedAt);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void ApplicationRole_Description_CanBeNullOrEmpty(string? description)
        {
            // Arrange
            var role = new ApplicationRole();

            // Act
            role.Description = description;

            // Assert
            Assert.Equal(description, role.Description);
        }

        [Fact]
        public void ApplicationRole_Description_CanBeNull()
        {
            // Arrange
            var role = new ApplicationRole();

            // Act
            role.Description = null;

            // Assert
            Assert.Null(role.Description);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void ApplicationRole_Category_CanBeNullOrEmpty(string? category)
        {
            // Arrange
            var role = new ApplicationRole();

            // Act
            role.Category = category;

            // Assert
            Assert.Equal(category, role.Category);
        }

        [Fact]
        public void ApplicationRole_Category_CanBeNull()
        {
            // Arrange
            var role = new ApplicationRole();

            // Act
            role.Category = null;

            // Assert
            Assert.Null(role.Category);
        }

        [Fact]
        public void ApplicationRole_IsActive_CanBeToggled()
        {
            // Arrange
            var role = new ApplicationRole();
            
            // Confirm default
            Assert.True(role.IsActive);

            // Act & Assert - toggle to false
            role.IsActive = false;
            Assert.False(role.IsActive);

            // Act & Assert - toggle back to true
            role.IsActive = true;
            Assert.True(role.IsActive);
        }

        [Fact]
        public void ApplicationRole_Constructor_EmptyConstructor_ShouldNotSetName()
        {
            // Arrange & Act
            var role = new ApplicationRole();

            // Assert
            Assert.Null(role.Name);
        }

        [Fact]
        public void ApplicationRole_Constructor_WithRoleName_ShouldOnlySetName()
        {
            // Arrange
            var roleName = "TestRole";

            // Act
            var role = new ApplicationRole(roleName);

            // Assert
            Assert.Equal(roleName, role.Name);
            Assert.Null(role.Description);
            Assert.Null(role.Category);
            Assert.True(role.IsActive); // Default value
        }
    }
}
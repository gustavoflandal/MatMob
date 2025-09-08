using MatMob.Models.Entities;
using Xunit;

namespace MatMob.Tests.Models.Entities
{
    public class PermissionTests
    {
        [Fact]
        public void Permission_Constructor_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var permission = new Permission();

            // Assert
            Assert.Equal(0, permission.Id);
            Assert.Equal(string.Empty, permission.Name);
            Assert.Equal(string.Empty, permission.Code);
            Assert.Null(permission.Description);
            Assert.Null(permission.Category);
            Assert.True(permission.IsActive);
            Assert.NotEqual(default(DateTime), permission.CreatedAt);
            Assert.True(permission.CreatedAt <= DateTime.UtcNow);
            Assert.Null(permission.CreatedBy);
            Assert.Null(permission.UpdatedAt);
            Assert.Null(permission.UpdatedBy);
            Assert.NotNull(permission.RolePermissions);
            Assert.Empty(permission.RolePermissions);
            Assert.NotNull(permission.UserPermissions);
            Assert.Empty(permission.UserPermissions);
        }

        [Fact]
        public void Permission_SetProperties_ShouldWorkCorrectly()
        {
            // Arrange
            var permission = new Permission();
            var name = "Gerenciar Usuários";
            var code = "USER_MANAGE";
            var description = "Permite gerenciar usuários do sistema";
            var category = "Usuários";
            var createdBy = "admin@test.com";
            var updatedBy = "manager@test.com";

            // Act
            permission.Name = name;
            permission.Code = code;
            permission.Description = description;
            permission.Category = category;
            permission.CreatedBy = createdBy;
            permission.UpdatedBy = updatedBy;
            permission.IsActive = false;

            // Assert
            Assert.Equal(name, permission.Name);
            Assert.Equal(code, permission.Code);
            Assert.Equal(description, permission.Description);
            Assert.Equal(category, permission.Category);
            Assert.Equal(createdBy, permission.CreatedBy);
            Assert.Equal(updatedBy, permission.UpdatedBy);
            Assert.False(permission.IsActive);
        }

        [Theory]
        [InlineData("Visualizar Dashboard", "DASHBOARD_VIEW", "Dashboard", "Acesso ao dashboard principal")]
        [InlineData("Criar Ordens", "ORDER_CREATE", "Ordens", "Criação de ordens de serviço")]
        [InlineData("Editar Técnicos", "TECH_EDIT", "Técnicos", "Edição de dados de técnicos")]
        public void Permission_SetMultipleProperties_ShouldWorkCorrectly(string name, string code, string category, string description)
        {
            // Arrange
            var permission = new Permission();

            // Act
            permission.Name = name;
            permission.Code = code;
            permission.Category = category;
            permission.Description = description;

            // Assert
            Assert.Equal(name, permission.Name);
            Assert.Equal(code, permission.Code);
            Assert.Equal(category, permission.Category);
            Assert.Equal(description, permission.Description);
        }

        [Fact]
        public void Permission_StringLengthLimits_ShouldRespectMaxLengths()
        {
            // Arrange
            var permission = new Permission();
            var longString50 = new string('A', 50);
            var longString100 = new string('B', 100);
            var longString450 = new string('C', 450);
            var longString500 = new string('D', 500);

            // Act
            permission.Name = longString100;
            permission.Code = longString100;
            permission.Category = longString50;
            permission.Description = longString500;
            permission.CreatedBy = longString450;
            permission.UpdatedBy = longString450;

            // Assert - These should not throw exceptions as they're within limits
            Assert.Equal(longString100, permission.Name);
            Assert.Equal(longString100, permission.Code);
            Assert.Equal(longString50, permission.Category);
            Assert.Equal(longString500, permission.Description);
            Assert.Equal(longString450, permission.CreatedBy);
            Assert.Equal(longString450, permission.UpdatedBy);
        }

        [Fact]
        public void Permission_CreatedAt_ShouldBeRecentTime()
        {
            // Arrange
            var beforeCreation = DateTime.UtcNow;

            // Act
            var permission = new Permission();
            var afterCreation = DateTime.UtcNow;

            // Assert
            Assert.True(permission.CreatedAt >= beforeCreation);
            Assert.True(permission.CreatedAt <= afterCreation);
        }

        [Fact]
        public void Permission_Collections_ShouldInitializeAsEmptyCollections()
        {
            // Arrange & Act
            var permission = new Permission();

            // Assert
            Assert.NotNull(permission.RolePermissions);
            Assert.Empty(permission.RolePermissions);
            Assert.IsType<List<RolePermission>>(permission.RolePermissions);

            Assert.NotNull(permission.UserPermissions);
            Assert.Empty(permission.UserPermissions);
            Assert.IsType<List<UserPermission>>(permission.UserPermissions);
        }

        [Fact]
        public void Permission_UpdatedAt_CanBeSet()
        {
            // Arrange
            var permission = new Permission();
            var updateTime = DateTime.UtcNow.AddMinutes(5);

            // Act
            permission.UpdatedAt = updateTime;

            // Assert
            Assert.Equal(updateTime, permission.UpdatedAt);
        }

        [Fact]
        public void Permission_IsActive_CanBeToggled()
        {
            // Arrange
            var permission = new Permission();
            
            // Confirm default
            Assert.True(permission.IsActive);

            // Act & Assert - toggle to false
            permission.IsActive = false;
            Assert.False(permission.IsActive);

            // Act & Assert - toggle back to true
            permission.IsActive = true;
            Assert.True(permission.IsActive);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Permission_OptionalFields_CanBeNullOrEmpty(string? value)
        {
            // Arrange
            var permission = new Permission();

            // Act
            permission.Description = value;
            permission.Category = value;
            permission.CreatedBy = value;
            permission.UpdatedBy = value;

            // Assert
            Assert.Equal(value, permission.Description);
            Assert.Equal(value, permission.Category);
            Assert.Equal(value, permission.CreatedBy);
            Assert.Equal(value, permission.UpdatedBy);
        }

        [Fact]
        public void Permission_OptionalFields_CanBeNull()
        {
            // Arrange
            var permission = new Permission();

            // Act
            permission.Description = null;
            permission.Category = null;
            permission.CreatedBy = null;
            permission.UpdatedBy = null;

            // Assert
            Assert.Null(permission.Description);
            Assert.Null(permission.Category);
            Assert.Null(permission.CreatedBy);
            Assert.Null(permission.UpdatedBy);
        }
    }
}
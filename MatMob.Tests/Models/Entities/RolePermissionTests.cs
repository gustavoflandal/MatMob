using MatMob.Models.Entities;
using Xunit;

namespace MatMob.Tests.Models.Entities
{
    public class RolePermissionTests
    {
        [Fact]
        public void RolePermission_Constructor_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var rolePermission = new RolePermission();

            // Assert
            Assert.Equal(0, rolePermission.Id);
            Assert.Equal(string.Empty, rolePermission.RoleId);
            Assert.Equal(0, rolePermission.PermissionId);
            Assert.NotEqual(default(DateTime), rolePermission.GrantedAt);
            Assert.True(rolePermission.GrantedAt <= DateTime.UtcNow);
            Assert.Null(rolePermission.GrantedBy);
            Assert.True(rolePermission.IsActive);
            // Navigation properties will be null until set
        }

        [Fact]
        public void RolePermission_SetProperties_ShouldWorkCorrectly()
        {
            // Arrange
            var rolePermission = new RolePermission();
            var roleId = "role-123";
            var permissionId = 456;
            var grantedBy = "admin@test.com";
            var grantedAt = DateTime.UtcNow.AddHours(-1);

            // Act
            rolePermission.RoleId = roleId;
            rolePermission.PermissionId = permissionId;
            rolePermission.GrantedBy = grantedBy;
            rolePermission.GrantedAt = grantedAt;
            rolePermission.IsActive = false;

            // Assert
            Assert.Equal(roleId, rolePermission.RoleId);
            Assert.Equal(permissionId, rolePermission.PermissionId);
            Assert.Equal(grantedBy, rolePermission.GrantedBy);
            Assert.Equal(grantedAt, rolePermission.GrantedAt);
            Assert.False(rolePermission.IsActive);
        }

        [Fact]
        public void RolePermission_StringLengthLimits_ShouldRespectMaxLengths()
        {
            // Arrange
            var rolePermission = new RolePermission();
            var longString450 = new string('A', 450);

            // Act
            rolePermission.RoleId = longString450;
            rolePermission.GrantedBy = longString450;

            // Assert - These should not throw exceptions as they're within limits
            Assert.Equal(longString450, rolePermission.RoleId);
            Assert.Equal(longString450, rolePermission.GrantedBy);
        }

        [Fact]
        public void RolePermission_GrantedAt_ShouldBeRecentTime()
        {
            // Arrange
            var beforeCreation = DateTime.UtcNow;

            // Act
            var rolePermission = new RolePermission();
            var afterCreation = DateTime.UtcNow;

            // Assert
            Assert.True(rolePermission.GrantedAt >= beforeCreation);
            Assert.True(rolePermission.GrantedAt <= afterCreation);
        }

        [Fact]
        public void RolePermission_IsActive_CanBeToggled()
        {
            // Arrange
            var rolePermission = new RolePermission();
            
            // Confirm default
            Assert.True(rolePermission.IsActive);

            // Act & Assert - toggle to false
            rolePermission.IsActive = false;
            Assert.False(rolePermission.IsActive);

            // Act & Assert - toggle back to true
            rolePermission.IsActive = true;
            Assert.True(rolePermission.IsActive);
        }

        [Theory]
        [InlineData("role-1", 1, "admin@test.com")]
        [InlineData("role-2", 999, "manager@test.com")]
        [InlineData("admin-role", 42, "system@test.com")]
        public void RolePermission_SetMultipleProperties_ShouldWorkCorrectly(string roleId, int permissionId, string grantedBy)
        {
            // Arrange
            var rolePermission = new RolePermission();

            // Act
            rolePermission.RoleId = roleId;
            rolePermission.PermissionId = permissionId;
            rolePermission.GrantedBy = grantedBy;

            // Assert
            Assert.Equal(roleId, rolePermission.RoleId);
            Assert.Equal(permissionId, rolePermission.PermissionId);
            Assert.Equal(grantedBy, rolePermission.GrantedBy);
        }

        [Fact]
        public void RolePermission_NavigationProperties_ShouldInitializeAsNull()
        {
            // Arrange & Act
            var rolePermission = new RolePermission();

            // Assert
            Assert.Null(rolePermission.Role);
            Assert.Null(rolePermission.Permission);
        }

        [Fact]
        public void RolePermission_NavigationProperties_CanBeSet()
        {
            // Arrange
            var rolePermission = new RolePermission();
            var role = new ApplicationRole("TestRole");
            var permission = new Permission { Name = "TestPermission", Code = "TEST_PERMISSION" };

            // Act
            rolePermission.Role = role;
            rolePermission.Permission = permission;

            // Assert
            Assert.NotNull(rolePermission.Role);
            Assert.NotNull(rolePermission.Permission);
            Assert.Equal(role, rolePermission.Role);
            Assert.Equal(permission, rolePermission.Permission);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void RolePermission_GrantedBy_CanBeNullOrEmpty(string? grantedBy)
        {
            // Arrange
            var rolePermission = new RolePermission();

            // Act
            rolePermission.GrantedBy = grantedBy;

            // Assert
            Assert.Equal(grantedBy, rolePermission.GrantedBy);
        }

        [Fact]
        public void RolePermission_GrantedBy_CanBeNull()
        {
            // Arrange
            var rolePermission = new RolePermission();

            // Act
            rolePermission.GrantedBy = null;

            // Assert
            Assert.Null(rolePermission.GrantedBy);
        }

        [Fact]
        public void RolePermission_PermissionId_CanBeSet()
        {
            // Arrange
            var rolePermission = new RolePermission();
            var permissionId = 12345;

            // Act
            rolePermission.PermissionId = permissionId;

            // Assert
            Assert.Equal(permissionId, rolePermission.PermissionId);
        }

        [Fact]
        public void RolePermission_Id_CanBeSet()
        {
            // Arrange
            var rolePermission = new RolePermission();
            var id = 999;

            // Act
            rolePermission.Id = id;

            // Assert
            Assert.Equal(id, rolePermission.Id);
        }
    }
}
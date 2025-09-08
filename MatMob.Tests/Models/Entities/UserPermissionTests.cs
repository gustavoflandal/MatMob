using MatMob.Models.Entities;
using Xunit;

namespace MatMob.Tests.Models.Entities
{
    public class UserPermissionTests
    {
        [Fact]
        public void UserPermission_Constructor_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var userPermission = new UserPermission();

            // Assert
            Assert.Equal(0, userPermission.Id);
            Assert.Equal(string.Empty, userPermission.UserId);
            Assert.Equal(0, userPermission.PermissionId);
            Assert.NotEqual(default(DateTime), userPermission.GrantedAt);
            Assert.True(userPermission.GrantedAt <= DateTime.UtcNow);
            Assert.Null(userPermission.GrantedBy);
            Assert.True(userPermission.IsActive);
            Assert.Null(userPermission.Reason);
            Assert.Null(userPermission.ExpiresAt);
            // Navigation properties will be null until set
        }

        [Fact]
        public void UserPermission_SetProperties_ShouldWorkCorrectly()
        {
            // Arrange
            var userPermission = new UserPermission();
            var userId = "user-123";
            var permissionId = 456;
            var grantedBy = "admin@test.com";
            var grantedAt = DateTime.UtcNow.AddHours(-1);
            var reason = "Permissão temporária para projeto";
            var expiresAt = DateTime.UtcNow.AddDays(30);

            // Act
            userPermission.UserId = userId;
            userPermission.PermissionId = permissionId;
            userPermission.GrantedBy = grantedBy;
            userPermission.GrantedAt = grantedAt;
            userPermission.Reason = reason;
            userPermission.ExpiresAt = expiresAt;
            userPermission.IsActive = false;

            // Assert
            Assert.Equal(userId, userPermission.UserId);
            Assert.Equal(permissionId, userPermission.PermissionId);
            Assert.Equal(grantedBy, userPermission.GrantedBy);
            Assert.Equal(grantedAt, userPermission.GrantedAt);
            Assert.Equal(reason, userPermission.Reason);
            Assert.Equal(expiresAt, userPermission.ExpiresAt);
            Assert.False(userPermission.IsActive);
        }

        [Fact]
        public void UserPermission_StringLengthLimits_ShouldRespectMaxLengths()
        {
            // Arrange
            var userPermission = new UserPermission();
            var longString450 = new string('A', 450);
            var longString500 = new string('B', 500);

            // Act
            userPermission.UserId = longString450;
            userPermission.GrantedBy = longString450;
            userPermission.Reason = longString500;

            // Assert - These should not throw exceptions as they're within limits
            Assert.Equal(longString450, userPermission.UserId);
            Assert.Equal(longString450, userPermission.GrantedBy);
            Assert.Equal(longString500, userPermission.Reason);
        }

        [Fact]
        public void UserPermission_GrantedAt_ShouldBeRecentTime()
        {
            // Arrange
            var beforeCreation = DateTime.UtcNow;

            // Act
            var userPermission = new UserPermission();
            var afterCreation = DateTime.UtcNow;

            // Assert
            Assert.True(userPermission.GrantedAt >= beforeCreation);
            Assert.True(userPermission.GrantedAt <= afterCreation);
        }

        [Fact]
        public void UserPermission_IsActive_CanBeToggled()
        {
            // Arrange
            var userPermission = new UserPermission();
            
            // Confirm default
            Assert.True(userPermission.IsActive);

            // Act & Assert - toggle to false
            userPermission.IsActive = false;
            Assert.False(userPermission.IsActive);

            // Act & Assert - toggle back to true
            userPermission.IsActive = true;
            Assert.True(userPermission.IsActive);
        }

        [Theory]
        [InlineData("user-1", 1, "admin@test.com", "Acesso especial")]
        [InlineData("user-2", 999, "manager@test.com", "Permissão temporária")]
        [InlineData("test-user", 42, "system@test.com", "Projeto específico")]
        public void UserPermission_SetMultipleProperties_ShouldWorkCorrectly(string userId, int permissionId, string grantedBy, string reason)
        {
            // Arrange
            var userPermission = new UserPermission();

            // Act
            userPermission.UserId = userId;
            userPermission.PermissionId = permissionId;
            userPermission.GrantedBy = grantedBy;
            userPermission.Reason = reason;

            // Assert
            Assert.Equal(userId, userPermission.UserId);
            Assert.Equal(permissionId, userPermission.PermissionId);
            Assert.Equal(grantedBy, userPermission.GrantedBy);
            Assert.Equal(reason, userPermission.Reason);
        }

        [Fact]
        public void UserPermission_NavigationProperties_ShouldInitializeAsNull()
        {
            // Arrange & Act
            var userPermission = new UserPermission();

            // Assert
            Assert.Null(userPermission.User);
            Assert.Null(userPermission.Permission);
        }

        [Fact]
        public void UserPermission_NavigationProperties_CanBeSet()
        {
            // Arrange
            var userPermission = new UserPermission();
            var user = new ApplicationUser("testuser@example.com");
            var permission = new Permission { Name = "TestPermission", Code = "TEST_PERMISSION" };

            // Act
            userPermission.User = user;
            userPermission.Permission = permission;

            // Assert
            Assert.NotNull(userPermission.User);
            Assert.NotNull(userPermission.Permission);
            Assert.Equal(user, userPermission.User);
            Assert.Equal(permission, userPermission.Permission);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void UserPermission_OptionalFields_CanBeNullOrEmpty(string? value)
        {
            // Arrange
            var userPermission = new UserPermission();

            // Act
            userPermission.GrantedBy = value;
            userPermission.Reason = value;

            // Assert
            Assert.Equal(value, userPermission.GrantedBy);
            Assert.Equal(value, userPermission.Reason);
        }

        [Fact]
        public void UserPermission_OptionalFields_CanBeNull()
        {
            // Arrange
            var userPermission = new UserPermission();

            // Act
            userPermission.GrantedBy = null;
            userPermission.Reason = null;

            // Assert
            Assert.Null(userPermission.GrantedBy);
            Assert.Null(userPermission.Reason);
        }

        [Fact]
        public void UserPermission_ExpiresAt_CanBeSet()
        {
            // Arrange
            var userPermission = new UserPermission();
            var expireTime = DateTime.UtcNow.AddDays(30);

            // Act
            userPermission.ExpiresAt = expireTime;

            // Assert
            Assert.Equal(expireTime, userPermission.ExpiresAt);
        }

        [Fact]
        public void UserPermission_ExpiresAt_CanBeNull()
        {
            // Arrange
            var userPermission = new UserPermission();

            // Act (explicitly set to null, though it's already null by default)
            userPermission.ExpiresAt = null;

            // Assert
            Assert.Null(userPermission.ExpiresAt);
        }

        [Fact]
        public void UserPermission_Id_CanBeSet()
        {
            // Arrange
            var userPermission = new UserPermission();
            var id = 12345;

            // Act
            userPermission.Id = id;

            // Assert
            Assert.Equal(id, userPermission.Id);
        }

        [Fact]
        public void UserPermission_Scenario_TemporaryPermission()
        {
            // Arrange - Simulating a temporary permission scenario
            var userPermission = new UserPermission();
            var userId = "temp-user-001";
            var permissionId = 100;
            var grantedBy = "project-manager@test.com";
            var reason = "Acesso temporário para projeto especial";
            var expiresAt = DateTime.UtcNow.AddDays(15);

            // Act
            userPermission.UserId = userId;
            userPermission.PermissionId = permissionId;
            userPermission.GrantedBy = grantedBy;
            userPermission.Reason = reason;
            userPermission.ExpiresAt = expiresAt;

            // Assert
            Assert.Equal(userId, userPermission.UserId);
            Assert.Equal(permissionId, userPermission.PermissionId);
            Assert.Equal(grantedBy, userPermission.GrantedBy);
            Assert.Equal(reason, userPermission.Reason);
            Assert.Equal(expiresAt, userPermission.ExpiresAt);
            Assert.True(userPermission.IsActive); // Should be active by default
            Assert.True(userPermission.ExpiresAt > DateTime.UtcNow); // Should not be expired yet
        }

        [Fact]
        public void UserPermission_Scenario_PermanentPermission()
        {
            // Arrange - Simulating a permanent permission scenario
            var userPermission = new UserPermission();
            var userId = "perm-user-001";
            var permissionId = 200;
            var grantedBy = "admin@test.com";
            var reason = "Permissão permanente do cargo";

            // Act
            userPermission.UserId = userId;
            userPermission.PermissionId = permissionId;
            userPermission.GrantedBy = grantedBy;
            userPermission.Reason = reason;
            // ExpiresAt is left null for permanent permissions

            // Assert
            Assert.Equal(userId, userPermission.UserId);
            Assert.Equal(permissionId, userPermission.PermissionId);
            Assert.Equal(grantedBy, userPermission.GrantedBy);
            Assert.Equal(reason, userPermission.Reason);
            Assert.Null(userPermission.ExpiresAt); // Permanent permissions don't expire
            Assert.True(userPermission.IsActive);
        }
    }
}
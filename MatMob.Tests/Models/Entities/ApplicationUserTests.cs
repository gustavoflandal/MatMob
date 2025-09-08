using Microsoft.AspNetCore.Identity;
using MatMob.Models.Entities;
using Xunit;

namespace MatMob.Tests.Models.Entities
{
    public class ApplicationUserTests
    {
        [Fact]
        public void ApplicationUser_Constructor_ShouldInheritFromIdentityUser()
        {
            // Arrange & Act
            var user = new ApplicationUser();

            // Assert
            Assert.IsAssignableFrom<IdentityUser>(user);
        }

        [Fact]
        public void ApplicationUser_Constructor_WithUserName_ShouldSetUserName()
        {
            // Arrange
            var userName = "testuser@example.com";

            // Act
            var user = new ApplicationUser(userName);

            // Assert
            Assert.Equal(userName, user.UserName);
        }

        [Fact]
        public void ApplicationUser_FullName_ShouldCombineFirstAndLastName()
        {
            // Arrange
            var user = new ApplicationUser
            {
                FirstName = "João",
                LastName = "Silva"
            };

            // Act
            var fullName = user.FullName;

            // Assert
            Assert.Equal("João Silva", fullName);
        }

        [Fact]
        public void ApplicationUser_FullName_WithOnlyFirstName_ShouldReturnFirstName()
        {
            // Arrange
            var user = new ApplicationUser
            {
                FirstName = "João",
                LastName = null
            };

            // Act
            var fullName = user.FullName;

            // Assert
            Assert.Equal("João", fullName);
        }

        [Fact]
        public void ApplicationUser_FullName_WithOnlyLastName_ShouldReturnLastName()
        {
            // Arrange
            var user = new ApplicationUser
            {
                FirstName = null,
                LastName = "Silva"
            };

            // Act
            var fullName = user.FullName;

            // Assert
            Assert.Equal("Silva", fullName);
        }

        [Fact]
        public void ApplicationUser_FullName_WithEmptyNames_ShouldReturnEmptyString()
        {
            // Arrange
            var user = new ApplicationUser
            {
                FirstName = "",
                LastName = ""
            };

            // Act
            var fullName = user.FullName;

            // Assert
            Assert.Equal("", fullName);
        }

        [Fact]
        public void ApplicationUser_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var user = new ApplicationUser();

            // Assert
            Assert.True(user.IsActive);
            Assert.Equal(0, user.LoginAttempts);
            Assert.NotEqual(default(DateTime), user.CreatedAt);
            Assert.True(user.CreatedAt <= DateTime.UtcNow);
            Assert.Empty(user.UserPermissions);
        }

        [Fact]
        public void ApplicationUser_SetProperties_ShouldWorkCorrectly()
        {
            // Arrange
            var user = new ApplicationUser();
            var profilePicture = "profile.jpg";
            var createdBy = "admin@test.com";
            var updatedBy = "manager@test.com";
            var lastLoginIp = "192.168.1.100";

            // Act
            user.ProfilePicture = profilePicture;
            user.CreatedBy = createdBy;
            user.UpdatedBy = updatedBy;
            user.LastLoginIp = lastLoginIp;
            user.IsActive = false;
            user.LoginAttempts = 3;

            // Assert
            Assert.Equal(profilePicture, user.ProfilePicture);
            Assert.Equal(createdBy, user.CreatedBy);
            Assert.Equal(updatedBy, user.UpdatedBy);
            Assert.Equal(lastLoginIp, user.LastLoginIp);
            Assert.False(user.IsActive);
            Assert.Equal(3, user.LoginAttempts);
        }

        [Fact]
        public void ApplicationUser_UpdatedAt_ShouldBeNullByDefault()
        {
            // Arrange & Act
            var user = new ApplicationUser();

            // Assert
            Assert.Null(user.UpdatedAt);
        }

        [Fact]
        public void ApplicationUser_LastLoginAt_ShouldBeNullByDefault()
        {
            // Arrange & Act
            var user = new ApplicationUser();

            // Assert
            Assert.Null(user.LastLoginAt);
        }

        [Fact]
        public void ApplicationUser_LastFailedLoginAt_ShouldBeNullByDefault()
        {
            // Arrange & Act
            var user = new ApplicationUser();

            // Assert
            Assert.Null(user.LastFailedLoginAt);
        }

        [Fact]
        public void ApplicationUser_UserPermissions_ShouldInitializeAsEmptyCollection()
        {
            // Arrange & Act
            var user = new ApplicationUser();

            // Assert
            Assert.NotNull(user.UserPermissions);
            Assert.Empty(user.UserPermissions);
            Assert.IsType<List<UserPermission>>(user.UserPermissions);
        }

        [Theory]
        [InlineData("João", "Silva", "João Silva")]
        [InlineData("Maria", "Santos Costa", "Maria Santos Costa")]
        [InlineData("Pedro", "", "Pedro")]
        [InlineData("", "Oliveira", "Oliveira")]
        [InlineData("", "", "")]
        [InlineData("  Ana  ", "  Souza  ", "Ana     Souza")]
        public void ApplicationUser_FullName_ShouldHandleVariousNameCombinations(string firstName, string lastName, string expected)
        {
            // Arrange
            var user = new ApplicationUser
            {
                FirstName = firstName,
                LastName = lastName
            };

            // Act
            var fullName = user.FullName;

            // Assert
            Assert.Equal(expected, fullName);
        }

        [Fact]
        public void ApplicationUser_StringLengthLimits_ShouldRespectMaxLengths()
        {
            // Arrange
            var user = new ApplicationUser();
            var longString100 = new string('A', 100);
            var longString200 = new string('B', 200);
            var longString450 = new string('C', 450);
            var longString500 = new string('D', 500);
            var ipAddress = "192.168.1.1";

            // Act
            user.FirstName = longString100;
            user.LastName = longString100;
            user.ProfilePicture = longString500;
            user.CreatedBy = longString450;
            user.UpdatedBy = longString450;
            user.LastLoginIp = ipAddress;

            // Assert - These should not throw exceptions as they're within limits
            Assert.Equal(longString100, user.FirstName);
            Assert.Equal(longString100, user.LastName);
            Assert.Equal(longString500, user.ProfilePicture);
            Assert.Equal(longString450, user.CreatedBy);
            Assert.Equal(longString450, user.UpdatedBy);
            Assert.Equal(ipAddress, user.LastLoginIp);
        }
    }
}
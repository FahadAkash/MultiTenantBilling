using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MultiTenantBilling.Application.Services;
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Repositories;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MultiTenantBilling.Tests.Services
{
    public class AuthorizationServiceTests
    {
        private readonly Mock<ITenantRepository<User>> _userRepositoryMock;
        private readonly Mock<ITenantRepository<Role>> _roleRepositoryMock;
        private readonly Mock<ITenantRepository<UserRole>> _userRoleRepositoryMock;
        private readonly Mock<ITenantService> _tenantServiceMock;
        private readonly Mock<ILogger<AuthorizationService>> _loggerMock;
        private readonly AuthorizationService _authorizationService;

        public AuthorizationServiceTests()
        {
            _userRepositoryMock = new Mock<ITenantRepository<User>>();
            _roleRepositoryMock = new Mock<ITenantRepository<Role>>();
            _userRoleRepositoryMock = new Mock<ITenantRepository<UserRole>>();
            _tenantServiceMock = new Mock<ITenantService>();
            _loggerMock = new Mock<ILogger<AuthorizationService>>();

            _authorizationService = new AuthorizationService(
                _userRepositoryMock.Object,
                _roleRepositoryMock.Object,
                _userRoleRepositoryMock.Object,
                _tenantServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task AssignRoleAsync_WithValidUserAndRole_ShouldReturnTrue()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var userEmail = "test@example.com";
            var roleName = "Admin";

            var user = new User
            {
                Id = userId,
                TenantId = tenantId,
                Email = userEmail,
                FirstName = "Test",
                LastName = "User",
                PasswordHash = "HASHED_password",
                IsActive = true
            };

            var role = new Role
            {
                Id = roleId,
                TenantId = tenantId,
                Name = roleName,
                Description = "Administrator role"
            };

            _tenantServiceMock.Setup(x => x.GetRequiredTenantId()).Returns(tenantId);
            _userRepositoryMock.Setup(x => x.GetByTenantIdAsync(tenantId))
                .ReturnsAsync(new User[] { user });
            _roleRepositoryMock.Setup(x => x.GetByTenantIdAsync(tenantId))
                .ReturnsAsync(new Role[] { role });
            _userRoleRepositoryMock.Setup(x => x.GetByTenantIdAsync(tenantId))
                .ReturnsAsync(new UserRole[] { });
            _userRoleRepositoryMock.Setup(x => x.AddAsync(It.IsAny<UserRole>()))
                .ReturnsAsync(new UserRole());

            // Act
            var result = await _authorizationService.AssignRoleAsync(userEmail, roleName);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task AssignRoleAsync_WithNonExistentUser_ShouldReturnFalse()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var userEmail = "nonexistent@example.com";
            var roleName = "Admin";

            _tenantServiceMock.Setup(x => x.GetRequiredTenantId()).Returns(tenantId);
            _userRepositoryMock.Setup(x => x.GetByTenantIdAsync(tenantId))
                .ReturnsAsync(new User[] { }); // No users found

            // Act
            var result = await _authorizationService.AssignRoleAsync(userEmail, roleName);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task AssignRoleAsync_WithNonExistentRole_ShouldCreateRoleAndReturnTrue()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var userEmail = "test@example.com";
            var roleName = "NewRole";

            var user = new User
            {
                Id = userId,
                TenantId = tenantId,
                Email = userEmail,
                FirstName = "Test",
                LastName = "User",
                PasswordHash = "HASHED_password",
                IsActive = true
            };

            var role = new Role
            {
                Id = roleId,
                TenantId = tenantId,
                Name = roleName,
                Description = $"{roleName} role"
            };

            _tenantServiceMock.Setup(x => x.GetRequiredTenantId()).Returns(tenantId);
            _userRepositoryMock.Setup(x => x.GetByTenantIdAsync(tenantId))
                .ReturnsAsync(new User[] { user });
            _roleRepositoryMock.Setup(x => x.GetByTenantIdAsync(tenantId))
                .ReturnsAsync(new Role[] { }); // No roles found initially
            _roleRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Role>()))
                .ReturnsAsync(role);
            _userRoleRepositoryMock.Setup(x => x.GetByTenantIdAsync(tenantId))
                .ReturnsAsync(new UserRole[] { });
            _userRoleRepositoryMock.Setup(x => x.AddAsync(It.IsAny<UserRole>()))
                .ReturnsAsync(new UserRole());

            // Act
            var result = await _authorizationService.AssignRoleAsync(userEmail, roleName);

            // Assert
            result.Should().BeTrue();
            _roleRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Role>()), Times.Once);
        }
    }
}
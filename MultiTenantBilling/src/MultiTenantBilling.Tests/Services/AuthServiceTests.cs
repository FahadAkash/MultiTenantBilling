using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MultiTenantBilling.Application.DTOs;
using MultiTenantBilling.Application.Services;
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Repositories;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MultiTenantBilling.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<ITenantRepository<User>> _userRepositoryMock;
        private readonly Mock<ITenantRepository<Role>> _roleRepositoryMock;
        private readonly Mock<ITenantRepository<UserRole>> _userRoleRepositoryMock;
        private readonly Mock<ITenantService> _tenantServiceMock;
        private readonly Mock<IOptions<JwtSettings>> _jwtSettingsMock;
        private readonly JwtService _jwtService;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<ITenantRepository<User>>();
            _roleRepositoryMock = new Mock<ITenantRepository<Role>>();
            _userRoleRepositoryMock = new Mock<ITenantRepository<UserRole>>();
            _tenantServiceMock = new Mock<ITenantService>();
            _loggerMock = new Mock<ILogger<AuthService>>();
            
            // Setup JwtSettings
            _jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
            _jwtSettingsMock.Setup(x => x.Value).Returns(new JwtSettings
            {
                Secret = "THIS_IS_A_VERY_SECURE_KEY_THAT_SHOULD_BE_CHANGED_IN_PRODUCTION_1234567890",
                Issuer = "MultiTenantBilling",
                Audience = "MultiTenantBillingUsers",
                ExpiryInHours = 1
            });
            
            _jwtService = new JwtService(_jwtSettingsMock.Object);

            _authService = new AuthService(
                _userRepositoryMock.Object,
                _roleRepositoryMock.Object,
                _userRoleRepositoryMock.Object,
                _tenantServiceMock.Object,
                _jwtService,
                _loggerMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_WithValidData_ShouldCreateUserAndReturnAuthResponse()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var registerDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "password123",
                FirstName = "John",
                LastName = "Doe"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                IsActive = true
            };

            _tenantServiceMock.Setup(x => x.GetRequiredTenantId()).Returns(tenantId);
            _userRepositoryMock.Setup(x => x.GetByTenantIdAsync(tenantId))
                .ReturnsAsync(new User[] { });
            _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>()))
                .ReturnsAsync(user);
            _roleRepositoryMock.Setup(x => x.GetByTenantIdAsync(tenantId))
                .ReturnsAsync(new Role[] { });
            _userRoleRepositoryMock.Setup(x => x.AddAsync(It.IsAny<UserRole>()))
                .ReturnsAsync(new UserRole());

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            result.Should().NotBeNull();
            result.User.Should().NotBeNull();
            result.User.Email.Should().Be(registerDto.Email);
            result.User.FirstName.Should().Be(registerDto.FirstName);
            result.User.LastName.Should().Be(registerDto.LastName);
            result.Token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnAuthResponse()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Email = loginDto.Email,
                FirstName = "John",
                LastName = "Doe",
                PasswordHash = "HASHED_password123", // Using the simple hash from AuthService
                IsActive = true
            };

            _tenantServiceMock.Setup(x => x.GetRequiredTenantId()).Returns(tenantId);
            _userRepositoryMock.Setup(x => x.GetByTenantIdAsync(tenantId))
                .ReturnsAsync(new User[] { user });

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.User.Should().NotBeNull();
            result.User.Email.Should().Be(loginDto.Email);
            result.Token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "wrongpassword"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Email = loginDto.Email,
                FirstName = "John",
                LastName = "Doe",
                PasswordHash = "HASHED_password123", // Using the simple hash from AuthService
                IsActive = true
            };

            _tenantServiceMock.Setup(x => x.GetRequiredTenantId()).Returns(tenantId);
            _userRepositoryMock.Setup(x => x.GetByTenantIdAsync(tenantId))
                .ReturnsAsync(new User[] { user });

            // Act
            Func<Task<AuthResponseDto>> act = async () => await _authService.LoginAsync(loginDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Invalid email or password");
        }

        [Fact]
        public async Task ChangePasswordAsync_WithValidCredentials_ShouldReturnTrue()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var email = "test@example.com";
            var currentPassword = "oldpassword";
            var newPassword = "newpassword";

            var user = new User
            {
                Id = userId,
                TenantId = tenantId,
                Email = email,
                FirstName = "John",
                LastName = "Doe",
                PasswordHash = "HASHED_oldpassword", // Using the simple hash from AuthService
                IsActive = true
            };

            _tenantServiceMock.Setup(x => x.GetRequiredTenantId()).Returns(tenantId);
            _userRepositoryMock.Setup(x => x.GetByTenantIdAsync(tenantId))
                .ReturnsAsync(new User[] { user });
            _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.ChangePasswordAsync(email, currentPassword, newPassword);

            // Assert
            result.Should().BeTrue();
            _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_WithInvalidCurrentPassword_ShouldReturnFalse()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var email = "test@example.com";
            var currentPassword = "wrongpassword";
            var newPassword = "newpassword";

            var user = new User
            {
                Id = userId,
                TenantId = tenantId,
                Email = email,
                FirstName = "John",
                LastName = "Doe",
                PasswordHash = "HASHED_correctpassword", // Using the simple hash from AuthService
                IsActive = true
            };

            _tenantServiceMock.Setup(x => x.GetRequiredTenantId()).Returns(tenantId);
            _userRepositoryMock.Setup(x => x.GetByTenantIdAsync(tenantId))
                .ReturnsAsync(new User[] { user });

            // Act
            var result = await _authService.ChangePasswordAsync(email, currentPassword, newPassword);

            // Assert
            result.Should().BeFalse();
            _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_WithNonExistentUser_ShouldReturnFalse()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var email = "nonexistent@example.com";
            var currentPassword = "oldpassword";
            var newPassword = "newpassword";

            _tenantServiceMock.Setup(x => x.GetRequiredTenantId()).Returns(tenantId);
            _userRepositoryMock.Setup(x => x.GetByTenantIdAsync(tenantId))
                .ReturnsAsync(new User[] { }); // No users found

            // Act
            var result = await _authService.ChangePasswordAsync(email, currentPassword, newPassword);

            // Assert
            result.Should().BeFalse();
            _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }
    }
}
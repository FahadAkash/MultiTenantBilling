using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MultiTenantBilling.Api.Controllers;
using MultiTenantBilling.Application.DTOs;
using MultiTenantBilling.Application.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace MultiTenantBilling.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _controller = new AuthController(_authServiceMock.Object);
        }

        [Fact]
        public async Task Register_WithValidData_ShouldReturnOkResult()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "SecurePassword123!",
                FirstName = "John",
                LastName = "Doe"
            };

            var authResponseDto = new AuthResponseDto
            {
                User = new UserDto
                {
                    Id = Guid.NewGuid(),
                    Email = "test@example.com",
                    FirstName = "John",
                    LastName = "Doe",
                    IsActive = true
                },
                Token = "jwt_token_here"
            };

            _authServiceMock.Setup(x => x.RegisterAsync(It.IsAny<RegisterDto>()))
                .ReturnsAsync(authResponseDto);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            
            var returnedAuthResponse = okResult.Value as AuthResponseDto;
            returnedAuthResponse.Should().NotBeNull();
            returnedAuthResponse.User.Email.Should().Be("test@example.com");
            returnedAuthResponse.Token.Should().Be("jwt_token_here");
        }

        [Fact]
        public async Task Register_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "SecurePassword123!",
                FirstName = "John",
                LastName = "Doe"
            };

            _authServiceMock.Setup(x => x.RegisterAsync(It.IsAny<RegisterDto>()))
                .ThrowsAsync(new InvalidOperationException("User already exists"));

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnOkResult()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "SecurePassword123!"
            };

            var authResponseDto = new AuthResponseDto
            {
                User = new UserDto
                {
                    Id = Guid.NewGuid(),
                    Email = "test@example.com",
                    FirstName = "John",
                    LastName = "Doe",
                    IsActive = true
                },
                Token = "jwt_token_here"
            };

            _authServiceMock.Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
                .ReturnsAsync(authResponseDto);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            
            var returnedAuthResponse = okResult.Value as AuthResponseDto;
            returnedAuthResponse.Should().NotBeNull();
            returnedAuthResponse.User.Email.Should().Be("test@example.com");
            returnedAuthResponse.Token.Should().Be("jwt_token_here");
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            };

            _authServiceMock.Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
                .ThrowsAsync(new InvalidOperationException("Invalid email or password"));

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var unauthorizedResult = result.Result as UnauthorizedObjectResult;
            unauthorizedResult.Should().NotBeNull();
            unauthorizedResult.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task ChangePassword_WithValidData_ShouldReturnOkResult()
        {
            // Arrange
            var changePasswordDto = new ChangePasswordDto
            {
                CurrentPassword = "OldPassword123!",
                NewPassword = "NewPassword456!"
            };

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "test@example.com")
            }, "mock"));

            _authServiceMock.Setup(x => x.ChangePasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() { User = user }
            };

            // Act
            var result = await _controller.ChangePassword(changePasswordDto);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            
            var returnedResult = okResult.Value as bool?;
            returnedResult.Should().NotBeNull();
            returnedResult.Should().BeTrue();
        }

        [Fact]
        public async Task ChangePassword_WithUnauthenticatedUser_ShouldReturnUnauthorized()
        {
            // Arrange
            var changePasswordDto = new ChangePasswordDto
            {
                CurrentPassword = "OldPassword123!",
                NewPassword = "NewPassword456!"
            };

            var user = new ClaimsPrincipal(new ClaimsIdentity()); // No claims

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() { User = user }
            };

            // Act
            var result = await _controller.ChangePassword(changePasswordDto);

            // Assert
            var unauthorizedResult = result.Result as UnauthorizedResult;
            unauthorizedResult.Should().NotBeNull();
            unauthorizedResult.StatusCode.Should().Be(401);
        }
    }
}
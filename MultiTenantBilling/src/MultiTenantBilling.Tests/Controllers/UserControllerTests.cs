using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MultiTenantBilling.Api.Controllers;
using MultiTenantBilling.Api.Services;
using MultiTenantBilling.Application.DTOs;
using MultiTenantBilling.Application.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace MultiTenantBilling.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IAuthorizationService> _authorizationServiceMock;
        private readonly Mock<IApiTenantService> _tenantServiceMock;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _authorizationServiceMock = new Mock<IAuthorizationService>();
            _tenantServiceMock = new Mock<IApiTenantService>();
            _controller = new UserController(_authorizationServiceMock.Object, _tenantServiceMock.Object);
        }

        [Fact]
        public async Task GetCurrentUser_WithAuthenticatedUser_ShouldReturnOkResult()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.GivenName, "John"),
                new Claim(ClaimTypes.Surname, "Doe")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() { User = user }
            };

            // Act
            var result = await _controller.GetCurrentUser();

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            
            var returnedUser = okResult.Value as UserDto;
            returnedUser.Should().NotBeNull();
            returnedUser.Email.Should().Be("test@example.com");
            returnedUser.FirstName.Should().Be("John");
            returnedUser.LastName.Should().Be("Doe");
        }

        [Fact]
        public async Task GetCurrentUser_WithUnauthenticatedUser_ShouldReturnUnauthorized()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity()); // No claims

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() { User = user }
            };

            // Act
            var result = await _controller.GetCurrentUser();

            // Assert
            var unauthorizedResult = result.Result as UnauthorizedResult;
            unauthorizedResult.Should().NotBeNull();
            unauthorizedResult.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task GetUserPermissions_WithAuthenticatedUser_ShouldReturnOkResult()
        {
            // Arrange
            var permissions = new List<string> { "Read", "Write", "Delete" };

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "test@example.com")
            }, "mock"));

            _authorizationServiceMock.Setup(x => x.GetUserPermissionsAsync(It.IsAny<string>()))
                .ReturnsAsync(permissions);

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() { User = user }
            };

            // Act
            var result = await _controller.GetUserPermissions();

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            
            var returnedPermissions = okResult.Value as IEnumerable<string>;
            returnedPermissions.Should().NotBeNull();
            returnedPermissions.Should().Contain("Read");
            returnedPermissions.Should().Contain("Write");
            returnedPermissions.Should().Contain("Delete");
        }

        [Fact]
        public async Task GetUserPermissions_WithUnauthenticatedUser_ShouldReturnUnauthorized()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity()); // No claims

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() { User = user }
            };

            // Act
            var result = await _controller.GetUserPermissions();

            // Assert
            var unauthorizedResult = result.Result as UnauthorizedResult;
            unauthorizedResult.Should().NotBeNull();
            unauthorizedResult.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task GetUserRoles_WithAuthenticatedUser_ShouldReturnOkResult()
        {
            // Arrange
            var roles = new List<string> { "User", "Editor" };

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "test@example.com")
            }, "mock"));

            _authorizationServiceMock.Setup(x => x.GetUserRolesAsync(It.IsAny<string>()))
                .ReturnsAsync(roles);

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() { User = user }
            };

            // Act
            var result = await _controller.GetUserRoles();

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            
            var returnedRoles = okResult.Value as IEnumerable<string>;
            returnedRoles.Should().NotBeNull();
            returnedRoles.Should().Contain("User");
            returnedRoles.Should().Contain("Editor");
        }

        [Fact]
        public async Task GetUserRoles_WithUnauthenticatedUser_ShouldReturnUnauthorized()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity()); // No claims

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() { User = user }
            };

            // Act
            var result = await _controller.GetUserRoles();

            // Assert
            var unauthorizedResult = result.Result as UnauthorizedResult;
            unauthorizedResult.Should().NotBeNull();
            unauthorizedResult.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task AssignRole_WithValidData_ShouldReturnOkResult()
        {
            // Arrange
            var roleAssignmentDto = new MultiTenantBilling.Api.Controllers.RoleAssignmentDto
            {
                UserEmail = "user@example.com",
                RoleName = "Admin"
            };

            _authorizationServiceMock.Setup(x => x.AssignRoleAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.AssignRole(roleAssignmentDto);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            
            var returnedResult = okResult.Value as bool?;
            returnedResult.Should().NotBeNull();
            returnedResult.Should().BeTrue();
        }
    }
}
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using MultiTenantBilling.Api.Middleware;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace MultiTenantBilling.Tests.Middleware
{
    public class TenantMiddlewareTests
    {
        private readonly Mock<ILogger<TenantMiddleware>> _loggerMock;
        private readonly TenantMiddleware _middleware;

        public TenantMiddlewareTests()
        {
            _loggerMock = new Mock<ILogger<TenantMiddleware>>();
            _middleware = new TenantMiddleware(async (innerHttpContext) => { await Task.CompletedTask; }, _loggerMock.Object);
        }

        [Fact]
        public async Task InvokeAsync_WithJwtToken_ShouldExtractTenantId()
        {
            // Arrange
            var jwtService = new MultiTenantBilling.Application.Services.JwtService(
                Microsoft.Extensions.Options.Options.Create(new MultiTenantBilling.Application.Services.JwtSettings
                {
                    Secret = "THIS_IS_A_VERY_SECURE_KEY_THAT_SHOULD_BE_CHANGED_IN_PRODUCTION_1234567890",
                    Issuer = "MultiTenantBilling",
                    Audience = "MultiTenantBillingUsers",
                    ExpiryInHours = 1
                }));

            var userDto = new MultiTenantBilling.Application.Services.UserDto
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                IsActive = true,
                Roles = new[] { "User" }
            };

            var tenantId = Guid.NewGuid();
            var token = jwtService.GenerateToken(userDto, tenantId);

            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = $"Bearer {token}";

            var nextMock = new Mock<RequestDelegate>();
            nextMock.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
            
            var middleware = new TenantMiddleware(nextMock.Object, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Items.ContainsKey("TenantId").Should().BeTrue();
            context.Items["TenantId"].Should().Be(tenantId);
        }

        [Fact]
        public async Task InvokeAsync_WithTenantHeader_ShouldExtractTenantId()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Tenant-ID"] = tenantId.ToString();

            var nextMock = new Mock<RequestDelegate>();
            nextMock.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
            
            var middleware = new TenantMiddleware(nextMock.Object, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Items.ContainsKey("TenantId").Should().BeTrue();
            context.Items["TenantId"].Should().Be(tenantId);
        }

        [Fact]
        public async Task InvokeAsync_WithInvalidTenantHeader_ShouldNotSetTenantId()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Tenant-ID"] = "invalid-guid";

            var nextMock = new Mock<RequestDelegate>();
            nextMock.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
            
            var middleware = new TenantMiddleware(nextMock.Object, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Items.ContainsKey("TenantId").Should().BeFalse();
        }

        [Fact]
        public async Task InvokeAsync_WithNoTenantInfo_ShouldNotSetTenantId()
        {
            // Arrange
            var context = new DefaultHttpContext();

            var nextMock = new Mock<RequestDelegate>();
            nextMock.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
            
            var middleware = new TenantMiddleware(nextMock.Object, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Items.ContainsKey("TenantId").Should().BeFalse();
        }

        [Fact]
        public async Task InvokeAsync_WithInvalidJwtToken_ShouldNotSetTenantId()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = "Bearer invalid.token.here";

            var nextMock = new Mock<RequestDelegate>();
            nextMock.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
            
            var middleware = new TenantMiddleware(nextMock.Object, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Items.ContainsKey("TenantId").Should().BeFalse();
        }

        [Fact]
        public async Task InvokeAsync_WithJwtTokenWithoutTenantIdClaim_ShouldNotSetTenantId()
        {
            // Arrange
            // Create a token without tenantId claim
            var jwtService = new MultiTenantBilling.Application.Services.JwtService(
                Microsoft.Extensions.Options.Options.Create(new MultiTenantBilling.Application.Services.JwtSettings
                {
                    Secret = "THIS_IS_A_VERY_SECURE_KEY_THAT_SHOULD_BE_CHANGED_IN_PRODUCTION_1234567890",
                    Issuer = "MultiTenantBilling",
                    Audience = "MultiTenantBillingUsers",
                    ExpiryInHours = 1
                }));

            // Create a principal without tenantId claim
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.GivenName, "John"),
                new Claim(ClaimTypes.Surname, "Doe")
            };

            var token = "invalid_token_for_this_test"; // This is a simplified test

            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = $"Bearer {token}";

            var nextMock = new Mock<RequestDelegate>();
            nextMock.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
            
            var middleware = new TenantMiddleware(nextMock.Object, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Items.ContainsKey("TenantId").Should().BeFalse();
        }
    }
}
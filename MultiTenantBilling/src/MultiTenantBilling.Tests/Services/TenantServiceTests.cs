using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using MultiTenantBilling.Api.Services;
using System;
using Xunit;

namespace MultiTenantBilling.Tests.Services
{
    public class TenantServiceTests
    {
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly TenantService _tenantService;

        public TenantServiceTests()
        {
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _tenantService = new TenantService(_httpContextAccessorMock.Object);
        }

        [Fact]
        public void GetTenantId_WhenTenantIdIsSet_ShouldReturnTenantId()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var httpContext = new DefaultHttpContext();
            httpContext.Items["TenantId"] = tenantId;
            
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            // Act
            var result = _tenantService.GetTenantId();

            // Assert
            result.Should().HaveValue();
            result.Should().Be(tenantId);
        }

        [Fact]
        public void GetTenantId_WhenTenantIdIsNotSet_ShouldReturnNull()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            // Act
            var result = _tenantService.GetTenantId();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetTenantId_WhenHttpContextIsNull_ShouldReturnNull()
        {
            // Arrange
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

            // Act
            var result = _tenantService.GetTenantId();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void IsTenantAvailable_WhenTenantIdIsSet_ShouldReturnTrue()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var httpContext = new DefaultHttpContext();
            httpContext.Items["TenantId"] = tenantId;
            
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            // Act
            var result = _tenantService.IsTenantAvailable();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsTenantAvailable_WhenTenantIdIsNotSet_ShouldReturnFalse()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            // Act
            var result = _tenantService.IsTenantAvailable();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void GetRequiredTenantId_WhenTenantIdIsSet_ShouldReturnTenantId()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var httpContext = new DefaultHttpContext();
            httpContext.Items["TenantId"] = tenantId;
            
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            // Act
            var result = _tenantService.GetRequiredTenantId();

            // Assert
            result.Should().Be(tenantId);
        }

        [Fact]
        public void GetRequiredTenantId_WhenTenantIdIsNotSet_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            // Act
            Action act = () => _tenantService.GetRequiredTenantId();

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Tenant ID is required but not available. Make sure the TenantMiddleware is registered and functioning correctly.");
        }

        [Fact]
        public void SetTenantId_ShouldSetTenantIdInHttpContext()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var httpContext = new DefaultHttpContext();
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            // Act
            _tenantService.SetTenantId(tenantId);

            // Assert
            httpContext.Items.ContainsKey("TenantId").Should().BeTrue();
            httpContext.Items["TenantId"].Should().Be(tenantId);
        }

        [Fact]
        public void SetTenantId_WhenHttpContextIsNull_ShouldNotThrowException()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

            // Act
            Action act = () => _tenantService.SetTenantId(tenantId);

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void GetRequiredTenantId_WhenHttpContextIsNull_ShouldThrowInvalidOperationException()
        {
            // Arrange
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

            // Act
            Action act = () => _tenantService.GetRequiredTenantId();

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Tenant ID is required but not available. Make sure the TenantMiddleware is registered and functioning correctly.");
        }
    }
}
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MultiTenantBilling.Api.Controllers;
using MultiTenantBilling.Api.Services;
using MultiTenantBilling.Application.Commands;
using MultiTenantBilling.Application.DTOs;
using MultiTenantBilling.Application.Queries;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MultiTenantBilling.Tests.Controllers
{
    public class AdminControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IApiTenantService> _tenantServiceMock;
        private readonly AdminController _controller;

        public AdminControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _tenantServiceMock = new Mock<IApiTenantService>();
            _controller = new AdminController(_mediatorMock.Object, _tenantServiceMock.Object);
        }

        [Fact]
        public void GetAdminDashboard_WithAuthenticatedUser_ShouldReturnOkResult()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "admin@example.com")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            // Act
            var result = _controller.GetAdminDashboard();

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().Be("Admin Dashboard - Welcome admin@example.com!");
        }

        [Fact]
        public void GetBillingInfo_WithAuthenticatedUser_ShouldReturnOkResult()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "user@example.com")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            // Act
            var result = _controller.GetBillingInfo();

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().Be("Billing Information - Access granted to user@example.com");
        }

        [Fact]
        public void CreateSubscription_WithAuthenticatedUser_ShouldReturnOkResult()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "user@example.com")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            // Act
            var result = _controller.CreateSubscription();

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().Be("Subscription created - Action performed by user@example.com");
        }

        [Fact]
        public void ProcessPayment_WithAuthenticatedUser_ShouldReturnOkResult()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "user@example.com")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            // Act
            var result = _controller.ProcessPayment();

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().Be("Payment processed - Action performed by user@example.com");
        }

        [Fact]
        public async Task CreatePlan_WithValidRequest_ShouldReturnOkResult()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            
            var request = new CreatePlanRequest
            {
                Name = "Enterprise Plan",
                Description = "Enterprise plan with all features",
                MonthlyPrice = 299.99m,
                MaxUsers = 1000,
                MaxStorageGb = 10000,
                IsActive = true
            };

            var planDto = new PlanDto
            {
                Id = planId,
                Name = "Enterprise Plan",
                Description = "Enterprise plan with all features",
                MonthlyPrice = 299.99m,
                MaxUsers = 1000,
                MaxStorageGb = 10000
            };

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "admin@example.com")
            }, "mock"));

            _tenantServiceMock.Setup(x => x.GetRequiredTenantId()).Returns(tenantId);
            _mediatorMock.Setup(x => x.Send(It.IsAny<CreatePlanCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(planDto);

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            // Act
            var result = await _controller.CreatePlan(request);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            
            var returnedPlan = okResult.Value as PlanDto;
            returnedPlan.Should().NotBeNull();
            returnedPlan.Id.Should().Be(planId);
            returnedPlan.Name.Should().Be("Enterprise Plan");
            returnedPlan.MonthlyPrice.Should().Be(299.99m);
        }

        [Fact]
        public async Task GetPlan_WithValidPlanId_ShouldReturnOkResult()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            
            var planDto = new PlanDto
            {
                Id = planId,
                Name = "Sample Plan",
                Description = "A sample plan for demonstration",
                MonthlyPrice = 29.99m,
                MaxUsers = 10,
                MaxStorageGb = 100
            };

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "admin@example.com")
            }, "mock"));

            _tenantServiceMock.Setup(x => x.GetRequiredTenantId()).Returns(tenantId);

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            // Act
            var result = await _controller.GetPlan(planId);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            
            var returnedPlan = okResult.Value as PlanDto;
            returnedPlan.Should().NotBeNull();
            returnedPlan.Id.Should().Be(planId);
            returnedPlan.Name.Should().Be("Sample Plan");
            returnedPlan.MonthlyPrice.Should().Be(29.99m);
        }

        [Fact]
        public async Task GetAllPlans_AdminEndpoint_ShouldReturnOkResult()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            
            var plans = new List<PlanDto>
            {
                new PlanDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Basic Plan",
                    Description = "Basic plan with limited features",
                    MonthlyPrice = 29.99m,
                    MaxUsers = 5,
                    MaxStorageGb = 100
                },
                new PlanDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Premium Plan",
                    Description = "Premium plan with all features",
                    MonthlyPrice = 99.99m,
                    MaxUsers = 100,
                    MaxStorageGb = 1000
                }
            };

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "admin@example.com")
            }, "mock"));

            _tenantServiceMock.Setup(x => x.GetRequiredTenantId()).Returns(tenantId);
            _mediatorMock.Setup(x => x.Send(It.IsAny<GetAllPlansQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(plans);

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            // Act
            var result = await _controller.GetAllPlans();

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            
            var returnedPlans = okResult.Value as IEnumerable<PlanDto>;
            returnedPlans.Should().NotBeNull();
            returnedPlans.Should().HaveCount(2);
        }
    }
}
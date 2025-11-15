using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MultiTenantBilling.Api.Controllers;
using MultiTenantBilling.Api.Services;
using MultiTenantBilling.Application.Commands;
using MultiTenantBilling.Application.DTOs;
using MultiTenantBilling.Application.Queries;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MultiTenantBilling.Tests.Controllers
{
    public class BillingControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IApiTenantService> _tenantServiceMock;
        private readonly BillingController _controller;

        public BillingControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _tenantServiceMock = new Mock<IApiTenantService>();
            _controller = new BillingController(_mediatorMock.Object, _tenantServiceMock.Object);
        }

        [Fact]
        public async Task CreateSubscription_WithValidRequest_ShouldReturnOkResult()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var subscriptionId = Guid.NewGuid();
            
            var request = new CreateSubscriptionRequest
            {
                PlanId = planId,
                PaymentMethodId = "pm_123456"
            };

            var subscriptionDto = new SubscriptionDto
            {
                Id = subscriptionId,
                TenantId = tenantId,
                PlanId = planId,
                StartDate = DateTime.UtcNow,
                Status = "Active"
            };

            _tenantServiceMock.Setup(x => x.GetRequiredTenantId()).Returns(tenantId);
            _mediatorMock.Setup(x => x.Send(It.IsAny<CreateSubscriptionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionDto);

            // Act
            var result = await _controller.CreateSubscription(request);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            
            var returnedSubscription = okResult.Value as SubscriptionDto;
            returnedSubscription.Should().NotBeNull();
            returnedSubscription.Id.Should().Be(subscriptionId);
            returnedSubscription.PlanId.Should().Be(planId);
        }

        [Fact]
        public async Task RecordUsage_WithValidRequest_ShouldReturnOkResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var usageRecordId = Guid.NewGuid();
            
            var request = new UsageRecordRequest
            {
                MetricName = "api_calls",
                Quantity = 100
            };

            var usageRecordDto = new UsageRecordDto
            {
                Id = usageRecordId,
                SubscriptionId = subscriptionId,
                MetricName = "api_calls",
                Quantity = 100,
                RecordedAt = DateTime.UtcNow
            };

            _mediatorMock.Setup(x => x.Send(It.IsAny<RecordUsageCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(usageRecordDto);

            // Act
            var result = await _controller.RecordUsage(subscriptionId, request);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            
            var returnedUsageRecord = okResult.Value as UsageRecordDto;
            returnedUsageRecord.Should().NotBeNull();
            returnedUsageRecord.Id.Should().Be(usageRecordId);
            returnedUsageRecord.SubscriptionId.Should().Be(subscriptionId);
            returnedUsageRecord.MetricName.Should().Be("api_calls");
            returnedUsageRecord.Quantity.Should().Be(100);
        }

        [Fact]
        public async Task GenerateInvoice_WithValidSubscriptionId_ShouldReturnOkResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var invoiceId = Guid.NewGuid();
            
            var invoiceDto = new InvoiceDto
            {
                Id = invoiceId,
                SubscriptionId = subscriptionId,
                Amount = 29.99m,
                Status = "Pending",
                InvoiceDate = DateTime.UtcNow
            };

            _mediatorMock.Setup(x => x.Send(It.IsAny<GenerateInvoiceCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(invoiceDto);

            // Act
            var result = await _controller.GenerateInvoice(subscriptionId);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            
            var returnedInvoice = okResult.Value as InvoiceDto;
            returnedInvoice.Should().NotBeNull();
            returnedInvoice.Id.Should().Be(invoiceId);
            returnedInvoice.SubscriptionId.Should().Be(subscriptionId);
            returnedInvoice.Amount.Should().Be(29.99m);
        }

        [Fact]
        public async Task ProcessPayment_WithValidRequest_ShouldReturnOkResult()
        {
            // Arrange
            var invoiceId = Guid.NewGuid();
            var paymentId = Guid.NewGuid();
            
            var request = new PaymentRequest
            {
                PaymentMethodId = "pm_123456"
            };

            var paymentDto = new PaymentDto
            {
                Id = paymentId,
                InvoiceId = invoiceId,
                Amount = 29.99m,
                PaymentDate = DateTime.UtcNow,
                Status = "Completed",
                Method = "Stripe"
            };

            _mediatorMock.Setup(x => x.Send(It.IsAny<ProcessPaymentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentDto);

            // Act
            var result = await _controller.ProcessPayment(invoiceId, request);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            
            var returnedPayment = okResult.Value as PaymentDto;
            returnedPayment.Should().NotBeNull();
            returnedPayment.Id.Should().Be(paymentId);
            returnedPayment.InvoiceId.Should().Be(invoiceId);
            returnedPayment.Amount.Should().Be(29.99m);
            returnedPayment.Status.Should().Be("Completed");
        }

        [Fact]
        public async Task GetAllPlans_WithValidTenantId_ShouldReturnOkResult()
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

            _tenantServiceMock.Setup(x => x.GetRequiredTenantId()).Returns(tenantId);
            _mediatorMock.Setup(x => x.Send(It.IsAny<GetAllPlansQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(plans);

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
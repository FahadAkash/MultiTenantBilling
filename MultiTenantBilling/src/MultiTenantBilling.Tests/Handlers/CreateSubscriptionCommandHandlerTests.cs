using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using MultiTenantBilling.Application.Commands;
using MultiTenantBilling.Application.DTOs;
using MultiTenantBilling.Application.Handlers;
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Domain.Events;
using MultiTenantBilling.Infrastructure.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MultiTenantBilling.Tests.Handlers
{
    public class CreateSubscriptionCommandHandlerTests
    {
        private readonly Mock<ITenantRepository<Subscription>> _subscriptionRepositoryMock;
        private readonly Mock<ITenantRepository<Plan>> _planRepositoryMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<CreateSubscriptionCommandHandler>> _loggerMock;
        private readonly CreateSubscriptionCommandHandler _handler;

        public CreateSubscriptionCommandHandlerTests()
        {
            _subscriptionRepositoryMock = new Mock<ITenantRepository<Subscription>>();
            _planRepositoryMock = new Mock<ITenantRepository<Plan>>();
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<CreateSubscriptionCommandHandler>>();

            _handler = new CreateSubscriptionCommandHandler(
                _subscriptionRepositoryMock.Object,
                _planRepositoryMock.Object,
                _mediatorMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidCommand_ShouldCreateSubscriptionAndPublishEvent()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var subscriptionId = Guid.NewGuid();

            var command = new CreateSubscriptionCommand
            {
                TenantId = tenantId,
                PlanId = planId
            };

            var plan = new Plan
            {
                Id = planId,
                TenantId = tenantId,
                Name = "Basic Plan",
                Description = "Basic plan with limited features",
                MonthlyPrice = 29.99m,
                MaxUsers = 5,
                MaxStorageGb = 100,
                IsActive = true
            };

            var subscription = new Subscription
            {
                Id = subscriptionId,
                TenantId = tenantId,
                PlanId = planId,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = "Active"
            };

            _planRepositoryMock.Setup(x => x.GetByIdForTenantAsync(planId, tenantId))
                .ReturnsAsync(plan);
            _subscriptionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Subscription>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<SubscriptionDto>();
            result.Id.Should().Be(subscriptionId);
            result.PlanId.Should().Be(planId);
            result.Status.Should().Be("Active");

            // Verify that the mediator was called to publish the event
            _mediatorMock.Verify(x => x.Publish(It.IsAny<SubscriptionCreatedEvent>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task Handle_WithNonExistentPlan_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var planId = Guid.NewGuid();

            var command = new CreateSubscriptionCommand
            {
                TenantId = tenantId,
                PlanId = planId
            };

            _planRepositoryMock.Setup(x => x.GetByIdForTenantAsync(planId, tenantId))
                .ReturnsAsync((Plan?)null); // Plan not found

            // Act
            Func<Task<SubscriptionDto>> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Plan not found or does not belong to tenant");
        }
    }
}
using MediatR;
using Microsoft.Extensions.Logging;
using MultiTenantBilling.Application.Commands;
using MultiTenantBilling.Application.DTOs;
using MultiTenantBilling.Application.Services;
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Domain.Events;
using MultiTenantBilling.Infrastructure.Repositories;

namespace MultiTenantBilling.Application.Handlers
{
    /// <summary>
    /// Handler for creating a new subscription
    /// </summary>
    public class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, SubscriptionDto>
    {
        private readonly ITenantRepository<Subscription> _subscriptionRepository;
        private readonly ITenantRepository<Plan> _planRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<CreateSubscriptionCommandHandler> _logger;

        public CreateSubscriptionCommandHandler(
            ITenantRepository<Subscription> subscriptionRepository,
            ITenantRepository<Plan> planRepository,
            IMediator mediator,
            ILogger<CreateSubscriptionCommandHandler> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _planRepository = planRepository;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<SubscriptionDto> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating subscription for tenant {TenantId} with plan {PlanId}", 
                request.TenantId, request.PlanId);

            // Get plan details
            var plan = await _planRepository.GetByIdForTenantAsync(request.PlanId, request.TenantId);
            if (plan == null)
            {
                throw new InvalidOperationException("Plan not found or does not belong to tenant");
            }

            // Create subscription
            var subscription = new Subscription
            {
                TenantId = request.TenantId,
                PlanId = request.PlanId,
                StartDate = request.StartDate,
                EndDate = request.StartDate.AddMonths(1), // Default monthly
                Status = "Active"
            };

            var createdSubscription = await _subscriptionRepository.AddAsync(subscription);

            // Raise domain event
            await _mediator.Publish(new SubscriptionCreatedEvent
            {
                TenantId = request.TenantId,
                SubscriptionId = createdSubscription.Id,
                PlanId = request.PlanId,
                StartDate = createdSubscription.StartDate,
                EndDate = createdSubscription.EndDate
            }, cancellationToken);

            return new SubscriptionDto
            {
                Id = createdSubscription.Id,
                TenantId = createdSubscription.TenantId,
                PlanId = createdSubscription.PlanId,
                StartDate = createdSubscription.StartDate,
                EndDate = createdSubscription.EndDate,
                Status = createdSubscription.Status
            };
        }
    }
}
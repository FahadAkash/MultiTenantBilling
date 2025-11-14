using MediatR;
using Microsoft.Extensions.Logging;
using MultiTenantBilling.Application.DTOs;
using MultiTenantBilling.Application.Queries;
using MultiTenantBilling.Infrastructure.Repositories;

namespace MultiTenantBilling.Application.Handlers
{
    /// <summary>
    /// Handler for getting subscription details
    /// </summary>
    public class GetSubscriptionQueryHandler : IRequestHandler<GetSubscriptionQuery, SubscriptionDto?>
    {
        private readonly ITenantRepository<Domain.Entities.Subscription> _subscriptionRepository;
        private readonly ILogger<GetSubscriptionQueryHandler> _logger;

        public GetSubscriptionQueryHandler(
            ITenantRepository<Domain.Entities.Subscription> subscriptionRepository,
            ILogger<GetSubscriptionQueryHandler> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _logger = logger;
        }

        public async Task<SubscriptionDto?> Handle(GetSubscriptionQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting subscription {SubscriptionId}", request.SubscriptionId);

            var subscription = await _subscriptionRepository.GetByIdAsync(request.SubscriptionId);
            if (subscription == null)
            {
                return null;
            }

            return new SubscriptionDto
            {
                Id = subscription.Id,
                TenantId = subscription.TenantId,
                PlanId = subscription.PlanId,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                Status = subscription.Status
            };
        }
    }
}


using MediatR;
using Microsoft.Extensions.Logging;
using MultiTenantBilling.Application.DTOs;
using MultiTenantBilling.Application.Queries;
using MultiTenantBilling.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MultiTenantBilling.Application.Handlers
{
    /// <summary>
    /// Handler for getting all subscriptions for a tenant
    /// </summary>
    public class GetAllSubscriptionsQueryHandler : IRequestHandler<GetAllSubscriptionsQuery, IEnumerable<SubscriptionDto>>
    {
        private readonly ITenantRepository<Domain.Entities.Subscription> _subscriptionRepository;
        private readonly ILogger<GetAllSubscriptionsQueryHandler> _logger;

        public GetAllSubscriptionsQueryHandler(
            ITenantRepository<Domain.Entities.Subscription> subscriptionRepository,
            ILogger<GetAllSubscriptionsQueryHandler> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<SubscriptionDto>> Handle(GetAllSubscriptionsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting all subscriptions for tenant {TenantId}", request.TenantId);

            var subscriptions = await _subscriptionRepository.GetByTenantIdAsync(request.TenantId);
            
            return subscriptions.Select(subscription => new SubscriptionDto
            {
                Id = subscription.Id,
                TenantId = subscription.TenantId,
                PlanId = subscription.PlanId,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                Status = subscription.Status
            }).ToList();
        }
    }
}
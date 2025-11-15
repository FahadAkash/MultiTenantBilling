using MediatR;
using Microsoft.Extensions.Logging;
using MultiTenantBilling.Application.Queries;
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Repositories;

namespace MultiTenantBilling.Application.Handlers
{
    /// <summary>
    /// Handler for getting subscriptions that need billing (used by Hangfire job)
    /// </summary>
    public class GetUpcomingBillingQueryHandler : IRequestHandler<GetUpcomingBillingQuery, List<UpcomingBillingDto>>
    {
        private readonly ITenantRepository<Subscription> _subscriptionRepository;
        private readonly ITenantRepository<Tenant> _tenantRepository;
        private readonly ILogger<GetUpcomingBillingQueryHandler> _logger;

        public GetUpcomingBillingQueryHandler(
            ITenantRepository<Subscription> subscriptionRepository,
            ITenantRepository<Tenant> tenantRepository,
            ILogger<GetUpcomingBillingQueryHandler> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _tenantRepository = tenantRepository;
            _logger = logger;
        }

        public async Task<List<UpcomingBillingDto>> Handle(GetUpcomingBillingQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting upcoming billing for date {BillingDate}", request.BillingDate);

            // Get all active subscriptions
            var allSubscriptions = await _subscriptionRepository.GetAllAsync();
            var activeSubscriptions = allSubscriptions
                .Where(s => s.Status == "Active" && s.EndDate.Date <= request.BillingDate.Date)
                .ToList();
                
            // Also include expired subscriptions that need to be processed for final billing
            var expiredSubscriptions = allSubscriptions
                .Where(s => s.Status == "Expired" && s.EndDate.Date <= request.BillingDate.Date)
                .ToList();

            var result = new List<UpcomingBillingDto>();

            foreach (var subscription in activeSubscriptions.Concat(expiredSubscriptions))
            {
                var tenant = await _tenantRepository.GetByIdAsync(subscription.TenantId);
                result.Add(new UpcomingBillingDto
                {
                    TenantId = subscription.TenantId,
                    SubscriptionId = subscription.Id,
                    PlanId = subscription.PlanId,
                    NextBillingDate = subscription.EndDate,
                    TenantName = tenant?.Name ?? "Unknown"
                });
            }

            return result;
        }
    }
}
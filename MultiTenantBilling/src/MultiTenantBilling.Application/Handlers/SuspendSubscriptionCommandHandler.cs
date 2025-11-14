using MediatR;
using Microsoft.Extensions.Logging;
using MultiTenantBilling.Application.Commands;
using MultiTenantBilling.Infrastructure.Repositories;

namespace MultiTenantBilling.Application.Handlers
{
    /// <summary>
    /// Handler for suspending subscriptions
    /// </summary>
    public class SuspendSubscriptionCommandHandler : IRequestHandler<SuspendSubscriptionCommand, bool>
    {
        private readonly ITenantRepository<Domain.Entities.Subscription> _subscriptionRepository;
        private readonly ILogger<SuspendSubscriptionCommandHandler> _logger;

        public SuspendSubscriptionCommandHandler(
            ITenantRepository<Domain.Entities.Subscription> subscriptionRepository,
            ILogger<SuspendSubscriptionCommandHandler> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(SuspendSubscriptionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogWarning("Suspending subscription {SubscriptionId}, reason: {Reason}",
                request.SubscriptionId, request.Reason);

            var subscription = await _subscriptionRepository.GetByIdAsync(request.SubscriptionId);
            if (subscription == null)
            {
                throw new InvalidOperationException("Subscription not found");
            }

            subscription.Status = "Suspended";
            await _subscriptionRepository.UpdateAsync(subscription);

            // TODO: Block API access for this tenant
            // TODO: Send suspension notification email
            // TODO: Log audit event

            _logger.LogInformation("Subscription {SubscriptionId} suspended successfully", request.SubscriptionId);
            return true;
        }
    }
}


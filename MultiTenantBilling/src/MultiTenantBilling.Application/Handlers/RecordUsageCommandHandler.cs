using MediatR;
using Microsoft.Extensions.Logging;
using MultiTenantBilling.Application.Commands;
using MultiTenantBilling.Application.DTOs;
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Domain.Events;
using MultiTenantBilling.Infrastructure.Repositories;

namespace MultiTenantBilling.Application.Handlers
{
    /// <summary>
    /// Handler for recording usage
    /// </summary>
    public class RecordUsageCommandHandler : IRequestHandler<RecordUsageCommand, UsageRecordDto>
    {
        private readonly ITenantRepository<UsageRecord> _usageRepository;
        private readonly ITenantRepository<Subscription> _subscriptionRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<RecordUsageCommandHandler> _logger;

        public RecordUsageCommandHandler(
            ITenantRepository<UsageRecord> usageRepository,
            ITenantRepository<Subscription> subscriptionRepository,
            IMediator mediator,
            ILogger<RecordUsageCommandHandler> logger)
        {
            _usageRepository = usageRepository;
            _subscriptionRepository = subscriptionRepository;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<UsageRecordDto> Handle(RecordUsageCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Recording usage for subscription {SubscriptionId}: {MetricName} = {Quantity}",
                request.SubscriptionId, request.MetricName, request.Quantity);

            var subscription = await _subscriptionRepository.GetByIdAsync(request.SubscriptionId);
            if (subscription == null)
            {
                throw new InvalidOperationException("Subscription not found");
            }

            var usageRecord = new UsageRecord
            {
                TenantId = subscription.TenantId,
                SubscriptionId = request.SubscriptionId,
                MetricName = request.MetricName,
                Quantity = request.Quantity,
                RecordedAt = request.RecordedAt ?? DateTime.UtcNow
            };

            var createdRecord = await _usageRepository.AddAsync(usageRecord);

            // Raise domain event
            await _mediator.Publish(new UsageRecordedEvent
            {
                TenantId = subscription.TenantId,
                SubscriptionId = request.SubscriptionId,
                MetricName = request.MetricName,
                Quantity = request.Quantity
            }, cancellationToken);

            return new UsageRecordDto
            {
                Id = createdRecord.Id,
                SubscriptionId = createdRecord.SubscriptionId,
                MetricName = createdRecord.MetricName,
                Quantity = createdRecord.Quantity,
                RecordedAt = createdRecord.RecordedAt
            };
        }
    }
}


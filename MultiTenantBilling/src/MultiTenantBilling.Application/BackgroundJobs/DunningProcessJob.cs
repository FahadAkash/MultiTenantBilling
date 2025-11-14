using MediatR;
using Microsoft.Extensions.Logging;
using MultiTenantBilling.Application.Commands;
using MultiTenantBilling.Application.Queries;
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Repositories;

namespace MultiTenantBilling.Application.BackgroundJobs
{
    /// <summary>
    /// Background job to handle dunning process (payment failure recovery)
    /// Runs daily at 8 AM
    /// </summary>
    public class DunningProcessJob
    {
        private readonly IMediator _mediator;
        private readonly ITenantRepository<Invoice> _invoiceRepository;
        private readonly ITenantRepository<Subscription> _subscriptionRepository;
        private readonly ILogger<DunningProcessJob> _logger;

        public DunningProcessJob(
            IMediator mediator,
            ITenantRepository<Invoice> invoiceRepository,
            ITenantRepository<Subscription> subscriptionRepository,
            ILogger<DunningProcessJob> logger)
        {
            _mediator = mediator;
            _invoiceRepository = invoiceRepository;
            _subscriptionRepository = subscriptionRepository;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            _logger.LogInformation("Starting dunning process job at {Time}", DateTime.UtcNow);

            try
            {
                // Get all failed payments
                var failedPayments = await _mediator.Send(new GetFailedPaymentsQuery
                {
                    MaxRetryAttempts = 3
                });

                _logger.LogInformation("Found {Count} failed payments in dunning process", failedPayments.Count);

                foreach (var failedPayment in failedPayments)
                {
                    try
                    {
                        var invoice = await _invoiceRepository.GetByIdAsync(failedPayment.InvoiceId);
                        if (invoice == null) continue;

                        var daysSinceFailure = (DateTime.UtcNow - failedPayment.LastAttemptDate).Days;

                        // Day 0: Immediate retry (already handled by PaymentRetryJob)
                        // Day 3: Send urgent email
                        if (daysSinceFailure == 3)
                        {
                            _logger.LogWarning("Sending urgent payment reminder for invoice {InvoiceId}",
                                failedPayment.InvoiceId);
                            // TODO: Send urgent email
                        }
                        // Day 7: Final warning
                        else if (daysSinceFailure == 7)
                        {
                            _logger.LogWarning("Sending final payment warning for invoice {InvoiceId}",
                                failedPayment.InvoiceId);
                            // TODO: Send final warning email
                        }
                        // Day 14: Suspend subscription
                        else if (daysSinceFailure >= 14)
                        {
                            _logger.LogError("Suspending subscription for tenant {TenantId} due to payment failure",
                                failedPayment.TenantId);

                            var subscription = (await _subscriptionRepository.GetByTenantIdAsync(failedPayment.TenantId))
                                .FirstOrDefault(s => s.Status == "Active");

                            if (subscription != null)
                            {
                                await _mediator.Send(new SuspendSubscriptionCommand
                                {
                                    SubscriptionId = subscription.Id,
                                    Reason = $"Payment failed for invoice {failedPayment.InvoiceId} after {failedPayment.RetryAttempt} attempts"
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing dunning for invoice {InvoiceId}",
                            failedPayment.InvoiceId);
                        // Continue with next payment
                    }
                }

                _logger.LogInformation("Dunning process job completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in dunning process job");
                throw;
            }
        }
    }
}


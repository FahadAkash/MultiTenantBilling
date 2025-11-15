using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;
using MultiTenantBilling.Domain.Events;
using System;
using System.Threading;
using System.Threading.Tasks;
using MultiTenantBilling.Infrastructure.Repositories;
using MultiTenantBilling.Domain.Entities;

namespace MultiTenantBilling.Application.EventHandlers
{
    /// <summary>
    /// Event handler for payment failed events
    /// Handles: dunning process, retry scheduling, notifications
    /// </summary>
    public class PaymentFailedEventHandler : INotificationHandler<PaymentFailedEvent>
    {
        private readonly ILogger<PaymentFailedEventHandler> _logger;
        private readonly ITenantRepository<Invoice> _invoiceRepository;
        private readonly ITenantRepository<Subscription> _subscriptionRepository;

        public PaymentFailedEventHandler(
            ILogger<PaymentFailedEventHandler> logger,
            ITenantRepository<Invoice> invoiceRepository,
            ITenantRepository<Subscription> subscriptionRepository)
        {
            _logger = logger;
            _invoiceRepository = invoiceRepository;
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task Handle(PaymentFailedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogWarning("Payment failed for invoice {InvoiceId}, attempt: {RetryAttempt}, reason: {FailureReason}",
                notification.InvoiceId, notification.RetryAttempt, notification.FailureReason);

            // Get the invoice to find the associated subscription
            var invoice = await _invoiceRepository.GetByIdAsync(notification.InvoiceId);
            if (invoice == null)
            {
                _logger.LogError("Invoice {InvoiceId} not found", notification.InvoiceId);
                return;
            }

            // Schedule retry based on attempt number
            if (notification.RetryAttempt < 3)
            {
                var retryDelay = GetRetryDelay(notification.RetryAttempt);
                
                // Schedule retry via MediatR command (will be handled by background job)
                BackgroundJob.Schedule<IMediator>(x => 
                    x.Send(new Commands.RetryFailedPaymentCommand 
                    { 
                        InvoiceId = notification.InvoiceId, 
                        RetryAttempt = notification.RetryAttempt + 1 
                    }, cancellationToken), 
                    retryDelay);
                
                _logger.LogInformation("Scheduled payment retry for invoice {InvoiceId} in {Delay}",
                    notification.InvoiceId, retryDelay);
            }
            else
            {
                _logger.LogError("Max retry attempts reached for invoice {InvoiceId}. Suspending subscription.",
                    notification.InvoiceId);
                
                // Cancel the subscription after multiple failed payment attempts
                // Check if the invoice has a valid subscription ID (not empty GUID)
                if (invoice.SubscriptionId != Guid.Empty)
                {
                    await CancelSubscriptionAsync(invoice.SubscriptionId);
                }
            }

            // TODO: Send failure notification email
            // TODO: Update analytics

            await Task.CompletedTask;
        }

        private TimeSpan GetRetryDelay(int attempt)
        {
            // Retry schedule: Day 0 (immediate), Day 3, Day 7
            return attempt switch
            {
                1 => TimeSpan.FromDays(3),
                2 => TimeSpan.FromDays(7),
                _ => TimeSpan.FromDays(14)
            };
        }

        private async Task CancelSubscriptionAsync(Guid subscriptionId)
        {
            try
            {
                var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
                if (subscription != null)
                {
                    subscription.Status = "Canceled";
                    await _subscriptionRepository.UpdateAsync(subscription);
                    
                    _logger.LogInformation("Subscription {SubscriptionId} canceled due to failed payments", subscriptionId);
                    
                    // TODO: Send cancellation notification to customer
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling subscription {SubscriptionId}", subscriptionId);
            }
        }
    }
}
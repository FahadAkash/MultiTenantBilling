using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;
using MultiTenantBilling.Domain.Events;

namespace MultiTenantBilling.Application.EventHandlers
{
    /// <summary>
    /// Event handler for payment failed events
    /// Handles: dunning process, retry scheduling, notifications
    /// </summary>
    public class PaymentFailedEventHandler : INotificationHandler<PaymentFailedEvent>
    {
        private readonly ILogger<PaymentFailedEventHandler> _logger;

        public PaymentFailedEventHandler(ILogger<PaymentFailedEventHandler> logger)
        {
            _logger = logger;
        }

        public async Task Handle(PaymentFailedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogWarning("Payment failed for invoice {InvoiceId}, attempt: {RetryAttempt}, reason: {FailureReason}",
                notification.InvoiceId, notification.RetryAttempt, notification.FailureReason);

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
                
                // Schedule suspension via MediatR command
                BackgroundJob.Enqueue<IMediator>(x => 
                    x.Send(new Commands.SuspendSubscriptionCommand 
                    { 
                        SubscriptionId = Guid.Empty, // TODO: Get subscription ID from invoice
                        Reason = $"Payment failed after {notification.RetryAttempt} attempts"
                    }, cancellationToken));
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
    }
}


using MediatR;
using Microsoft.Extensions.Logging;
using MultiTenantBilling.Application.Commands;
using MultiTenantBilling.Application.Queries;

namespace MultiTenantBilling.Application.BackgroundJobs
{
    /// <summary>
    /// Background job to retry failed payments (dunning process)
    /// Runs every 15 minutes
    /// </summary>
    public class PaymentRetryJob
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PaymentRetryJob> _logger;

        public PaymentRetryJob(IMediator mediator, ILogger<PaymentRetryJob> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            _logger.LogInformation("Starting payment retry job at {Time}", DateTime.UtcNow);

            try
            {
                // Get failed payments that need retry
                var failedPayments = await _mediator.Send(new GetFailedPaymentsQuery
                {
                    MaxRetryAttempts = 3,
                    OlderThan = DateTime.UtcNow.AddDays(-14) // Only retry payments from last 14 days
                });

                _logger.LogInformation("Found {Count} failed payments to retry", failedPayments.Count);

                foreach (var failedPayment in failedPayments)
                {
                    try
                    {
                        _logger.LogInformation("Retrying payment for invoice {InvoiceId}, attempt {RetryAttempt}",
                            failedPayment.InvoiceId, failedPayment.RetryAttempt + 1);

                        // Retry payment
                        var result = await _mediator.Send(new RetryFailedPaymentCommand
                        {
                            InvoiceId = failedPayment.InvoiceId,
                            RetryAttempt = failedPayment.RetryAttempt + 1
                        });

                        if (result)
                        {
                            _logger.LogInformation("Payment retry succeeded for invoice {InvoiceId}",
                                failedPayment.InvoiceId);
                        }
                        else
                        {
                            _logger.LogWarning("Payment retry failed for invoice {InvoiceId}",
                                failedPayment.InvoiceId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error retrying payment for invoice {InvoiceId}",
                            failedPayment.InvoiceId);
                        // Continue with next payment
                    }
                }

                _logger.LogInformation("Payment retry job completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in payment retry job");
                throw;
            }
        }
    }
}


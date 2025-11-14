using MediatR;
using Microsoft.Extensions.Logging;
using MultiTenantBilling.Application.Commands;
using MultiTenantBilling.Application.Queries;

namespace MultiTenantBilling.Application.BackgroundJobs
{
    /// <summary>
    /// Background job to generate invoices for subscriptions that are due for billing
    /// Runs daily at 2 AM
    /// </summary>
    public class InvoiceGenerationJob
    {
        private readonly IMediator _mediator;
        private readonly ILogger<InvoiceGenerationJob> _logger;

        public InvoiceGenerationJob(IMediator mediator, ILogger<InvoiceGenerationJob> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            _logger.LogInformation("Starting invoice generation job at {Time}", DateTime.UtcNow);

            try
            {
                var billingDate = DateTime.UtcNow.Date;

                // Get all subscriptions that need billing
                var upcomingBilling = await _mediator.Send(new GetUpcomingBillingQuery
                {
                    BillingDate = billingDate
                });

                _logger.LogInformation("Found {Count} subscriptions due for billing", upcomingBilling.Count);

                foreach (var billing in upcomingBilling)
                {
                    try
                    {
                        _logger.LogInformation("Generating invoice for subscription {SubscriptionId}, tenant {TenantId}",
                            billing.SubscriptionId, billing.TenantId);

                        // Generate invoice
                        var invoice = await _mediator.Send(new GenerateInvoiceCommand
                        {
                            SubscriptionId = billing.SubscriptionId,
                            InvoiceDate = billingDate,
                            IncludeOverage = true
                        });

                        _logger.LogInformation("Invoice {InvoiceId} generated successfully for tenant {TenantId}",
                            invoice.Id, billing.TenantId);

                        // TODO: Trigger automatic payment if payment method is on file
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error generating invoice for subscription {SubscriptionId}",
                            billing.SubscriptionId);
                        // Continue with next subscription
                    }
                }

                _logger.LogInformation("Invoice generation job completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in invoice generation job");
                throw;
            }
        }
    }
}


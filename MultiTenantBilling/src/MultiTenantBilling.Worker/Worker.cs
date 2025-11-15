using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MultiTenantBilling.Application.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using MultiTenantBilling.Application.BackgroundJobs;
using MultiTenantBilling.Infrastructure.Repositories;
using MultiTenantBilling.Domain.Entities;

namespace MultiTenantBilling.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }

                // Run billing tasks
                await ProcessBillingTasks(stoppingToken);

                // Wait for 1 hour before next run
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task ProcessBillingTasks(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Starting billing processing cycle");

                // Create a scope for dependency injection
                using var scope = _serviceProvider.CreateScope();
                
                // Get required services
                var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
                var invoiceService = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
                var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
                
                // Run the invoice generation job
                var invoiceJob = scope.ServiceProvider.GetRequiredService<InvoiceGenerationJob>();
                await invoiceJob.ExecuteAsync();
                
                // Process expired subscriptions (check for subscriptions that should be canceled)
                await ProcessExpiredSubscriptions(scope, stoppingToken);
                
                _logger.LogInformation("Billing processing cycle completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during billing processing");
            }
        }
        
        private async Task ProcessExpiredSubscriptions(IServiceScope scope, CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Processing expired subscriptions");
                
                var subscriptionRepository = scope.ServiceProvider.GetRequiredService<ITenantRepository<Subscription>>();
                
                // Get all expired subscriptions (status = "Expired")
                var expiredSubscriptions = await subscriptionRepository.GetAllAsync();
                var subscriptionsToCancel = expiredSubscriptions.Where(s => s.Status == "Expired").ToList();
                
                _logger.LogInformation("Found {Count} expired subscriptions to process", subscriptionsToCancel.Count);
                
                foreach (var subscription in subscriptionsToCancel)
                {
                    // Check if the subscription has been expired for more than the grace period (7 days)
                    if (subscription.EndDate.AddDays(7) <= DateTime.UtcNow)
                    {
                        _logger.LogInformation("Canceling subscription {SubscriptionId} due to expiration", subscription.Id);
                        
                        // Cancel the subscription
                        subscription.Status = "Canceled";
                        await subscriptionRepository.UpdateAsync(subscription);
                        
                        // TODO: Send cancellation notification to customer
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during expired subscription processing");
            }
        }
    }
}
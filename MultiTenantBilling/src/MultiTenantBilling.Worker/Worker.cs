using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MultiTenantBilling.Application.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

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
                
                // In a real implementation, you would:
                // 1. Find subscriptions that need billing today
                // 2. Generate invoices for those subscriptions
                // 3. Attempt to process payments for pending invoices
                // 4. Handle failed payments (dunning process)
                
                _logger.LogInformation("Billing processing cycle completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during billing processing");
            }
        }
    }
}
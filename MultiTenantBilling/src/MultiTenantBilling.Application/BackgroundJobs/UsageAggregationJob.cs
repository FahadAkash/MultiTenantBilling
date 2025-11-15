using Microsoft.Extensions.Logging;
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Repositories;

namespace MultiTenantBilling.Application.BackgroundJobs
{
    /// <summary>
    /// Background job to aggregate usage records for analytics and billing
    /// Runs hourly
    /// </summary>
    public class UsageAggregationJob
    {
        private readonly ITenantRepository<UsageRecord> _usageRepository;
        private readonly ILogger<UsageAggregationJob> _logger;

        public UsageAggregationJob(
            ITenantRepository<UsageRecord> usageRepository,
            ILogger<UsageAggregationJob> logger)
        {
            _usageRepository = usageRepository;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            _logger.LogInformation("Starting usage aggregation job at {Time}", DateTime.UtcNow);

            try
            {
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddHours(-1); // Aggregate last hour

                // Get all usage records from the last hour
                var allUsage = await _usageRepository.GetAllEntitiesAsync();
                var recentUsage = allUsage
                    .Where(u => u.RecordedAt >= startDate && u.RecordedAt < endDate)
                    .ToList();

                // Group by tenant and metric
                var aggregated = recentUsage
                    .GroupBy(u => new { u.TenantId, u.MetricName })
                    .Select(g => new
                    {
                        g.Key.TenantId,
                        g.Key.MetricName,
                        TotalQuantity = g.Sum(u => u.Quantity),
                        RecordCount = g.Count()
                    })
                    .ToList();

                _logger.LogInformation("Aggregated {Count} usage metrics", aggregated.Count);

                // TODO: Store aggregated data in UsageAggregates table for fast queries
                // TODO: Calculate overages and update subscription limits
                // TODO: Trigger alerts if usage exceeds limits

                _logger.LogInformation("Usage aggregation job completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in usage aggregation job");
                throw;
            }
        }
    }
}


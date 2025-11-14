using MultiTenantBilling.Application.DTOs;

namespace MultiTenantBilling.Application.Commands
{
    /// <summary>
    /// Command to record usage for a subscription
    /// </summary>
    public class RecordUsageCommand : ICommand<UsageRecordDto>
    {
        public Guid SubscriptionId { get; set; }
        public string MetricName { get; set; } = default!;
        public double Quantity { get; set; }
        public DateTime? RecordedAt { get; set; }
    }
}


using MultiTenantBilling.Domain.Common;
using System;

namespace MultiTenantBilling.Domain.Entities
{
    public class UsageRecord : BaseEntity
    {
        public Guid SubscriptionId { get; set; }
        public Subscription Subscription { get; set; } = default!;

        public string MetricName { get; set; } = default!; // e.g., "API Calls", "Storage GB"
        public double Quantity { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }
}
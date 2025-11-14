using MultiTenantBilling.Domain.Common;
using MultiTenantBilling.Domain.Interface;
using System;

namespace MultiTenantBilling.Domain.Entities
{
    public class UsageRecord : BaseEntity, ITenantEntity
    {
        public Guid TenantId { get; set; }
        public Guid SubscriptionId { get; set; }
        public Subscription Subscription { get; set; } = default!;

        public string MetricName { get; set; } = default!; // e.g., "API Calls", "Storage GB"
        public double Quantity { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }
}
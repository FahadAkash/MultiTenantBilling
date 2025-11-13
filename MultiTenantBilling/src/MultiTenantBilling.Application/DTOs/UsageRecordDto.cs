using System;

namespace MultiTenantBilling.Application.DTOs
{
    public class UsageRecordDto
    {
        public Guid Id { get; set; }
        public Guid SubscriptionId { get; set; }
        public string MetricName { get; set; } = default!;
        public double Quantity { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}
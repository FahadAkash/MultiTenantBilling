namespace MultiTenantBilling.Domain.Events
{
    /// <summary>
    /// Event raised when usage is recorded
    /// </summary>
    public class UsageRecordedEvent : IDomainEvent
    {
        public Guid TenantId { get; set; }
        public Guid SubscriptionId { get; set; }
        public string MetricName { get; set; } = default!;
        public double Quantity { get; set; }
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    }
}


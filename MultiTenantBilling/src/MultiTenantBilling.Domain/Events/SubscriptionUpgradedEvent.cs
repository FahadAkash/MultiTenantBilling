namespace MultiTenantBilling.Domain.Events
{
    /// <summary>
    /// Event raised when a subscription is upgraded/downgraded
    /// </summary>
    public class SubscriptionUpgradedEvent : IDomainEvent
    {
        public Guid TenantId { get; set; }
        public Guid SubscriptionId { get; set; }
        public Guid OldPlanId { get; set; }
        public Guid NewPlanId { get; set; }
        public decimal ProratedAmount { get; set; }
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    }
}


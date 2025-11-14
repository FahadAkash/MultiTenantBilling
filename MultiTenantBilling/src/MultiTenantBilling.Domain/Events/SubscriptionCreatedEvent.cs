namespace MultiTenantBilling.Domain.Events
{
    /// <summary>
    /// Event raised when a subscription is created
    /// </summary>
    public class SubscriptionCreatedEvent : IDomainEvent
    {
        public Guid TenantId { get; set; }
        public Guid SubscriptionId { get; set; }
        public Guid PlanId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    }
}


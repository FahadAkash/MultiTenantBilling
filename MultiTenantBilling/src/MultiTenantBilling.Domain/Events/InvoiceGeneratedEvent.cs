namespace MultiTenantBilling.Domain.Events
{
    /// <summary>
    /// Event raised when an invoice is generated
    /// </summary>
    public class InvoiceGeneratedEvent : IDomainEvent
    {
        public Guid TenantId { get; set; }
        public Guid InvoiceId { get; set; }
        public Guid SubscriptionId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    }
}


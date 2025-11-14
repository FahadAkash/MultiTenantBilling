namespace MultiTenantBilling.Domain.Events
{
    /// <summary>
    /// Event raised when a payment fails
    /// </summary>
    public class PaymentFailedEvent : IDomainEvent
    {
        public Guid TenantId { get; set; }
        public Guid InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public string FailureReason { get; set; } = default!;
        public int RetryAttempt { get; set; }
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    }
}


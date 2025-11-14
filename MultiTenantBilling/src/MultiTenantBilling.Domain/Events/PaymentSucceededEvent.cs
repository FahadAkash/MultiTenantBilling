namespace MultiTenantBilling.Domain.Events
{
    /// <summary>
    /// Event raised when a payment is successfully processed
    /// </summary>
    public class PaymentSucceededEvent : IDomainEvent
    {
        public Guid TenantId { get; set; }
        public Guid InvoiceId { get; set; }
        public Guid PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethodId { get; set; } = default!;
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    }
}


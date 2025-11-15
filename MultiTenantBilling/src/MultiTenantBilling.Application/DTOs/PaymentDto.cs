using System;

namespace MultiTenantBilling.Application.DTOs
{
    public class PaymentDto
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Method { get; set; } = default!;
        public string Status { get; set; } = default!;
        public string? TransactionId { get; set; }
        public bool IsRetry { get; set; }
        public int RetryAttempt { get; set; }
        public string? FailureReason { get; set; }
    }
}
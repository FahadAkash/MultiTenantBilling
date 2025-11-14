using MultiTenantBilling.Application.DTOs;

namespace MultiTenantBilling.Application.Queries
{
    /// <summary>
    /// Query to get failed payments that need retry (for dunning process)
    /// </summary>
    public class GetFailedPaymentsQuery : IQuery<List<FailedPaymentDto>>
    {
        public int? MaxRetryAttempts { get; set; }
        public DateTime? OlderThan { get; set; }
    }

    public class FailedPaymentDto
    {
        public Guid InvoiceId { get; set; }
        public Guid TenantId { get; set; }
        public decimal Amount { get; set; }
        public int RetryAttempt { get; set; }
        public DateTime LastAttemptDate { get; set; }
        public string FailureReason { get; set; } = default!;
    }
}


using MultiTenantBilling.Application.DTOs;

namespace MultiTenantBilling.Application.Commands
{
    /// <summary>
    /// Command to process a payment for an invoice
    /// </summary>
    public class ProcessPaymentCommand : ICommand<PaymentDto>
    {
        public Guid InvoiceId { get; set; }
        public string PaymentMethodId { get; set; } = default!;
        public bool IsRetry { get; set; } = false;
        public int RetryAttempt { get; set; } = 0;
    }
}


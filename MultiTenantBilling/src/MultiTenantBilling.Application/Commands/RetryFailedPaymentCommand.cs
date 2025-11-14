namespace MultiTenantBilling.Application.Commands
{
    /// <summary>
    /// Command to retry a failed payment (part of dunning process)
    /// </summary>
    public class RetryFailedPaymentCommand : ICommand<bool>
    {
        public Guid InvoiceId { get; set; }
        public int RetryAttempt { get; set; }
    }
}


using MultiTenantBilling.Application.DTOs;

namespace MultiTenantBilling.Application.Commands
{
    /// <summary>
    /// Command to generate an invoice for a subscription
    /// </summary>
    public class GenerateInvoiceCommand : ICommand<InvoiceDto>
    {
        public Guid SubscriptionId { get; set; }
        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
        public bool IncludeOverage { get; set; } = true;
    }
}


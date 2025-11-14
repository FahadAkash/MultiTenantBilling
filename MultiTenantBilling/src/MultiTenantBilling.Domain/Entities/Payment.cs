using MultiTenantBilling.Domain.Common;
using MultiTenantBilling.Domain.Interface;
using System;

namespace MultiTenantBilling.Domain.Entities
{
    public class Payment : BaseEntity, ITenantEntity
    {
        public Guid TenantId { get; set; }
        public Guid InvoiceId { get; set; }
        public Invoice Invoice { get; set; } = default!;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string Method { get; set; } = "Stripe"; // Stripe, Bank, Manual
        public string Status { get; set; } = "Success"; // Success, Failed
        public string? TransactionId { get; set; }
    }
}
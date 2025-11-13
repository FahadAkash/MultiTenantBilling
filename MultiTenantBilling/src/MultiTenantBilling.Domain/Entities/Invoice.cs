using MultiTenantBilling.Domain.Common;
using System;
using System.Collections.Generic;

namespace MultiTenantBilling.Domain.Entities
{
    public class Invoice : BaseEntity
    {
        public Guid SubscriptionId { get; set; }
        public Subscription Subscription { get; set; } = default!;

        public decimal Amount { get; set; }
        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; }
        public bool IsPaid { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Paid, Overdue
        public string? StripeInvoiceId { get; set; }

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
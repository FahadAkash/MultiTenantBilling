using System;
using System.Collections.Generic;

namespace MultiTenantBilling.Application.DTOs
{
    public class InvoiceDto
    {
        public Guid Id { get; set; }
        public Guid SubscriptionId { get; set; }
        public decimal Amount { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = default!;
        public bool IsPaid { get; set; }
        public IEnumerable<PaymentDto>? Payments { get; set; }
    }
}
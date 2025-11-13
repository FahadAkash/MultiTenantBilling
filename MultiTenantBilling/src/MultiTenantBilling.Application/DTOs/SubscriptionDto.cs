using System;
using System.Collections.Generic;

namespace MultiTenantBilling.Application.DTOs
{
    public class SubscriptionDto
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string TenantName { get; set; } = default!;
        public Guid PlanId { get; set; }
        public string PlanName { get; set; } = default!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = default!;
        public IEnumerable<InvoiceDto>? Invoices { get; set; }
    }
}
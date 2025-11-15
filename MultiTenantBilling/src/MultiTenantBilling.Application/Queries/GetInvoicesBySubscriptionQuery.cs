using MultiTenantBilling.Application.DTOs;
using System;
using System.Collections.Generic;

namespace MultiTenantBilling.Application.Queries
{
    /// <summary>
    /// Query to get invoices for a specific subscription
    /// </summary>
    public class GetInvoicesBySubscriptionQuery : IQuery<IEnumerable<InvoiceDto>>
    {
        public Guid SubscriptionId { get; set; }
        public Guid TenantId { get; set; }
    }
}
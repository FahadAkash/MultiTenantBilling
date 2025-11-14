using MultiTenantBilling.Domain.Common;
using MultiTenantBilling.Domain.Interface;
using System;
using System.Collections.Generic;

namespace MultiTenantBilling.Domain.Entities
{
    public class Subscription : BaseEntity, ITenantEntity
    {
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = default!;

        public Guid PlanId { get; set; }
        public Plan Plan { get; set; } = default!;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Active"; // Active, Paused, Canceled
        public string? StripeSubscriptionId { get; set; }

        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public ICollection<UsageRecord> UsageRecords { get; set; } = new List<UsageRecord>();
    }
}
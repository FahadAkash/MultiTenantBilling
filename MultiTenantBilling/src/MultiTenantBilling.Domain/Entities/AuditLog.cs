using MultiTenantBilling.Domain.Common;
using System;

namespace MultiTenantBilling.Domain.Entities
{
    public class AuditLog : BaseEntity
    {
        public Guid? TenantId { get; set; }
        public string UserEmail { get; set; } = default!;
        public string Action { get; set; } = default!; // e.g. "CreatedInvoice"
        public string EntityName { get; set; } = default!;
        public string EntityId { get; set; } = default!;
        public string Changes { get; set; } = default!; // JSON diff or description
    }
}
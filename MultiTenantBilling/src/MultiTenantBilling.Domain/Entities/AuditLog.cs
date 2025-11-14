using MultiTenantBilling.Domain.Common;
using MultiTenantBilling.Domain.Interface;
using System;

namespace MultiTenantBilling.Domain.Entities
{
    public class AuditLog : BaseEntity, ITenantEntity
    {
        public Guid TenantId { get; set; }
        public Guid? TenantIdNullable { get; set; }
        public string UserEmail { get; set; } = default!;
        public string Action { get; set; } = default!; // e.g. "CreatedInvoice"
        public string EntityName { get; set; } = default!;
        public string EntityId { get; set; } = default!;
        public string Changes { get; set; } = default!; // JSON diff or description
    }
}
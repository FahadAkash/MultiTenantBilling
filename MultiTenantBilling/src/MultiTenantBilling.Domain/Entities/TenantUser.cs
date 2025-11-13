using MultiTenantBilling.Domain.Common;
using System;

namespace MultiTenantBilling.Domain.Entities
{
    public class TenantUser : BaseEntity
    {
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Role { get; set; } = "Viewer"; // TenantAdmin, Finance, Viewer
    }
}
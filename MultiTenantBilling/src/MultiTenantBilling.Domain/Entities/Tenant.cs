using MultiTenantBilling.Domain.Common;
using System.Collections.Generic;

namespace MultiTenantBilling.Domain.Entities
{
    public class Tenant : BaseEntity
    {
        public string Name { get; set; } = default!;
        public string Subdomain { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? Phone { get; set; }
        public string Status { get; set; } = "Active";

        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<TenantUser> Users { get; set; } = new List<TenantUser>();
    }
}
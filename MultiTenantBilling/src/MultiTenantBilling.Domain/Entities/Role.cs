using MultiTenantBilling.Domain.Common;
using MultiTenantBilling.Domain.Interface;
using System;
using System.Collections.Generic;

namespace MultiTenantBilling.Domain.Entities
{
    public class Role : BaseEntity, ITenantEntity
    {
        public Guid TenantId { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
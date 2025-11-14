using MultiTenantBilling.Domain.Common;
using MultiTenantBilling.Domain.Interface;
using System;

namespace MultiTenantBilling.Domain.Entities
{
    public class UserRole : BaseEntity, ITenantEntity
    {
        public Guid TenantId { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
        public Guid RoleId { get; set; }
        public Role Role { get; set; } = default!;
    }
}
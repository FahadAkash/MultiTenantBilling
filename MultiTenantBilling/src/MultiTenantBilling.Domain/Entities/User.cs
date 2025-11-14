using MultiTenantBilling.Domain.Common;
using MultiTenantBilling.Domain.Interface;
using System;
using System.Collections.Generic;

namespace MultiTenantBilling.Domain.Entities
{
    public class User : BaseEntity, ITenantEntity
    {
        public Guid TenantId { get; set; }
        public string Email { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginAt { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
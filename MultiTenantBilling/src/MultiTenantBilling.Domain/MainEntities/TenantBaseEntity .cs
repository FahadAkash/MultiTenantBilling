using MultiTenantBilling.Domain.Common;
using MultiTenantBilling.Domain.Interface;
using System;

namespace MultiTenantBilling.Domain.MainEntities
{
    internal class TenantBaseEntity : BaseEntity, ITenantEntity, ISoftDeletable
    {
        public Guid TenantId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}
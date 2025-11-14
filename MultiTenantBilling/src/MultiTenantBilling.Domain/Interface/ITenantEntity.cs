using System;

namespace MultiTenantBilling.Domain.Interface
{
    public interface ITenantEntity
    {
        Guid TenantId { get; set; }
    }
}
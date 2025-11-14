using System;

namespace MultiTenantBilling.Application.Services
{
    public interface ITenantService
    {
        Guid? GetTenantId();
        void SetTenantId(Guid tenantId);
        bool IsTenantAvailable();
        Guid GetRequiredTenantId();
    }
}
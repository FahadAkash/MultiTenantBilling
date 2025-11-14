using System;

namespace MultiTenantBilling.Api.Services
{
    public interface IApiTenantService
    {
        Guid? GetTenantId();
        void SetTenantId(Guid tenantId);
        bool IsTenantAvailable();
        Guid GetRequiredTenantId();
    }
}
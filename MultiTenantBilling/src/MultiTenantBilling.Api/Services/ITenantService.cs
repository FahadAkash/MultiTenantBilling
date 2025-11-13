using System;

namespace MultiTenantBilling.Api.Services
{
    public interface ITenantService
    {
        Guid? GetTenantId();
        void SetTenantId(Guid tenantId);
    }
}
using MultiTenantBilling.Application.Services;
using System;

namespace MultiTenantBilling.Worker.Services
{
    public class MockTenantService : ITenantService
    {
        // For worker services, we might need to set the tenant ID manually
        // or get it from the context of the operation being performed
        private Guid? _tenantId;

        public Guid? GetTenantId()
        {
            return _tenantId;
        }

        public void SetTenantId(Guid tenantId)
        {
            _tenantId = tenantId;
        }

        public bool IsTenantAvailable()
        {
            return _tenantId.HasValue;
        }

        public Guid GetRequiredTenantId()
        {
            if (!_tenantId.HasValue)
            {
                throw new InvalidOperationException("Tenant ID is required but not available.");
            }
            
            return _tenantId.Value;
        }
    }
}
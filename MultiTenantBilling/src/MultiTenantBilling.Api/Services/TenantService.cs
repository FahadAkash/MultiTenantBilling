using Microsoft.AspNetCore.Http;
using MultiTenantBilling.Application.Services; // Add this using statement
using System;

namespace MultiTenantBilling.Api.Services
{
    public class TenantService : IApiTenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? GetTenantId()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.Items["TenantId"] is Guid tenantId)
            {
                return tenantId;
            }
            return null;
        }

        public bool IsTenantAvailable()
        {
            return GetTenantId().HasValue;
        }

        public Guid GetRequiredTenantId()
        {
            var tenantId = GetTenantId();
            if (!tenantId.HasValue)
            {
                throw new InvalidOperationException("Tenant ID is required but not available. Make sure the TenantMiddleware is registered and functioning correctly.");
            }
            return tenantId.Value;
        }

        public void SetTenantId(Guid tenantId)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                context.Items["TenantId"] = tenantId;
            }
        }
    }
}
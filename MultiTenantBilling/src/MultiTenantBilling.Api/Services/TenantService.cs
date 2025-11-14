using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;

namespace MultiTenantBilling.Api.Services
{
    public class TenantService : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TenantService> _logger;

        public TenantService(IHttpContextAccessor httpContextAccessor, ILogger<TenantService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
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

        public void SetTenantId(Guid tenantId)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                context.Items["TenantId"] = tenantId;
            }
            else
            {
                _logger.LogWarning("Unable to set tenant ID - no HTTP context available");
            }
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
                throw new InvalidOperationException("Tenant ID is required but not available in the current context.");
            }
            
            return tenantId.Value;
        }
    }
}
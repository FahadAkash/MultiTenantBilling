using Microsoft.AspNetCore.Http;
using System;

namespace MultiTenantBilling.Api.Services
{
    public class TenantService : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? GetTenantId()
        {
            if (_httpContextAccessor.HttpContext?.Items["TenantId"] is Guid tenantId)
            {
                return tenantId;
            }
            
            return null;
        }

        public void SetTenantId(Guid tenantId)
        {
            _httpContextAccessor.HttpContext!.Items["TenantId"] = tenantId;
        }
    }
}
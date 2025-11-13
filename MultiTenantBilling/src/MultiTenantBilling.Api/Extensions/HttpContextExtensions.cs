using Microsoft.AspNetCore.Http;
using System;

namespace MultiTenantBilling.Api.Extensions
{
    public static class HttpContextExtensions
    {
        public static Guid? GetTenantId(this HttpContext httpContext)
        {
            if (httpContext.Items["TenantId"] is Guid tenantId)
            {
                return tenantId;
            }
            
            return null;
        }

        public static void SetTenantId(this HttpContext httpContext, Guid tenantId)
        {
            httpContext.Items["TenantId"] = tenantId;
        }
    }
}
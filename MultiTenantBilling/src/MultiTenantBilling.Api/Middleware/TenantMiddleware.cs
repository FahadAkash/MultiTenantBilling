using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace MultiTenantBilling.Api.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Extract tenant ID from request header
            if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantIdHeader))
            {
                if (Guid.TryParse(tenantIdHeader, out Guid tenantId))
                {
                    // Store tenant ID in HttpContext items
                    context.Items["TenantId"] = tenantId;
                }
            }

            // Call the next middleware in the pipeline
            await _next(context);
        }
    }
}
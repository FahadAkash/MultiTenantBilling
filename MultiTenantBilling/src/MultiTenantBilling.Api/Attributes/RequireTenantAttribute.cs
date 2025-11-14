using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using MultiTenantBilling.Api.Extensions;
using System;

namespace MultiTenantBilling.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireTenantAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            var logger = httpContext.RequestServices.GetService(typeof(ILogger<RequireTenantAttribute>)) as ILogger;

            var tenantId = httpContext.GetTenantId();
            
            if (!tenantId.HasValue)
            {
                logger?.LogWarning("Unauthorized access attempt without tenant ID");
                context.Result = new UnauthorizedObjectResult(new { 
                    Error = "Tenant ID is required", 
                    Message = "Please provide a valid tenant ID in the request headers, JWT token, or subdomain." 
                });
                return;
            }
            
            logger?.LogInformation("Tenant ID validated: {TenantId}", tenantId.Value);
            base.OnActionExecuting(context);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace MultiTenantBilling.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireTenantAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var tenantId = context.HttpContext.GetTenantId();
            
            if (!tenantId.HasValue)
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            
            base.OnActionExecuting(context);
        }
    }
}
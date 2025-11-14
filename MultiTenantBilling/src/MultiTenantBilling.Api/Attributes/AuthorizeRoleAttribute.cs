using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using MultiTenantBilling.Application.Services;
using System;
using System.Threading.Tasks;

namespace MultiTenantBilling.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly string[] _roles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var authorizationService = context.HttpContext.RequestServices.GetService<IAuthorizationService>();
            
            // In a real implementation, you would get the user email from the JWT token
            var userEmail = "user@example.com";

            var hasRole = false;
            foreach (var role in _roles)
            {
                if (await authorizationService.HasRoleAsync(userEmail, role))
                {
                    hasRole = true;
                    break;
                }
            }

            if (!hasRole)
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }
    }
}
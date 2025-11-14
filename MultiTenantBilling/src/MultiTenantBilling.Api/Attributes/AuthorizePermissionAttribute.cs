using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using MultiTenantBilling.Application.Services;
using System;
using System.Threading.Tasks;

namespace MultiTenantBilling.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizePermissionAttribute : ActionFilterAttribute
    {
        private readonly string[] _permissions;

        public AuthorizePermissionAttribute(params string[] permissions)
        {
            _permissions = permissions;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var authorizationService = context.HttpContext.RequestServices.GetService<IAuthorizationService>();
            
            // In a real implementation, you would get the user email from the JWT token
            var userEmail = "user@example.com";

            var hasPermission = false;
            foreach (var permission in _permissions)
            {
                if (await authorizationService.HasPermissionAsync(userEmail, permission))
                {
                    hasPermission = true;
                    break;
                }
            }

            if (!hasPermission)
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }
    }
}
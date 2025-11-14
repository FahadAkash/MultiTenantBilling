using MultiTenantBilling.Application.Services; // Add this using statement
using System;

namespace MultiTenantBilling.Api.Services
{
    public interface IApiTenantService : ITenantService // Inherit from ITenantService
    {
        // The interface now inherits all members from ITenantService
        // We can add any API-specific members here if needed
    }
}
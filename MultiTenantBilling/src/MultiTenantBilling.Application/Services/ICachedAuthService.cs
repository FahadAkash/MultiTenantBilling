using MultiTenantBilling.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiTenantBilling.Application.Services
{
    public interface ICachedAuthService : IAuthService
    {
        // This interface inherits all methods from IAuthService
        // and can be used to register our cached implementation
    }
}
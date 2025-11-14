using Microsoft.AspNetCore.Mvc;
using MultiTenantBilling.Api.Attributes;
using MultiTenantBilling.Api.Extensions;
using MultiTenantBilling.Api.Services;
using System;

namespace MultiTenantBilling.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TenantTestController : ControllerBase
    {
        private readonly IApiTenantService _tenantService;

        public TenantTestController(IApiTenantService tenantService)
        {
            _tenantService = tenantService;
        }

        [HttpGet("info")]
        public IActionResult GetTenantInfo()
        {
            var tenantId = _tenantService.GetTenantId();
            
            if (!tenantId.HasValue)
            {
                return Unauthorized("Tenant ID is required");
            }

            return Ok(new { TenantId = tenantId.Value });
        }

        [HttpGet("middleware")]
        [RequireTenant]
        public IActionResult GetTenantFromMiddleware()
        {
            var tenantId = HttpContext.GetTenantId();
            
            return Ok(new { TenantId = tenantId });
        }
    }
}
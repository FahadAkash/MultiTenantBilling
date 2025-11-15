using Microsoft.AspNetCore.Mvc;
using MultiTenantBilling.Api.Attributes;
using MultiTenantBilling.Api.Services;
using MultiTenantBilling.Application.Services;
using System;
using System.Threading.Tasks;

namespace MultiTenantBilling.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [RequireTenant]
    public class CacheTestController : ControllerBase
    {
        private readonly IPlanService _planService;
        private readonly IApiTenantService _tenantService;

        public CacheTestController(IPlanService planService, IApiTenantService tenantService)
        {
            _planService = planService;
            _tenantService = tenantService;
        }

        [HttpGet("plans")]
        public async Task<IActionResult> GetAllPlans()
        {
            var plans = await _planService.GetAllPlansAsync();
            return Ok(plans);
        }

        [HttpGet("plans/{planId}")]
        public async Task<IActionResult> GetPlan(Guid planId)
        {
            var plan = await _planService.GetPlanAsync(planId);
            if (plan == null)
                return NotFound();
            return Ok(plan);
        }
    }
}
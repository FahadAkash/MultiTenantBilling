using Microsoft.AspNetCore.Mvc;
using MultiTenantBilling.Api.Attributes;
using MultiTenantBilling.Api.Services;
using MultiTenantBilling.Application.Services;
using MultiTenantBilling.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace MultiTenantBilling.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [RequireTenant]
    public class RedisTestController : ControllerBase
    {
        private readonly IPlanService _planService;
        private readonly ICacheService _cacheService;
        private readonly IApiTenantService _tenantService;

        public RedisTestController(IPlanService planService, ICacheService cacheService, IApiTenantService tenantService)
        {
            _planService = planService;
            _cacheService = cacheService;
            _tenantService = tenantService;
        }

        [HttpPost("plans")]
        public async Task<IActionResult> CreatePlan([FromBody] Plan plan)
        {
            var planDto = await _planService.CreatePlanAsync(plan);
            return Ok(planDto);
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

        [HttpPost("cache/test")]
        public async Task<IActionResult> TestCache([FromBody] TestCacheRequest request)
        {
            var tenantId = _tenantService.GetRequiredTenantId();
            var cacheKey = $"test_{tenantId}_{request.Key}";
            
            // Set value in cache
            await _cacheService.SetAsync(cacheKey, request.Value, TimeSpan.FromMinutes(5));
            
            return Ok(new { Message = "Value cached successfully", Key = cacheKey });
        }

        [HttpGet("cache/test/{key}")]
        public async Task<IActionResult> GetCachedValue(string key)
        {
            var tenantId = _tenantService.GetRequiredTenantId();
            var cacheKey = $"test_{tenantId}_{key}";
            
            // Get value from cache
            var value = await _cacheService.GetAsync<string>(cacheKey);
            
            if (value == null)
                return NotFound(new { Message = "Key not found in cache" });
                
            return Ok(new { Key = key, Value = value });
        }
    }

    public class TestCacheRequest
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
using MultiTenantBilling.Application.DTOs;
using MultiTenantBilling.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiTenantBilling.Application.Services
{
    public class CachedPlanService : IPlanService
    {
        private readonly IPlanService _planService;
        private readonly ICacheService _cacheService;
        private readonly ITenantService _tenantService;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30); // Plans change less frequently

        public CachedPlanService(
            IPlanService planService,
            ICacheService cacheService,
            ITenantService tenantService)
        {
            _planService = planService;
            _cacheService = cacheService;
            _tenantService = tenantService;
        }

        public async Task<PlanDto> CreatePlanAsync(Plan plan)
        {
            // Create the plan using the underlying service
            var planDto = await _planService.CreatePlanAsync(plan);
            
            // Invalidate the cached list of plans since we've added a new one
            var tenantId = _tenantService.GetRequiredTenantId();
            var cacheKey = $"plans_{tenantId}";
            await _cacheService.RemoveAsync(cacheKey);
            
            return planDto;
        }

        public async Task<PlanDto> GetPlanAsync(Guid planId)
        {
            var tenantId = _tenantService.GetRequiredTenantId();
            var cacheKey = $"plan_{tenantId}_{planId}";
            
            // Try to get from cache first
            var cachedPlan = await _cacheService.GetAsync<PlanDto>(cacheKey);
            if (cachedPlan != null)
            {
                return cachedPlan;
            }
            
            // If not in cache, get from the underlying service
            var plan = await _planService.GetPlanAsync(planId);
            
            // Cache the result
            await _cacheService.SetAsync(cacheKey, plan, _cacheExpiration);
            
            return plan;
        }

        public async Task<IEnumerable<PlanDto>> GetAllPlansAsync()
        {
            var tenantId = _tenantService.GetRequiredTenantId();
            var cacheKey = $"plans_{tenantId}";
            
            // Try to get from cache first
            var cachedPlans = await _cacheService.GetAsync<IEnumerable<PlanDto>>(cacheKey);
            if (cachedPlans != null)
            {
                return cachedPlans;
            }
            
            // If not in cache, get from the underlying service
            var plans = await _planService.GetAllPlansAsync();
            
            // Cache the result
            await _cacheService.SetAsync(cacheKey, plans, _cacheExpiration);
            
            return plans;
        }

        public async Task<PlanDto> UpdatePlanAsync(Guid planId, Plan plan)
        {
            // Update the plan using the underlying service
            var planDto = await _planService.UpdatePlanAsync(planId, plan);
            
            // Update the cached plan
            var tenantId = _tenantService.GetRequiredTenantId();
            var cacheKey = $"plan_{tenantId}_{planId}";
            await _cacheService.SetAsync(cacheKey, planDto, _cacheExpiration);
            
            // Also invalidate the cached list of plans
            var plansCacheKey = $"plans_{tenantId}";
            await _cacheService.RemoveAsync(plansCacheKey);
            
            return planDto;
        }

        public async Task<bool> DeletePlanAsync(Guid planId)
        {
            // Delete the plan using the underlying service
            var result = await _planService.DeletePlanAsync(planId);
            
            if (result)
            {
                // Remove the cached plan
                var tenantId = _tenantService.GetRequiredTenantId();
                var cacheKey = $"plan_{tenantId}_{planId}";
                await _cacheService.RemoveAsync(cacheKey);
                
                // Also invalidate the cached list of plans
                var plansCacheKey = $"plans_{tenantId}";
                await _cacheService.RemoveAsync(plansCacheKey);
            }
            
            return result;
        }
    }
}
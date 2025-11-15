using MultiTenantBilling.Application.DTOs;
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiTenantBilling.Application.Services
{
    public class PlanService : IPlanService
    {
        private readonly ITenantRepository<Plan> _planRepository;
        private readonly ITenantService _tenantService;

        public PlanService(ITenantRepository<Plan> planRepository, ITenantService tenantService)
        {
            _planRepository = planRepository;
            _tenantService = tenantService;
        }

        public async Task<PlanDto> CreatePlanAsync(Plan plan)
        {
            // Set the tenant ID from the current tenant context
            plan.TenantId = _tenantService.GetRequiredTenantId();
            
            var createdPlan = await _planRepository.AddAsync(plan);
            return MapToDto(createdPlan);
        }

        public async Task<PlanDto> GetPlanAsync(Guid planId)
        {
            var tenantId = _tenantService.GetRequiredTenantId();
            var plan = await _planRepository.GetByIdForTenantAsync(planId, tenantId);
            return plan != null ? MapToDto(plan) : null;
        }

        public async Task<IEnumerable<PlanDto>> GetAllPlansAsync()
        {
            var tenantId = _tenantService.GetRequiredTenantId();
            var plans = await _planRepository.GetByTenantIdAsync(tenantId);
            return plans.Select(MapToDto);
        }

        public async Task<PlanDto> UpdatePlanAsync(Guid planId, Plan plan)
        {
            var tenantId = _tenantService.GetRequiredTenantId();
            var existingPlan = await _planRepository.GetByIdForTenantAsync(planId, tenantId);
            if (existingPlan == null)
                return null;

            // Update properties
            existingPlan.Name = plan.Name;
            existingPlan.Description = plan.Description;
            existingPlan.MonthlyPrice = plan.MonthlyPrice;
            existingPlan.MaxUsers = plan.MaxUsers;
            existingPlan.MaxStorageGb = plan.MaxStorageGb;
            existingPlan.IsActive = plan.IsActive;

            var updatedPlan = await _planRepository.UpdateAsync(existingPlan);
            return MapToDto(updatedPlan);
        }

        public async Task<bool> DeletePlanAsync(Guid planId)
        {
            var tenantId = _tenantService.GetRequiredTenantId();
            var plan = await _planRepository.GetByIdForTenantAsync(planId, tenantId);
            if (plan == null)
                return false;

            await _planRepository.DeleteAsync(plan.Id);
            return true;
        }

        private PlanDto MapToDto(Plan plan)
        {
            return new PlanDto
            {
                Id = plan.Id,
                Name = plan.Name,
                Description = plan.Description,
                MonthlyPrice = plan.MonthlyPrice,
                MaxUsers = plan.MaxUsers,
                MaxStorageGb = plan.MaxStorageGb
            };
        }
    }
}
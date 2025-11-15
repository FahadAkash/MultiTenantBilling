using MultiTenantBilling.Application.DTOs;
using MultiTenantBilling.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiTenantBilling.Application.Services
{
    public interface IPlanService
    {
        Task<PlanDto> CreatePlanAsync(Plan plan);
        Task<PlanDto> GetPlanAsync(Guid planId);
        Task<IEnumerable<PlanDto>> GetAllPlansAsync();
        Task<PlanDto> UpdatePlanAsync(Guid planId, Plan plan);
        Task<bool> DeletePlanAsync(Guid planId);
    }
}
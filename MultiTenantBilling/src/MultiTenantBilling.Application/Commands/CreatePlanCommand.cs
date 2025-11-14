using MediatR;
using MultiTenantBilling.Application.DTOs;
using System;

namespace MultiTenantBilling.Application.Commands
{
    /// <summary>
    /// Command for creating a new plan
    /// </summary>
    public class CreatePlanCommand : IRequest<PlanDto>
    {
        public Guid TenantId { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal MonthlyPrice { get; set; }
        public int MaxUsers { get; set; }
        public int MaxStorageGb { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
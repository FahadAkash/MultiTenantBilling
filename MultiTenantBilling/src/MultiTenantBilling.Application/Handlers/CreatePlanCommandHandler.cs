using MediatR;
using Microsoft.Extensions.Logging;
using MultiTenantBilling.Application.Commands;
using MultiTenantBilling.Application.DTOs;
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MultiTenantBilling.Application.Handlers
{
    /// <summary>
    /// Handler for creating a new plan
    /// </summary>
    public class CreatePlanCommandHandler : IRequestHandler<CreatePlanCommand, PlanDto>
    {
        private readonly ITenantRepository<Plan> _planRepository;
        private readonly ILogger<CreatePlanCommandHandler> _logger;

        public CreatePlanCommandHandler(
            ITenantRepository<Plan> planRepository,
            ILogger<CreatePlanCommandHandler> logger)
        {
            _planRepository = planRepository;
            _logger = logger;
        }

        public async Task<PlanDto> Handle(CreatePlanCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating plan {PlanName} for tenant {TenantId}", 
                request.Name, request.TenantId);

            // Create plan
            var plan = new Plan
            {
                TenantId = request.TenantId,
                Name = request.Name,
                Description = request.Description,
                MonthlyPrice = request.MonthlyPrice,
                MaxUsers = request.MaxUsers,
                MaxStorageGb = request.MaxStorageGb,
                IsActive = request.IsActive
            };

            var createdPlan = await _planRepository.AddAsync(plan);

            return new PlanDto
            {
                Id = createdPlan.Id,
                Name = createdPlan.Name,
                Description = createdPlan.Description,
                MonthlyPrice = createdPlan.MonthlyPrice,
                MaxUsers = createdPlan.MaxUsers,
                MaxStorageGb = createdPlan.MaxStorageGb
            };
        }
    }
}
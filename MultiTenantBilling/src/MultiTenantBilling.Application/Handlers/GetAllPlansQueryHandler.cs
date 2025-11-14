using MediatR;
using Microsoft.Extensions.Logging;
using MultiTenantBilling.Application.DTOs;
using MultiTenantBilling.Application.Queries;
using MultiTenantBilling.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MultiTenantBilling.Application.Handlers
{
    /// <summary>
    /// Handler for getting all plans for a tenant
    /// </summary>
    public class GetAllPlansQueryHandler : IRequestHandler<GetAllPlansQuery, IEnumerable<PlanDto>>
    {
        private readonly ITenantRepository<Domain.Entities.Plan> _planRepository;
        private readonly ILogger<GetAllPlansQueryHandler> _logger;

        public GetAllPlansQueryHandler(
            ITenantRepository<Domain.Entities.Plan> planRepository,
            ILogger<GetAllPlansQueryHandler> logger)
        {
            _planRepository = planRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<PlanDto>> Handle(GetAllPlansQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting all plans for tenant {TenantId}", request.TenantId);

            var plans = await _planRepository.GetByTenantIdAsync(request.TenantId);
            
            return plans.Select(plan => new PlanDto
            {
                Id = plan.Id,
                Name = plan.Name,
                Description = plan.Description,
                MonthlyPrice = plan.MonthlyPrice,
                MaxUsers = plan.MaxUsers,
                MaxStorageGb = plan.MaxStorageGb
            }).ToList();
        }
    }
}
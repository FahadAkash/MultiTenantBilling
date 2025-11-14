using MultiTenantBilling.Application.DTOs;
using System.Collections.Generic;

namespace MultiTenantBilling.Application.Queries
{
    /// <summary>
    /// Query to get all plans for a tenant
    /// </summary>
    public class GetAllPlansQuery : IQuery<IEnumerable<PlanDto>>
    {
        public System.Guid TenantId { get; set; }
    }
}
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Data;

namespace MultiTenantBilling.Infrastructure.Repositories
{
    public class PlanRepository : TenantRepositoryBase<Plan>
    {
        public PlanRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}


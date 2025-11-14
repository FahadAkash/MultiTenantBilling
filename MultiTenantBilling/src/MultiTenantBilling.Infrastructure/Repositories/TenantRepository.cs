using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Data;

namespace MultiTenantBilling.Infrastructure.Repositories
{
    public class TenantRepository : TenantRepositoryBase<Tenant>
    {
        public TenantRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}


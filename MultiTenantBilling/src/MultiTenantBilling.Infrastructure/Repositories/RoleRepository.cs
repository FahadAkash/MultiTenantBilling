using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Data;

namespace MultiTenantBilling.Infrastructure.Repositories
{
    public class RoleRepository : TenantRepositoryBase<Role>
    {
        public RoleRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
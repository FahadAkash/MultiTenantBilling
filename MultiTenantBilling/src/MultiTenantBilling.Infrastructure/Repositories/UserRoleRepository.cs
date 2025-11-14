using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Data;

namespace MultiTenantBilling.Infrastructure.Repositories
{
    public class UserRoleRepository : TenantRepositoryBase<UserRole>
    {
        public UserRoleRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
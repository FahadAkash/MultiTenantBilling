using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Data;

namespace MultiTenantBilling.Infrastructure.Repositories
{
    public class UserRepository : TenantRepositoryBase<User>
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
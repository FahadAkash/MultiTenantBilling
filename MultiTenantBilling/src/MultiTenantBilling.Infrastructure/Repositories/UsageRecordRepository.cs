using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Data;

namespace MultiTenantBilling.Infrastructure.Repositories
{
    public class UsageRecordRepository : TenantRepositoryBase<UsageRecord>
    {
        public UsageRecordRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}


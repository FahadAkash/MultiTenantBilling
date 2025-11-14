using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Data;
using System;

namespace MultiTenantBilling.Infrastructure.Repositories
{
    public class SubscriptionRepository : TenantRepositoryBase<Subscription>
    {
        public SubscriptionRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
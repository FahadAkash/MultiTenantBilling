using Microsoft.EntityFrameworkCore;
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiTenantBilling.Infrastructure.Repositories
{
    public class SubscriptionRepository : TenantRepositoryBase<Subscription>
    {
        public SubscriptionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public new async Task<Subscription> GetByIdAsync(Guid id)
        {
            // This method should not be used directly without tenant context
            throw new NotImplementedException("Use GetByIdForTenantAsync with tenant ID");
        }

        public new async Task<IEnumerable<Subscription>> GetAllAsync()
        {
            // This method should not be used directly without tenant context
            throw new NotImplementedException("Use GetAllForTenantAsync with tenant ID");
        }
    }
}
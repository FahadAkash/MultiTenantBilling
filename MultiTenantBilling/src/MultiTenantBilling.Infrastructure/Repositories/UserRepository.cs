using Microsoft.EntityFrameworkCore;
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiTenantBilling.Infrastructure.Repositories
{
    public class UserRepository : TenantRepositoryBase<User>
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public new Task<User> GetByIdAsync(Guid id)
        {
            // This method should not be used directly without tenant context
            throw new NotImplementedException("Use GetByIdForTenantAsync with tenant ID");
        }

        public new Task<IEnumerable<User>> GetAllAsync()
        {
            // This method should not be used directly without tenant context
            throw new NotImplementedException("Use GetAllForTenantAsync with tenant ID");
        }
    }
}
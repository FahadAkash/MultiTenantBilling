using Microsoft.EntityFrameworkCore;
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiTenantBilling.Infrastructure.Repositories
{
    public class InvoiceRepository : TenantRepositoryBase<Invoice>
    {
        public InvoiceRepository(ApplicationDbContext context) : base(context)
        {
        }

        public new async Task<Invoice> GetByIdAsync(Guid id)
        {
            // This method should not be used directly without tenant context
            throw new NotImplementedException("Use GetByIdForTenantAsync with tenant ID");
        }

        public new async Task<IEnumerable<Invoice>> GetAllAsync()
        {
            // This method should not be used directly without tenant context
            throw new NotImplementedException("Use GetAllForTenantAsync with tenant ID");
        }
    }
}
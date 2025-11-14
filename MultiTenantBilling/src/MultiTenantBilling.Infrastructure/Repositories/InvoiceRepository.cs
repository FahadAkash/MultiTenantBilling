using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Data;
using System;

namespace MultiTenantBilling.Infrastructure.Repositories
{
    public class InvoiceRepository : TenantRepositoryBase<Invoice>
    {
        public InvoiceRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
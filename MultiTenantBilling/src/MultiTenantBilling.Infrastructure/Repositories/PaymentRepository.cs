using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Data;

namespace MultiTenantBilling.Infrastructure.Repositories
{
    public class PaymentRepository : TenantRepositoryBase<Payment>
    {
        public PaymentRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}


using MultiTenantBilling.Domain.Common;
using MultiTenantBilling.Domain.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiTenantBilling.Infrastructure.Repositories
{
    public interface ITenantRepository<T> where T : BaseEntity, ITenantEntity
    {
        Task<T> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetByTenantIdAsync(Guid tenantId);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<bool> DeleteAsync(Guid id);
    }
}
using Microsoft.EntityFrameworkCore;
using MultiTenantBilling.Domain.Common;
using MultiTenantBilling.Domain.Interface;
using MultiTenantBilling.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiTenantBilling.Infrastructure.Repositories
{
    public abstract class TenantRepositoryBase<T> : ITenantRepository<T> where T : BaseEntity, ITenantEntity
    {
        protected readonly ApplicationDbContext _context;

        protected TenantRepositoryBase(ApplicationDbContext context)
        {
            _context = context;
        }

        public virtual Task<T> GetByIdAsync(Guid id)
        {
            // This method should be implemented by concrete classes to ensure tenant filtering
            throw new NotImplementedException("Use GetByIdForTenantAsync instead or implement tenant filtering in concrete class");
        }

        public async Task<T> GetByIdForTenantAsync(Guid id, Guid tenantId)
        {
            return await _context.Set<T>()
                .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId);
        }

        public virtual Task<IEnumerable<T>> GetAllAsync()
        {
            // This method should be implemented by concrete classes to ensure tenant filtering
            throw new NotImplementedException("Use GetAllForTenantAsync instead or implement tenant filtering in concrete class");
        }

        public async Task<IEnumerable<T>> GetAllForTenantAsync(Guid tenantId)
        {
            return await _context.Set<T>()
                .Where(e => e.TenantId == tenantId)
                .ToListAsync();
        }

        public async Task<IEnumerable<T>> GetByTenantIdAsync(Guid tenantId)
        {
            return await _context.Set<T>()
                .Where(e => e.TenantId == tenantId)
                .ToListAsync();
        }

        public async Task<T> AddAsync(T entity)
        {
            _context.Set<T>().Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity == null) return false;

            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
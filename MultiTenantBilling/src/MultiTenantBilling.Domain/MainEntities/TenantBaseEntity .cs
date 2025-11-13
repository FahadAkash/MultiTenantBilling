using MultiTenantBilling.Domain.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantBilling.Domain.MainEntities
{
    internal class TenantBaseEntity : BaseEntities , ITenantEntity, ISoftDeletable
    {
        public Guid TenantId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}

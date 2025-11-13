using MultiTenantBilling.Domain.Common;
using System.Collections.Generic;

namespace MultiTenantBilling.Domain.Entities
{
    public class Plan : BaseEntity
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal MonthlyPrice { get; set; }
        public int MaxUsers { get; set; }
        public int MaxStorageGb { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
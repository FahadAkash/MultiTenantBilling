using MultiTenantBilling.Domain.Common;
using System;

namespace MultiTenantBilling.Domain.MainEntities
{
    internal abstract class BaseEntities : BaseEntity
    {
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
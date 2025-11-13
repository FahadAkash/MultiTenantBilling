using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantBilling.Domain.Interface
{
    internal interface ITenantEntity
    {
        Guid TenantId { get; set; }
    }
}

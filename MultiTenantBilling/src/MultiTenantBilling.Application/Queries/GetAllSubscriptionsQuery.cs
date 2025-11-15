using MultiTenantBilling.Application.DTOs;
using System.Collections.Generic;

namespace MultiTenantBilling.Application.Queries
{
    /// <summary>
    /// Query to get all subscriptions for a tenant
    /// </summary>
    public class GetAllSubscriptionsQuery : IQuery<IEnumerable<SubscriptionDto>>
    {
        public Guid TenantId { get; set; }
    }
}
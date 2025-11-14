using MultiTenantBilling.Application.DTOs;

namespace MultiTenantBilling.Application.Queries
{
    /// <summary>
    /// Query to get a subscription by ID
    /// </summary>
    public class GetSubscriptionQuery : IQuery<SubscriptionDto?>
    {
        public Guid SubscriptionId { get; set; }
    }
}


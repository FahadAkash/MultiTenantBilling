namespace MultiTenantBilling.Application.Queries
{
    /// <summary>
    /// Query to get subscriptions that need billing (for scheduled job)
    /// </summary>
    public class GetUpcomingBillingQuery : IQuery<List<UpcomingBillingDto>>
    {
        public DateTime BillingDate { get; set; }
    }

    public class UpcomingBillingDto
    {
        public Guid TenantId { get; set; }
        public Guid SubscriptionId { get; set; }
        public Guid PlanId { get; set; }
        public DateTime NextBillingDate { get; set; }
        public string TenantName { get; set; } = default!;
    }
}


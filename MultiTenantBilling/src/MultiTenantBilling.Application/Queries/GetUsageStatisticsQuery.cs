namespace MultiTenantBilling.Application.Queries
{
    /// <summary>
    /// Query to get usage statistics for a subscription or tenant
    /// </summary>
    public class GetUsageStatisticsQuery : IQuery<UsageStatisticsDto>
    {
        public Guid? SubscriptionId { get; set; }
        public Guid? TenantId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class UsageStatisticsDto
    {
        public Dictionary<string, double> Metrics { get; set; } = new();
        public Dictionary<string, double> Limits { get; set; } = new();
        public Dictionary<string, double> Overages { get; set; } = new();
        public decimal EstimatedOverageCost { get; set; }
    }
}


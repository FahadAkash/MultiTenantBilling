namespace MultiTenantBilling.Application.Queries
{
    /// <summary>
    /// Query to get monthly recurring revenue (MRR) for a tenant or all tenants
    /// </summary>
    public class GetMonthlyRevenueQuery : IQuery<MonthlyRevenueDto>
    {
        public Guid? TenantId { get; set; }
        public DateTime? Month { get; set; }
    }

    public class MonthlyRevenueDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal RecurringRevenue { get; set; }
        public decimal OverageRevenue { get; set; }
        public int ActiveSubscriptions { get; set; }
        public DateTime Month { get; set; }
    }
}


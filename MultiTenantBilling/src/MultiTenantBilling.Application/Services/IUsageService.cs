using MultiTenantBilling.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace MultiTenantBilling.Application.Services
{
    public interface IUsageService
    {
        Task<UsageRecordDto> RecordUsageAsync(Guid subscriptionId, string metricName, double quantity);
        Task<double> GetTotalUsageAsync(Guid subscriptionId, string metricName, DateTime startDate, DateTime endDate);
    }
}
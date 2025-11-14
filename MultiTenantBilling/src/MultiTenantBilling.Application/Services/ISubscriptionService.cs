using MultiTenantBilling.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace MultiTenantBilling.Application.Services
{
    public interface ISubscriptionService
    {
        Task<SubscriptionDto> CreateSubscriptionAsync(Guid tenantId, Guid planId, DateTime startDate);
        Task<SubscriptionDto> GetSubscriptionAsync(Guid subscriptionId);
        Task<SubscriptionDto> UpdateSubscriptionAsync(Guid subscriptionId, Guid newPlanId);
        Task<bool> CancelSubscriptionAsync(Guid subscriptionId);
        Task<decimal> CalculateOverageAsync(Guid subscriptionId);
    }
}
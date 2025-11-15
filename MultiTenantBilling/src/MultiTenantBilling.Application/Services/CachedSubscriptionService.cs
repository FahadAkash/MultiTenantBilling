using MultiTenantBilling.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiTenantBilling.Application.Services
{
    public class CachedSubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly ICacheService _cacheService;
        private readonly ITenantService _tenantService;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(10);

        public CachedSubscriptionService(
            ISubscriptionService subscriptionService,
            ICacheService cacheService,
            ITenantService tenantService)
        {
            _subscriptionService = subscriptionService;
            _cacheService = cacheService;
            _tenantService = tenantService;
        }

        public async Task<SubscriptionDto> CreateSubscriptionAsync(Guid tenantId, Guid planId, DateTime startDate)
        {
            // Create the subscription using the underlying service
            var subscription = await _subscriptionService.CreateSubscriptionAsync(tenantId, planId, startDate);
            
            // Cache the subscription
            var cacheKey = $"subscription_{tenantId}_{subscription.Id}";
            await _cacheService.SetAsync(cacheKey, subscription, _cacheExpiration);
            
            return subscription;
        }

        public async Task<SubscriptionDto> GetSubscriptionAsync(Guid subscriptionId)
        {
            var tenantId = _tenantService.GetRequiredTenantId();
            var cacheKey = $"subscription_{tenantId}_{subscriptionId}";
            
            // Try to get from cache first
            var cachedSubscription = await _cacheService.GetAsync<SubscriptionDto>(cacheKey);
            if (cachedSubscription != null)
            {
                return cachedSubscription;
            }
            
            // If not in cache, get from the underlying service
            var subscription = await _subscriptionService.GetSubscriptionAsync(subscriptionId);
            
            // Cache the result
            await _cacheService.SetAsync(cacheKey, subscription, _cacheExpiration);
            
            return subscription;
        }

        public async Task<SubscriptionDto> UpdateSubscriptionAsync(Guid subscriptionId, Guid newPlanId)
        {
            // Update the subscription using the underlying service
            var subscription = await _subscriptionService.UpdateSubscriptionAsync(subscriptionId, newPlanId);
            
            // Update the cached subscription
            var tenantId = _tenantService.GetRequiredTenantId();
            var cacheKey = $"subscription_{tenantId}_{subscriptionId}";
            await _cacheService.SetAsync(cacheKey, subscription, _cacheExpiration);
            
            return subscription;
        }

        public async Task<bool> CancelSubscriptionAsync(Guid subscriptionId)
        {
            // Cancel the subscription using the underlying service
            var result = await _subscriptionService.CancelSubscriptionAsync(subscriptionId);
            
            if (result)
            {
                // Remove the cached subscription
                var tenantId = _tenantService.GetRequiredTenantId();
                var cacheKey = $"subscription_{tenantId}_{subscriptionId}";
                await _cacheService.RemoveAsync(cacheKey);
            }
            
            return result;
        }

        public async Task<decimal> CalculateOverageAsync(Guid subscriptionId)
        {
            // For this method, we won't cache the result as it's likely to change frequently
            return await _subscriptionService.CalculateOverageAsync(subscriptionId);
        }
    }
}
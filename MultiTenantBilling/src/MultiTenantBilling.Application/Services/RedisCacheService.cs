using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace MultiTenantBilling.Application.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public T Get<T>(string key)
        {
            var cachedValue = _cache.GetString(key);
            if (string.IsNullOrEmpty(cachedValue))
                return default(T);

            return JsonSerializer.Deserialize<T>(cachedValue);
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var cachedValue = await _cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(cachedValue))
                return default(T);

            return JsonSerializer.Deserialize<T>(cachedValue);
        }

        public void Set<T>(string key, T value, DateTimeOffset absoluteExpiration)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = absoluteExpiration
            };

            var serializedValue = JsonSerializer.Serialize(value);
            _cache.SetString(key, serializedValue, options);
        }

        public async Task SetAsync<T>(string key, T value, DateTimeOffset absoluteExpiration)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = absoluteExpiration
            };

            var serializedValue = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, serializedValue, options);
        }

        public void Set<T>(string key, T value, TimeSpan slidingExpiration)
        {
            var options = new DistributedCacheEntryOptions
            {
                SlidingExpiration = slidingExpiration
            };

            var serializedValue = JsonSerializer.Serialize(value);
            _cache.SetString(key, serializedValue, options);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan slidingExpiration)
        {
            var options = new DistributedCacheEntryOptions
            {
                SlidingExpiration = slidingExpiration
            };

            var serializedValue = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, serializedValue, options);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }
    }
}
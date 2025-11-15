using System;
using System.Threading.Tasks;

namespace MultiTenantBilling.Application.Services
{
    public interface ICacheService
    {
        /// <summary>
        /// Gets a cached item by key
        /// </summary>
        /// <typeparam name="T">Type of the cached item</typeparam>
        /// <param name="key">Cache key</param>
        /// <returns>Cached item or default value if not found</returns>
        T Get<T>(string key);

        /// <summary>
        /// Gets a cached item by key asynchronously
        /// </summary>
        /// <typeparam name="T">Type of the cached item</typeparam>
        /// <param name="key">Cache key</param>
        /// <returns>Cached item or default value if not found</returns>
        Task<T> GetAsync<T>(string key);

        /// <summary>
        /// Sets a cached item with absolute expiration
        /// </summary>
        /// <typeparam name="T">Type of the cached item</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="value">Value to cache</param>
        /// <param name="absoluteExpiration">Absolute expiration time</param>
        void Set<T>(string key, T value, DateTimeOffset absoluteExpiration);

        /// <summary>
        /// Sets a cached item with absolute expiration asynchronously
        /// </summary>
        /// <typeparam name="T">Type of the cached item</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="value">Value to cache</param>
        /// <param name="absoluteExpiration">Absolute expiration time</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task SetAsync<T>(string key, T value, DateTimeOffset absoluteExpiration);

        /// <summary>
        /// Sets a cached item with sliding expiration
        /// </summary>
        /// <typeparam name="T">Type of the cached item</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="value">Value to cache</param>
        /// <param name="slidingExpiration">Sliding expiration time</param>
        void Set<T>(string key, T value, TimeSpan slidingExpiration);

        /// <summary>
        /// Sets a cached item with sliding expiration asynchronously
        /// </summary>
        /// <typeparam name="T">Type of the cached item</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="value">Value to cache</param>
        /// <param name="slidingExpiration">Sliding expiration time</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task SetAsync<T>(string key, T value, TimeSpan slidingExpiration);

        /// <summary>
        /// Removes a cached item by key
        /// </summary>
        /// <param name="key">Cache key</param>
        void Remove(string key);

        /// <summary>
        /// Removes a cached item by key asynchronously
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task RemoveAsync(string key);
    }
}
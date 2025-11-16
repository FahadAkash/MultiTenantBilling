using Microsoft.Extensions.Logging;
using MultiTenantBilling.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace MultiTenantBilling.Application.Services
{
    /// <summary>
    /// Service for managing user session caching with Redis
    /// </summary>
    public class UserSessionCacheService
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<UserSessionCacheService> _logger;
        private readonly TimeSpan _sessionCacheExpiration = TimeSpan.FromMinutes(30); // Default 30 minutes
        private readonly TimeSpan _loginAttemptCacheExpiration = TimeSpan.FromMinutes(15); // 15 minutes for login attempts

        public UserSessionCacheService(ICacheService cacheService, ILogger<UserSessionCacheService> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        /// <summary>
        /// Gets cached user session data by email
        /// </summary>
        /// <param name="email">User email</param>
        /// <returns>Cached UserDto or null if not found</returns>
        public async Task<UserDto?> GetUserSessionAsync(string email)
        {
            var cacheKey = GenerateUserSessionKey(email);
            var userDto = await _cacheService.GetAsync<UserDto>(cacheKey);
            
            if (userDto != null)
            {
                _logger.LogInformation("Found user session in cache for email {Email}", email);
            }
            else
            {
                _logger.LogInformation("User session not found in cache for email {Email}", email);
            }
            
            return userDto;
        }

        /// <summary>
        /// Caches user session data
        /// </summary>
        /// <param name="userDto">User data to cache</param>
        public async Task CacheUserSessionAsync(UserDto userDto)
        {
            var cacheKey = GenerateUserSessionKey(userDto.Email);
            await _cacheService.SetAsync(cacheKey, userDto, _sessionCacheExpiration);
            _logger.LogInformation("Cached user session for email {Email} with {Minutes} minute expiration", 
                userDto.Email, _sessionCacheExpiration.TotalMinutes);
        }

        /// <summary>
        /// Gets cached user data for login attempts
        /// </summary>
        /// <param name="email">User email</param>
        /// <returns>Cached UserDto or null if not found</returns>
        public async Task<UserDto?> GetLoginAttemptAsync(string email)
        {
            var cacheKey = GenerateLoginAttemptKey(email);
            var userDto = await _cacheService.GetAsync<UserDto>(cacheKey);
            
            if (userDto != null)
            {
                _logger.LogInformation("Found login attempt in cache for email {Email}", email);
            }
            else
            {
                _logger.LogInformation("Login attempt not found in cache for email {Email}", email);
            }
            
            return userDto;
        }

        /// <summary>
        /// Caches user data for login attempts
        /// </summary>
        /// <param name="userDto">User data to cache</param>
        public async Task CacheLoginAttemptAsync(UserDto userDto)
        {
            var cacheKey = GenerateLoginAttemptKey(userDto.Email);
            await _cacheService.SetAsync(cacheKey, userDto, _loginAttemptCacheExpiration);
            _logger.LogInformation("Cached login attempt for email {Email} with {Minutes} minute expiration", 
                userDto.Email, _loginAttemptCacheExpiration.TotalMinutes);
        }

        /// <summary>
        /// Invalidates user session cache
        /// </summary>
        /// <param name="email">User email</param>
        public async Task InvalidateUserSessionAsync(string email)
        {
            var sessionCacheKey = GenerateUserSessionKey(email);
            var loginCacheKey = GenerateLoginAttemptKey(email);
            
            await _cacheService.RemoveAsync(sessionCacheKey);
            await _cacheService.RemoveAsync(loginCacheKey);
            
            _logger.LogInformation("Invalidated cache for user {Email}", email);
        }

        /// <summary>
        /// Invalidates all user-related caches
        /// </summary>
        /// <param name="email">User email</param>
        public async Task InvalidateAllUserCachesAsync(string email)
        {
            await InvalidateUserSessionAsync(email);
            _logger.LogInformation("Invalidated all caches for user {Email}", email);
        }

        /// <summary>
        /// Generates a cache key for user sessions
        /// </summary>
        /// <param name="email">User email</param>
        /// <returns>Cache key</returns>
        private string GenerateUserSessionKey(string email)
        {
            return $"user_session_{email.ToLower()}";
        }

        /// <summary>
        /// Generates a cache key for login attempts
        /// </summary>
        /// <param name="email">User email</param>
        /// <returns>Cache key</returns>
        private string GenerateLoginAttemptKey(string email)
        {
            return $"login_attempt_{email.ToLower()}";
        }
    }
}
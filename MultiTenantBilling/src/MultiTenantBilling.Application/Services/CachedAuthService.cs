using Microsoft.Extensions.Logging;
using MultiTenantBilling.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace MultiTenantBilling.Application.Services
{
    public class CachedAuthService : ICachedAuthService
    {
        private readonly IAuthService _authService;
        private readonly UserSessionCacheService _userSessionCacheService;
        private readonly ILogger<CachedAuthService> _logger;

        public CachedAuthService(
            IAuthService authService,
            UserSessionCacheService userSessionCacheService,
            ILogger<CachedAuthService> logger)
        {
            _authService = authService;
            _userSessionCacheService = userSessionCacheService;
            _logger = logger;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            _logger.LogInformation("User login attempt: {Email}", loginDto.Email);

            try
            {
                // Try to get cached user data for login attempt first
                var cachedUserDto = await _userSessionCacheService.GetLoginAttemptAsync(loginDto.Email);
                if (cachedUserDto != null)
                {
                    _logger.LogInformation("Found user {Email} in login attempt cache", loginDto.Email);
                    
                    // Generate a new token for the cached user
                    // Note: We need to get the JwtService to generate a new token
                    // For now, we'll delegate to the original service but cache the result
                    var result = await _authService.LoginAsync(loginDto);
                    
                    // Cache the result for future login attempts
                    await _userSessionCacheService.CacheLoginAttemptAsync(result.User);
                    _logger.LogInformation("Cached user data for {Email} login attempts", loginDto.Email);
                    
                    return result;
                }

                // Perform the actual login
                var authResult = await _authService.LoginAsync(loginDto);
                
                // Cache the user data for future login attempts
                await _userSessionCacheService.CacheLoginAttemptAsync(authResult.User);
                _logger.LogInformation("Cached user data for {Email} login attempts", loginDto.Email);
                
                return authResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Email}", loginDto.Email);
                throw;
            }
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var result = await _authService.RegisterAsync(registerDto);
            
            // Cache the user session
            await _userSessionCacheService.CacheUserSessionAsync(result.User);
            
            return result;
        }

        public async Task<AuthResponseDto> AdminRegisterAsync(AdminRegisterDto adminRegisterDto)
        {
            var result = await _authService.AdminRegisterAsync(adminRegisterDto);
            
            // Cache the user session
            await _userSessionCacheService.CacheUserSessionAsync(result.User);
            
            return result;
        }

        public async Task<bool> ChangePasswordAsync(string email, string currentPassword, string newPassword)
        {
            var result = await _authService.ChangePasswordAsync(email, currentPassword, newPassword);
            
            // Invalidate cache for this user
            if (result)
            {
                await _userSessionCacheService.InvalidateAllUserCachesAsync(email);
                _logger.LogInformation("Invalidated cache for user {Email} after password change", email);
            }
            
            return result;
        }

        public async Task<bool> AssignRoleAsync(string userEmail, string roleName)
        {
            var result = await _authService.AssignRoleAsync(userEmail, roleName);
            
            // Invalidate cache for this user
            if (result)
            {
                await _userSessionCacheService.InvalidateAllUserCachesAsync(userEmail);
                _logger.LogInformation("Invalidated cache for user {UserEmail} after role assignment", userEmail);
            }
            
            return result;
        }

        public async Task<bool> RemoveRoleAsync(string userEmail, string roleName)
        {
            var result = await _authService.RemoveRoleAsync(userEmail, roleName);
            
            // Invalidate cache for this user
            if (result)
            {
                await _userSessionCacheService.InvalidateAllUserCachesAsync(userEmail);
                _logger.LogInformation("Invalidated cache for user {UserEmail} after role removal", userEmail);
            }
            
            return result;
        }

        public async Task<bool> ActivateUserAsync(Guid userId)
        {
            var result = await _authService.ActivateUserAsync(userId);
            
            // Note: We don't have the user email here to invalidate the cache
            // In a production implementation, we would need to get the user email
            
            return result;
        }

        public async Task<bool> DeactivateUserAsync(Guid userId)
        {
            var result = await _authService.DeactivateUserAsync(userId);
            
            // Note: We don't have the user email here to invalidate the cache
            // In a production implementation, we would need to get the user email
            
            return result;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            return await _authService.GetAllUsersAsync();
        }
    }
}
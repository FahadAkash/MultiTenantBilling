using Microsoft.Extensions.Logging;
using MultiTenantBilling.Application.DTOs;
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Repositories;
using System;
using System.Threading.Tasks;

namespace MultiTenantBilling.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly ITenantRepository<User> _userRepository;
        private readonly ITenantRepository<Role> _roleRepository;
        private readonly ITenantRepository<UserRole> _userRoleRepository;
        private readonly ITenantService _tenantService; // Use ITenantService from Application layer
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ITenantRepository<User> userRepository,
            ITenantRepository<Role> roleRepository,
            ITenantRepository<UserRole> userRoleRepository,
            ITenantService tenantService, // Use ITenantService from Application layer
            JwtService jwtService,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _tenantService = tenantService;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            _logger.LogInformation("Registering new user: {Email}", registerDto.Email);

            var tenantId = _tenantService.GetRequiredTenantId();

            // Check if user already exists
            var existingUser = await GetUserByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists");
            }

            // Create new user
            var user = new User
            {
                TenantId = tenantId,
                Email = registerDto.Email,
                PasswordHash = HashPassword(registerDto.Password), // In a real implementation, use a proper password hashing library
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                IsActive = true
            };

            var createdUser = await _userRepository.AddAsync(user);

            // Assign default role
            var defaultRole = await GetRoleByNameAsync("User");
            if (defaultRole != null)
            {
                var userRole = new UserRole
                {
                    TenantId = tenantId,
                    UserId = createdUser.Id,
                    RoleId = defaultRole.Id
                };
                await _userRoleRepository.AddAsync(userRole);
            }

            var userDto = new UserDto
            {
                Id = createdUser.Id,
                Email = createdUser.Email,
                FirstName = createdUser.FirstName,
                LastName = createdUser.LastName,
                IsActive = createdUser.IsActive,
                Roles = new[] { "User" }
            };

            return new AuthResponseDto
            {
                Token = _jwtService.GenerateToken(userDto, tenantId),
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = userDto
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            _logger.LogInformation("User login attempt: {Email}", loginDto.Email);

            var tenantId = _tenantService.GetRequiredTenantId();

            // Find user by email
            var user = await GetUserByEmailAsync(loginDto.Email);
            if (user == null)
            {
                throw new InvalidOperationException("Invalid email or password");
            }

            // Verify password
            if (!VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                throw new InvalidOperationException("Invalid email or password");
            }

            // Update last login timestamp
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            // Get user roles
            var roles = await GetUserRolesAsync(user.Id);

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                Roles = roles
            };

            return new AuthResponseDto
            {
                Token = _jwtService.GenerateToken(userDto, tenantId),
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = userDto
            };
        }

        public async Task<bool> ChangePasswordAsync(string email, string currentPassword, string newPassword)
        {
            _logger.LogInformation("Changing password for user: {Email}", email);

            var tenantId = _tenantService.GetRequiredTenantId();

            // Find user by email
            var user = await GetUserByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            // Verify current password
            if (!VerifyPassword(currentPassword, user.PasswordHash))
            {
                return false;
            }

            // Update password
            user.PasswordHash = HashPassword(newPassword);
            await _userRepository.UpdateAsync(user);

            return true;
        }

        public async Task<bool> AssignRoleAsync(string userEmail, string roleName)
        {
            _logger.LogInformation("Assigning role {RoleName} to user {UserEmail}", roleName, userEmail);

            var tenantId = _tenantService.GetRequiredTenantId();

            // Find user by email
            var user = await GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                return false;
            }

            // Find role by name
            var role = await GetRoleByNameAsync(roleName);
            if (role == null)
            {
                return false;
            }

            // Check if user already has this role
            var existingUserRole = await GetUserRoleAsync(user.Id, role.Id);
            if (existingUserRole != null)
            {
                return true; // User already has this role
            }

            // Create user-role association
            var userRole = new UserRole
            {
                TenantId = tenantId,
                UserId = user.Id,
                RoleId = role.Id
            };

            await _userRoleRepository.AddAsync(userRole);
            return true;
        }

        public async Task<bool> RemoveRoleAsync(string userEmail, string roleName)
        {
            _logger.LogInformation("Removing role {RoleName} from user {UserEmail}", roleName, userEmail);

            // Find user by email
            var user = await GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                return false;
            }

            // Find role by name
            var role = await GetRoleByNameAsync(roleName);
            if (role == null)
            {
                return false;
            }

            // Find user-role association
            var userRole = await GetUserRoleAsync(user.Id, role.Id);
            if (userRole == null)
            {
                return true; // User doesn't have this role
            }

            // Remove user-role association
            await _userRoleRepository.DeleteAsync(userRole.Id);
            return true;
        }

        #region Helper Methods

        private async Task<User> GetUserByEmailAsync(string email)
        {
            // Get the current tenant ID
            var tenantId = _tenantService.GetRequiredTenantId();
            
            // Query the database for the user with the specified email and tenant ID
            // We need to filter by both email and tenant ID for multi-tenancy
            var users = await _userRepository.GetByTenantIdAsync(tenantId);
            return users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<Role> GetRoleByNameAsync(string roleName)
        {
            // Get the current tenant ID
            var tenantId = _tenantService.GetRequiredTenantId();
            
            // Query the database for the role with the specified name and tenant ID
            var roles = await _roleRepository.GetByTenantIdAsync(tenantId);
            return roles.FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<UserRole> GetUserRoleAsync(Guid userId, Guid roleId)
        {
            // Get the current tenant ID
            var tenantId = _tenantService.GetRequiredTenantId();
            
            // Query the database for the user-role association with the specified user ID, role ID, and tenant ID
            var userRoles = await _userRoleRepository.GetByTenantIdAsync(tenantId);
            return userRoles.FirstOrDefault(ur => ur.UserId == userId && ur.RoleId == roleId);
        }

        private async Task<string[]> GetUserRolesAsync(Guid userId)
        {
            // Get the current tenant ID
            var tenantId = _tenantService.GetRequiredTenantId();
            
            // Query the database for the user roles with the specified user ID and tenant ID
            var userRoles = await _userRoleRepository.GetByTenantIdAsync(tenantId);
            var userSpecificRoles = userRoles.Where(ur => ur.UserId == userId);
            
            // Get the role names for these user-role associations
            var roles = await _roleRepository.GetByTenantIdAsync(tenantId);
            var roleNames = userSpecificRoles
                .Join(roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .ToArray();
                
            return roleNames;
        }

        private string HashPassword(string password)
        {
            // In a real implementation, use a proper password hashing library like BCrypt or ASP.NET Core Identity
            // For demonstration purposes, we'll just return a simple hash
            // DO NOT use this in production!
            return $"HASHED_{password}";
        }

        private bool VerifyPassword(string password, string hash)
        {
            // In a real implementation, use a proper password hashing library
            // For demonstration purposes, we'll just compare with our simple hash
            return hash == $"HASHED_{password}";
        }

        #endregion
    }
}
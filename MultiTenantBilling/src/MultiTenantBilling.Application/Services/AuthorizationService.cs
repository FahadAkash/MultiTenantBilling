using Microsoft.Extensions.Logging;
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiTenantBilling.Application.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly ITenantRepository<User> _userRepository;
        private readonly ITenantRepository<Role> _roleRepository;
        private readonly ITenantRepository<UserRole> _userRoleRepository;
        private readonly ITenantService _tenantService; // Use ITenantService from Application layer
        private readonly ILogger<AuthorizationService> _logger;

        public AuthorizationService(
            ITenantRepository<User> userRepository,
            ITenantRepository<Role> roleRepository,
            ITenantRepository<UserRole> userRoleRepository,
            ITenantService tenantService, // Use ITenantService from Application layer
            ILogger<AuthorizationService> logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _tenantService = tenantService;
            _logger = logger;
        }

        public async Task<bool> HasPermissionAsync(string userEmail, string permission)
        {
            _logger.LogInformation("Checking permission {Permission} for user {UserEmail}", permission, userEmail);

            // Find user by email
            var user = await GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                return false;
            }

            // Get user roles
            var roles = await GetUserRolesAsync(user.Id);

            // Check if any role has the specified permission
            // In a real implementation, you would have a permissions table and check against that
            // For now, we'll use a simple mapping
            foreach (var role in roles)
            {
                if (RoleHasPermission(role, permission))
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> HasRoleAsync(string userEmail, string roleName)
        {
            _logger.LogInformation("Checking role {RoleName} for user {UserEmail}", roleName, userEmail);

            // Find user by email
            var user = await GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                return false;
            }

            // Get user roles
            var roles = await GetUserRolesAsync(user.Id);

            // Check if user has the specified role
            return roles.Contains(roleName);
        }

        public async Task<IEnumerable<string>> GetUserPermissionsAsync(string userEmail)
        {
            _logger.LogInformation("Getting permissions for user {UserEmail}", userEmail);

            // Find user by email
            var user = await GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                return new string[0];
            }

            // Get user roles
            var roles = await GetUserRolesAsync(user.Id);

            // Get permissions for each role
            var permissions = new List<string>();
            foreach (var role in roles)
            {
                permissions.AddRange(GetRolePermissions(role));
            }

            return permissions.Distinct();
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(string userEmail)
        {
            _logger.LogInformation("Getting roles for user {UserEmail}", userEmail);

            // Find user by email
            var user = await GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                return new string[0];
            }

            // Get user roles
            return await GetUserRolesAsync(user.Id);
        }

        public async Task<bool> AssignRoleAsync(string userEmail, string roleName)
        {
            _logger.LogInformation("Assigning role {RoleName} to user {UserEmail}", roleName, userEmail);

            // Find user by email
            var user = await GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                return false;
            }

            // Find or create role by name
            var role = await GetOrCreateRoleAsync(roleName, $"{roleName} role", user.TenantId);
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
                TenantId = user.TenantId,
                UserId = user.Id,
                RoleId = role.Id
            };

            await _userRoleRepository.AddAsync(userRole);
            return true;
        }

        #region Helper Methods

        private async Task<User?> GetUserByEmailAsync(string email)
        {
            // Get the current tenant ID
            var tenantId = _tenantService.GetRequiredTenantId();
            
            // Query the database for the user with the specified email and tenant ID
            var users = await _userRepository.GetByTenantIdAsync(tenantId);
            return users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
        {
            // Get the current tenant ID
            var tenantId = _tenantService.GetRequiredTenantId();
            
            // Query the database for the user with the specified email and tenant ID
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

        private async Task<Role> GetOrCreateRoleAsync(string roleName, string description, Guid tenantId)
        {
            // First, try to find the role in the database
            var existingRole = await GetRoleByNameAsync(roleName);
            if (existingRole != null)
            {
                return existingRole;
            }

            // Role doesn't exist, create it
            var newRole = new Role
            {
                TenantId = tenantId,
                Name = roleName,
                Description = description
            };

            // Save the role to the database
            var createdRole = await _roleRepository.AddAsync(newRole);
            return createdRole;
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

        private bool RoleHasPermission(string roleName, string permission)
        {
            // In a real implementation, you would have a permissions table
            // For now, we'll use a simple mapping
            return (roleName, permission) switch
            {
                ("Admin", _) => true,
                ("User", "view_billing") => true,
                ("BillingManager", "view_billing") => true,
                ("BillingManager", "manage_subscriptions") => true,
                ("BillingManager", "process_payments") => true,
                _ => false
            };
        }

        private IEnumerable<string> GetRolePermissions(string roleName)
        {
            // In a real implementation, you would query a permissions table
            // For now, we'll use a simple mapping
            return roleName switch
            {
                "Admin" => new[] { "view_billing", "manage_subscriptions", "process_payments", "manage_users" },
                "User" => new[] { "view_billing" },
                "BillingManager" => new[] { "view_billing", "manage_subscriptions", "process_payments" },
                _ => new string[0]
            };
        }

        #endregion
    }
}
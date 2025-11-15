using Microsoft.Extensions.Logging;
using MultiTenantBilling.Application.DTOs;
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MultiTenantBilling.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly ITenantRepository<User> _userRepository;
        private readonly ITenantRepository<Role> _roleRepository;
        private readonly ITenantRepository<UserRole> _userRoleRepository;
        private readonly ITenantRepository<Tenant> _tenantRepository;
        private readonly ITenantRepository<Plan> _planRepository; // Add Plan repository
        private readonly ITenantService _tenantService; // Use ITenantService from Application layer
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ITenantRepository<User> userRepository,
            ITenantRepository<Role> roleRepository,
            ITenantRepository<UserRole> userRoleRepository,
            ITenantRepository<Tenant> tenantRepository,
            ITenantRepository<Plan> planRepository, // Add Plan repository parameter
            ITenantService tenantService, // Use ITenantService from Application layer
            JwtService jwtService,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _tenantRepository = tenantRepository;
            _planRepository = planRepository; // Initialize Plan repository
            _tenantService = tenantService;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            _logger.LogInformation("Registering new user: {Email}", registerDto.Email);

            try
            {
                // For registration, we need to create a new tenant for the user
                var tenantId = await CreateTenantForUserAsync(registerDto);
                _logger.LogInformation("Created tenant {TenantId} for user {Email}", tenantId, registerDto.Email);

                // Set the tenant ID in the tenant service for the current request context
                _tenantService.SetTenantId(tenantId);

                // Check if user already exists
                var existingUser = await GetUserByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("User with email {Email} already exists", registerDto.Email);
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
                _logger.LogInformation("Created user {UserId} for tenant {TenantId}", createdUser.Id, tenantId);

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
                    _logger.LogInformation("Assigned User role to user {UserId}", createdUser.Id);
                }

                // Create default plans for the new tenant
                _logger.LogInformation("Creating default plans for tenant {TenantId}", tenantId);
                await CreateDefaultPlansForTenantAsync(tenantId);
                _logger.LogInformation("Default plans created for tenant {TenantId}", tenantId);

                var userDto = new UserDto
                {
                    Id = createdUser.Id,
                    Email = createdUser.Email,
                    FirstName = createdUser.FirstName,
                    LastName = createdUser.LastName,
                    IsActive = createdUser.IsActive,
                    Roles = new[] { "User" }
                };

                var token = _jwtService.GenerateToken(userDto, tenantId);
                _logger.LogInformation("Generated JWT token for user {UserId} in tenant {TenantId}", createdUser.Id, tenantId);

                return new AuthResponseDto
                {
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    User = userDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user {Email}", registerDto.Email);
                throw;
            }
        }

        public async Task<AuthResponseDto> AdminRegisterAsync(AdminRegisterDto adminRegisterDto)
        {
            _logger.LogInformation("Admin registering new user: {Email} for tenant {TenantId}", adminRegisterDto.Email, adminRegisterDto.TenantId);

            try
            {
                // Set the tenant ID in the tenant service for the current request context
                _tenantService.SetTenantId(adminRegisterDto.TenantId);

                // Check if user already exists
                var existingUser = await GetUserByEmailAsync(adminRegisterDto.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("User with email {Email} already exists", adminRegisterDto.Email);
                    throw new InvalidOperationException("User with this email already exists");
                }

                // Create new user
                var user = new User
                {
                    TenantId = adminRegisterDto.TenantId,
                    Email = adminRegisterDto.Email,
                    PasswordHash = HashPassword(adminRegisterDto.Password), // In a real implementation, use a proper password hashing library
                    FirstName = adminRegisterDto.FirstName,
                    LastName = adminRegisterDto.LastName,
                    IsActive = true
                };

                var createdUser = await _userRepository.AddAsync(user);
                _logger.LogInformation("Created user {UserId} for tenant {TenantId}", createdUser.Id, adminRegisterDto.TenantId);

                // Assign roles specified by admin
                foreach (var roleName in adminRegisterDto.Roles)
                {
                    var role = await GetRoleByNameAsync(roleName);
                    if (role != null)
                    {
                        var userRole = new UserRole
                        {
                            TenantId = adminRegisterDto.TenantId,
                            UserId = createdUser.Id,
                            RoleId = role.Id
                        };
                        await _userRoleRepository.AddAsync(userRole);
                        _logger.LogInformation("Assigned {RoleName} role to user {UserId}", roleName, createdUser.Id);
                    }
                }

                // If no roles were specified, assign default "User" role
                if (!adminRegisterDto.Roles.Any())
                {
                    var defaultRole = await GetRoleByNameAsync("User");
                    if (defaultRole != null)
                    {
                        var userRole = new UserRole
                        {
                            TenantId = adminRegisterDto.TenantId,
                            UserId = createdUser.Id,
                            RoleId = defaultRole.Id
                        };
                        await _userRoleRepository.AddAsync(userRole);
                        _logger.LogInformation("Assigned default User role to user {UserId}", createdUser.Id);
                    }
                }

                var userRoles = await GetUserRolesAsyncForTenant(createdUser.Id, adminRegisterDto.TenantId);

                var userDto = new UserDto
                {
                    Id = createdUser.Id,
                    Email = createdUser.Email,
                    FirstName = createdUser.FirstName,
                    LastName = createdUser.LastName,
                    IsActive = createdUser.IsActive,
                    Roles = userRoles
                };

                var token = _jwtService.GenerateToken(userDto, adminRegisterDto.TenantId);
                _logger.LogInformation("Generated JWT token for user {UserId} in tenant {TenantId}", createdUser.Id, adminRegisterDto.TenantId);

                return new AuthResponseDto
                {
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    User = userDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user {Email}", adminRegisterDto.Email);
                throw;
            }
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            _logger.LogInformation("User login attempt: {Email}", loginDto.Email);

            try
            {
                // For login, we need to find the user first and then get their tenant ID
                // We can't use _tenantService.GetRequiredTenantId() here because the tenant ID
                // is part of what we're trying to authenticate
                
                // Find user by email (we'll need to search across all tenants for this specific case)
                var user = await FindUserByEmailAcrossTenantsAsync(loginDto.Email);
                if (user == null)
                {
                    _logger.LogWarning("User with email {Email} not found", loginDto.Email);
                    throw new InvalidOperationException("Invalid email or password");
                }

                // Verify password
                if (!VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Invalid password for user {Email}", loginDto.Email);
                    throw new InvalidOperationException("Invalid email or password");
                }

                // Get the tenant ID from the user record
                var tenantId = user.TenantId;
                _logger.LogInformation("Found user {UserId} in tenant {TenantId} for email {Email}", user.Id, tenantId, loginDto.Email);

                // Update last login timestamp
                user.LastLoginAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
                _logger.LogInformation("Updated last login timestamp for user {UserId}", user.Id);

                // Get user roles
                var roles = await GetUserRolesAsyncForTenant(user.Id, tenantId);
                _logger.LogInformation("Retrieved {RoleCount} roles for user {UserId}", roles.Length, user.Id);

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    Roles = roles
                };

                var token = _jwtService.GenerateToken(userDto, tenantId);
                _logger.LogInformation("Generated JWT token for user {UserId} in tenant {TenantId}", user.Id, tenantId);

                return new AuthResponseDto
                {
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    User = userDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Email}", loginDto.Email);
                throw;
            }
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

        private async Task<Guid> CreateTenantForUserAsync(RegisterDto registerDto)
        {
            // Generate a tenant name based on the user's name
            var tenantName = $"{registerDto.FirstName} {registerDto.LastName}'s Organization";
            
            // Generate a subdomain based on the user's email
            var emailParts = registerDto.Email.Split('@');
            var subdomain = emailParts[0].ToLower().Replace(".", "-");
            
            // Create a new tenant
            var tenant = new Tenant
            {
                TenantId = Guid.NewGuid(),
                Name = tenantName,
                Subdomain = subdomain,
                Email = registerDto.Email,
                Status = "Active"
            };
            
            var createdTenant = await _tenantRepository.AddAsync(tenant);
            return createdTenant.Id;
        }

        // Create default plans for a new tenant
        private async Task CreateDefaultPlansForTenantAsync(Guid tenantId)
        {
            _logger.LogInformation("Creating default plans for tenant {TenantId}", tenantId);

            try
            {
                // Create a basic plan
                var basicPlan = new Plan
                {
                    TenantId = tenantId,
                    Name = "Basic Plan",
                    Description = "Basic plan with limited features",
                    MonthlyPrice = 29.99m,
                    MaxUsers = 5,
                    MaxStorageGb = 100,
                    IsActive = true
                };
                var savedBasicPlan = await _planRepository.AddAsync(basicPlan);
                _logger.LogInformation("Basic plan created with ID {PlanId} for tenant {TenantId}", savedBasicPlan.Id, tenantId);

                // Create a professional plan
                var professionalPlan = new Plan
                {
                    TenantId = tenantId,
                    Name = "Professional Plan",
                    Description = "Professional plan with advanced features",
                    MonthlyPrice = 99.99m,
                    MaxUsers = 20,
                    MaxStorageGb = 500,
                    IsActive = true
                };
                var savedProfessionalPlan = await _planRepository.AddAsync(professionalPlan);
                _logger.LogInformation("Professional plan created with ID {PlanId} for tenant {TenantId}", savedProfessionalPlan.Id, tenantId);

                // Create an enterprise plan
                var enterprisePlan = new Plan
                {
                    TenantId = tenantId,
                    Name = "Enterprise Plan",
                    Description = "Enterprise plan with all features",
                    MonthlyPrice = 299.99m,
                    MaxUsers = 100,
                    MaxStorageGb = 2000,
                    IsActive = true
                };
                var savedEnterprisePlan = await _planRepository.AddAsync(enterprisePlan);
                _logger.LogInformation("Enterprise plan created with ID {PlanId} for tenant {TenantId}", savedEnterprisePlan.Id, tenantId);

                _logger.LogInformation("Default plans created for tenant {TenantId}", tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating default plans for tenant {TenantId}", tenantId);
                throw;
            }
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

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            _logger.LogInformation("Getting all users");

            // Get the current tenant ID
            var tenantId = _tenantService.GetRequiredTenantId();

            // Get all users for this tenant
            var users = await _userRepository.GetByTenantIdAsync(tenantId);

            // Convert to DTOs
            var userDtos = users.Select(user => new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                Roles = new string[0] // We'll populate roles separately if needed
            }).ToList();

            return userDtos;
        }

        public async Task<bool> ActivateUserAsync(Guid userId)
        {
            _logger.LogInformation("Activating user {UserId}", userId);

            // Get the current tenant ID
            var tenantId = _tenantService.GetRequiredTenantId();

            // Find user by ID
            var user = await _userRepository.GetByIdForTenantAsync(userId, tenantId);
            if (user == null)
            {
                return false;
            }

            // Activate user
            user.IsActive = true;
            await _userRepository.UpdateAsync(user);

            return true;
        }

        public async Task<bool> DeactivateUserAsync(Guid userId)
        {
            _logger.LogInformation("Deactivating user {UserId}", userId);

            // Get the current tenant ID
            var tenantId = _tenantService.GetRequiredTenantId();

            // Find user by ID
            var user = await _userRepository.GetByIdForTenantAsync(userId, tenantId);
            if (user == null)
            {
                return false;
            }

            // Deactivate user
            user.IsActive = false;
            await _userRepository.UpdateAsync(user);

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

        private async Task<User> FindUserByEmailAcrossTenantsAsync(string email)
        {
            // For login purposes, we need to search across all tenants
            // This is a special case where we don't have tenant context yet
            var allUsers = await _userRepository.GetAllEntitiesAsync();
            return allUsers.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
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

        private async Task<string[]> GetUserRolesAsyncForTenant(Guid userId, Guid tenantId)
        {
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
            // This is just a simple example for demonstration purposes
            return password; // Don't do this in production!
        }

        private bool VerifyPassword(string password, string hash)
        {
            // In a real implementation, use a proper password hashing library
            // This is just a simple example for demonstration purposes
            return password == hash; // Don't do this in production!
        }
    }
    #endregion
}
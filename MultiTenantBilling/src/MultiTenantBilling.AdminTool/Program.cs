﻿﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MultiTenantBilling.Application.DTOs;
using MultiTenantBilling.Application.Services;
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Data;
using MultiTenantBilling.Infrastructure.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

// Create a host builder to configure services
var builder = Host.CreateApplicationBuilder(args);

// Register database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql("Host=localhost;Port=5432;Database=multitenant;Username=postgres;Password=fahadakashMain"));

// Register tenant services
builder.Services.AddScoped<ITenantService, MockTenantService>();

// Register repositories
builder.Services.AddScoped<ITenantRepository<User>, UserRepository>();
builder.Services.AddScoped<ITenantRepository<Role>, RoleRepository>();
builder.Services.AddScoped<ITenantRepository<UserRole>, UserRoleRepository>();
builder.Services.AddScoped<ITenantRepository<Tenant>, TenantRepository>();
builder.Services.AddScoped<ITenantRepository<Plan>, PlanRepository>();
builder.Services.AddScoped<ITenantRepository<Subscription>, SubscriptionRepository>();

// Register application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddSingleton<JwtService>();

var host = builder.Build();

// Get the required services
var scope = host.Services.CreateScope();
var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
var authorizationService = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();
var roleRepository = scope.ServiceProvider.GetRequiredService<ITenantRepository<Role>>();
var userRepository = scope.ServiceProvider.GetRequiredService<ITenantRepository<User>>();
var userRoleRepository = scope.ServiceProvider.GetRequiredService<ITenantRepository<UserRole>>();
var tenantRepository = scope.ServiceProvider.GetRequiredService<ITenantRepository<Tenant>>();

// Set a mock tenant ID for our operations
((MockTenantService)tenantService).SetTenantId(Guid.Parse("11111111-1111-1111-1111-111111111111"));

try
{
    var tenantId = tenantService.GetRequiredTenantId();
    
    // First, let's create the tenant if it doesn't exist
    Console.WriteLine("Checking if tenant exists...");
    // For Tenant entity, we need to query directly since GetByIdAsync throws NotImplementedException
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var tenant = await context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
    
    if (tenant == null)
    {
        Console.WriteLine("Tenant not found, creating it...");
        tenant = new Tenant
        {
            Id = tenantId, // Set the ID to match the tenantId
            TenantId = tenantId, // For Tenant entity, TenantId should be the same as Id
            Name = "Default Tenant",
            Subdomain = "default",
            Email = "admin@example.com",
            Status = "Active"
        };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
        Console.WriteLine($"Tenant created with ID: {tenant.Id}");
    }
    else
    {
        Console.WriteLine($"Tenant already exists with ID: {tenant.Id}");
    }
    
    // First, let's create the Admin role if it doesn't exist
    Console.WriteLine("Checking if Admin role exists...");
    var roles = await roleRepository.GetByTenantIdAsync(tenantId);
    var adminRole = roles.FirstOrDefault(r => r.Name == "Admin");
    
    if (adminRole == null)
    {
        Console.WriteLine("Admin role not found, creating it...");
        adminRole = new Role
        {
            TenantId = tenantId,
            Name = "Admin",
            Description = "Administrator role with full access"
        };
        adminRole = await roleRepository.AddAsync(adminRole);
        Console.WriteLine($"Admin role created with ID: {adminRole.Id}");
    }
    else
    {
        Console.WriteLine($"Admin role already exists with ID: {adminRole.Id}");
    }
    
    // Check if admin user exists
    Console.WriteLine("Checking if admin user exists...");
    var users = await userRepository.GetByTenantIdAsync(tenantId);
    var adminUser = users.FirstOrDefault(u => u.Email == "admin@example.com");
    
    if (adminUser == null)
    {
        // Create an admin user directly in the existing tenant
        Console.WriteLine("Creating admin user...");
        var adminUserEntity = new User
        {
            TenantId = tenantId,
            Email = "admin@example.com",
            PasswordHash = "Admin123!", // Using the simple hash from AuthService
            FirstName = "Admin",
            LastName = "User",
            IsActive = true
        };

        adminUser = await userRepository.AddAsync(adminUserEntity);
        Console.WriteLine($"Admin user created successfully with ID: {adminUser.Id}");
    }
    else
    {
        Console.WriteLine($"Admin user already exists with ID: {adminUser.Id}");
    }
    
    // Check if the user already has the Admin role
    Console.WriteLine("Checking if user already has Admin role...");
    var userRoles = await userRoleRepository.GetByTenantIdAsync(tenantId);
    var existingUserRole = userRoles.FirstOrDefault(ur => ur.UserId == adminUser.Id && ur.RoleId == adminRole.Id);
    
    if (existingUserRole == null)
    {
        // Assign Admin role to the user
        Console.WriteLine("Assigning Admin role to the user...");
        var userRole = new UserRole
        {
            TenantId = tenantId,
            UserId = adminUser.Id,
            RoleId = adminRole.Id
        };
        await userRoleRepository.AddAsync(userRole);
        Console.WriteLine("Admin role assigned successfully!");
    }
    else
    {
        Console.WriteLine("User already has Admin role assigned.");
    }
    
    Console.WriteLine("Admin user and role setup completed!");

    // Create a basic plan
    Console.WriteLine("Creating a basic plan...");
    var planRepository = scope.ServiceProvider.GetRequiredService<ITenantRepository<Plan>>();
    var plans = await planRepository.GetByTenantIdAsync(tenantId);
    var basicPlan = plans.FirstOrDefault(p => p.Name == "Basic Plan");

    if (basicPlan == null)
    {
        basicPlan = new Plan
        {
            TenantId = tenantId,
            Name = "Basic Plan",
            Description = "Basic plan with limited features",
            MonthlyPrice = 29.99m,
            MaxUsers = 5,
            MaxStorageGb = 100,
            IsActive = true
        };
        basicPlan = await planRepository.AddAsync(basicPlan);
        Console.WriteLine($"Basic plan created with ID: {basicPlan.Id}");
    }
    else
    {
        Console.WriteLine($"Basic plan already exists with ID: {basicPlan.Id}");
    }

    Console.WriteLine("Setup completed successfully!");

    // Create a subscription for the admin user
    Console.WriteLine("Creating a subscription for the admin user...");
    var subscriptionRepository = scope.ServiceProvider.GetRequiredService<ITenantRepository<Subscription>>();
    var subscription = new Subscription
    {
        TenantId = tenantId,
        PlanId = basicPlan.Id,
        StartDate = DateTime.UtcNow,
        EndDate = DateTime.UtcNow.AddMonths(1),
        Status = "Active"
    };
    subscription = await subscriptionRepository.AddAsync(subscription);
    Console.WriteLine($"Subscription created with ID: {subscription.Id}");

    Console.WriteLine("All setup completed successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"Error creating admin user: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}

public class MockTenantService : ITenantService
{
    private Guid? _tenantId;

    public Guid? GetTenantId()
    {
        return _tenantId;
    }

    public bool IsTenantAvailable()
    {
        return _tenantId.HasValue;
    }

    public Guid GetRequiredTenantId()
    {
        if (!_tenantId.HasValue)
            throw new InvalidOperationException("Tenant ID is required but not available.");
        return _tenantId.Value;
    }

    public void SetTenantId(Guid tenantId)
    {
        _tenantId = tenantId;
    }
}
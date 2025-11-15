using Microsoft.EntityFrameworkCore;
using MultiTenantBilling.Application.Services;
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Data;
using MultiTenantBilling.Infrastructure.Repositories;
using MultiTenantBilling.Worker;
using MultiTenantBilling.Worker.Services;
using MultiTenantBilling.Application.BackgroundJobs;

var builder = Host.CreateApplicationBuilder(args);

// Add Redis caching
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? 
                           builder.Configuration["Redis:ConnectionString"] ?? 
                           "localhost:6379";
    options.InstanceName = builder.Configuration["Redis:InstanceName"] ?? "MultiTenantBilling_";
});

// Register cache service
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// Register database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register tenant services (for worker, we might need a different implementation)
// For now, we'll register a mock implementation or a real one if needed
builder.Services.AddScoped<ITenantService, MockTenantService>();

// Register repositories
builder.Services.AddScoped<ITenantRepository<Subscription>, SubscriptionRepository>();
builder.Services.AddScoped<ITenantRepository<Invoice>, InvoiceRepository>();
builder.Services.AddScoped<ITenantRepository<User>, UserRepository>();
builder.Services.AddScoped<ITenantRepository<Role>, RoleRepository>();
builder.Services.AddScoped<ITenantRepository<UserRole>, UserRoleRepository>();
builder.Services.AddScoped<ITenantRepository<Plan>, PlanRepository>();
builder.Services.AddScoped<ITenantRepository<UsageRecord>, UsageRecordRepository>();
builder.Services.AddScoped<ITenantRepository<Payment>, PaymentRepository>();
builder.Services.AddScoped<ITenantRepository<Tenant>, TenantRepository>();

// Register base services
builder.Services.AddScoped<SubscriptionService>();
builder.Services.AddScoped<PlanService>();

// Register cached services with decorator pattern
builder.Services.AddScoped<ISubscriptionService>(provider =>
{
    var subscriptionService = provider.GetRequiredService<SubscriptionService>();
    var cacheService = provider.GetRequiredService<ICacheService>();
    var tenantService = provider.GetRequiredService<ITenantService>();
    return new CachedSubscriptionService(subscriptionService, cacheService, tenantService);
});

builder.Services.AddScoped<IPlanService>(provider =>
{
    var planService = provider.GetRequiredService<PlanService>();
    var cacheService = provider.GetRequiredService<ICacheService>();
    var tenantService = provider.GetRequiredService<ITenantService>();
    return new CachedPlanService(planService, cacheService, tenantService);
});

builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IUsageService, UsageService>();

// Register background jobs
builder.Services.AddScoped<InvoiceGenerationJob>();

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IAuthService).Assembly));

// Add background services
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
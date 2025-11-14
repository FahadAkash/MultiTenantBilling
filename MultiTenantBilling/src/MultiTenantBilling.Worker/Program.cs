using Microsoft.EntityFrameworkCore;
using MultiTenantBilling.Application.Services;
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Data;
using MultiTenantBilling.Infrastructure.Repositories;
using MultiTenantBilling.Worker;
using MultiTenantBilling.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

// Register database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register tenant services (for worker, we might need a different implementation)
// For now, we'll register a mock implementation or a real one if needed
builder.Services.AddScoped<ITenantService, MockTenantService>();

// Register repositories
builder.Services.AddScoped<ITenantRepository<Subscription>, SubscriptionRepository>();
builder.Services.AddScoped<ITenantRepository<Invoice>, InvoiceRepository>();
builder.Services.AddScoped<ITenantRepository<User>, UserRepository>();
builder.Services.AddScoped<ITenantRepository<Role>, RoleRepository>();
builder.Services.AddScoped<ITenantRepository<UserRole>, UserRoleRepository>();
builder.Services.AddScoped<ITenantRepository<Plan>, TenantRepositoryBase<Plan>>();
builder.Services.AddScoped<ITenantRepository<UsageRecord>, TenantRepositoryBase<UsageRecord>>();
builder.Services.AddScoped<ITenantRepository<Payment>, TenantRepositoryBase<Payment>>();

// Register application services
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IUsageService, UsageService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
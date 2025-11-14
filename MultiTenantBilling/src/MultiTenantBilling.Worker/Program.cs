using Microsoft.EntityFrameworkCore;
using MultiTenantBilling.Application.Services;
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Data;
using MultiTenantBilling.Infrastructure.Repositories;
using MultiTenantBilling.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Register database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
builder.Services.AddScoped<ITenantRepository<Subscription>, SubscriptionRepository>();
builder.Services.AddScoped<ITenantRepository<Invoice>, InvoiceRepository>();
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
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using MultiTenantBilling.Api.Filters;
using MultiTenantBilling.Api.Middleware;
using MultiTenantBilling.Api.Services;
using MultiTenantBilling.Application.BackgroundJobs;
using MultiTenantBilling.Application.Services;
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Data;
using MultiTenantBilling.Infrastructure.Repositories;
using System.IO;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(); // Add this to register controllers

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001", "http://localhost:3002")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Multi-Tenant Billing API", 
        Version = "v1",
        Description = "A multi-tenant billing system API with subscription management, invoicing, and payment processing."
    });

    // Add JWT Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    // Set the comments path for the Swagger JSON and UI.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Register database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register tenant services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IApiTenantService, TenantService>();
// Since IApiTenantService implements ITenantService, we can register it directly
builder.Services.AddScoped<ITenantService>(provider => 
    (ITenantService)provider.GetRequiredService<IApiTenantService>());

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
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();

// Register JWT service
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddSingleton<JwtService>();

// Add JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IAuthService).Assembly));

// Add authorization with policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ViewBilling", policy => policy.RequireRole("Admin", "User", "BillingManager"));
    options.AddPolicy("ManageSubscriptions", policy => policy.RequireRole("Admin", "BillingManager"));
    options.AddPolicy("ProcessPayments", policy => policy.RequireRole("Admin", "BillingManager"));
});

// Hangfire
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

// Register background jobs
builder.Services.AddScoped<InvoiceGenerationJob>();
builder.Services.AddScoped<PaymentRetryJob>();
builder.Services.AddScoped<UsageAggregationJob>();
builder.Services.AddScoped<DunningProcessJob>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Multi-Tenant Billing API v1");
});

// Add Scalar in all environments
app.MapScalarApiReference(options =>
{
    options.WithTitle("Multi-Tenant Billing API");
    options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.UseHttpsRedirection();

// Add CORS middleware
app.UseCors("AllowFrontend");

// Add authentication and authorization middleware FIRST
app.UseAuthentication();
app.UseAuthorization();

// Register tenant middleware AFTER authentication
app.UseMiddleware<TenantMiddleware>();

// Add Hangfire dashboard (protected by authorization)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

// Schedule background jobs after the app is built
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<Hangfire.IRecurringJobManager>();
    
    recurringJobManager.AddOrUpdate<InvoiceGenerationJob>(
        "InvoiceGenerationJob",
        job => job.ExecuteAsync(),
        "0 2 * * *", // Run daily at 2 AM
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

    recurringJobManager.AddOrUpdate<PaymentRetryJob>(
        "PaymentRetryJob",
        job => job.ExecuteAsync(),
        "*/15 * * * *", // Run every 15 minutes
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

    recurringJobManager.AddOrUpdate<UsageAggregationJob>(
        "UsageAggregationJob",
        job => job.ExecuteAsync(),
        "0 * * * *", // Run hourly
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

    recurringJobManager.AddOrUpdate<DunningProcessJob>(
        "DunningProcessJob",
        job => job.ExecuteAsync(),
        "0 8 * * *", // Run daily at 8 AM
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
}

// Add controllers
app.MapControllers(); // Add this to map controller routes

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
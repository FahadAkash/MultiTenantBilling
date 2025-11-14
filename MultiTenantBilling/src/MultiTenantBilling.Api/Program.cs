using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using MultiTenantBilling.Api.Middleware;
using MultiTenantBilling.Api.Services;
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
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// Register application services
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
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

// Add authorization with policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ViewBilling", policy => policy.RequireRole("Admin", "User", "BillingManager"));
    options.AddPolicy("ManageSubscriptions", policy => policy.RequireRole("Admin", "BillingManager"));
    options.AddPolicy("ProcessPayments", policy => policy.RequireRole("Admin", "BillingManager"));
});

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

// Register tenant middleware
app.UseMiddleware<TenantMiddleware>();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

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
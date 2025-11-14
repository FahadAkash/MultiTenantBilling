using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantBilling.Api.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantMiddleware> _logger;

        public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            Guid? tenantId = null;

            // Strategy 1: Extract tenant ID from JWT token (recommended for production)
            if (context.Request.Headers.TryGetValue("Authorization", out var authHeader) && 
                authHeader.ToString().StartsWith("Bearer "))
            {
                var token = authHeader.ToString().Substring("Bearer ".Length).Trim();
                tenantId = ExtractTenantIdFromJwt(token);
            }

            // Strategy 2: Extract tenant ID from custom header (for API clients)
            if (!tenantId.HasValue && context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantIdHeader))
            {
                if (Guid.TryParse(tenantIdHeader, out Guid headerTenantId))
                {
                    tenantId = headerTenantId;
                }
            }

            // Strategy 3: Extract tenant from subdomain (for web apps)
            if (!tenantId.HasValue)
            {
                tenantId = ExtractTenantIdFromSubdomain(context.Request.Host.Host);
            }

            // Store tenant ID in HttpContext items
            if (tenantId.HasValue)
            {
                context.Items["TenantId"] = tenantId.Value;
                _logger.LogInformation("Tenant identified: {TenantId}", tenantId.Value);
            }
            else
            {
                _logger.LogWarning("No tenant ID found in request");
            }

            // Call the next middleware in the pipeline
            await _next(context);
        }

        private Guid? ExtractTenantIdFromJwt(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(token);
                
                // Look for tenantId in claims
                foreach (var claim in jsonToken.Claims)
                {
                    if (claim.Type == "tenantId" && Guid.TryParse(claim.Value, out Guid tenantId))
                    {
                        return tenantId;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting tenant ID from JWT token");
            }

            return null;
        }

        private Guid? ExtractTenantIdFromSubdomain(string host)
        {
            try
            {
                // Example: acme-corp.yourapp.com → extract "acme-corp"
                // This would require a lookup service to map subdomain to tenant ID
                // For now, we'll just log the subdomain
                var parts = host.Split('.');
                if (parts.Length > 2)
                {
                    var subdomain = parts[0];
                    _logger.LogInformation("Subdomain detected: {Subdomain}", subdomain);
                    // In a real implementation, you would look up the tenant ID from the subdomain
                    // return _tenantLookupService.GetTenantIdBySubdomain(subdomain);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting tenant ID from subdomain");
            }

            return null;
        }
    }
}
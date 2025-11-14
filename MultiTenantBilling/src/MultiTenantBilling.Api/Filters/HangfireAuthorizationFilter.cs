using Hangfire.Dashboard;

namespace MultiTenantBilling.Api.Filters
{
    /// <summary>
    /// Authorization filter for Hangfire dashboard
    /// TODO: Implement proper authentication/authorization in production
    /// </summary>
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // TODO: Add proper authentication check
            // For now, allow all (NOT FOR PRODUCTION!)
            return true;
        }
    }
}


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiTenantBilling.Api.Attributes;
using System.Security.Claims;

namespace MultiTenantBilling.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [RequireTenant]
    [Authorize] // Require authentication for all actions in this controller
    public class AdminController : ControllerBase
    {
        [HttpGet("dashboard")]
        [Authorize(Roles = "Admin")] // Require Admin role
        public ActionResult<string> GetAdminDashboard()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            return Ok($"Admin Dashboard - Welcome {userEmail}!");
        }

        [HttpGet("billing")]
        [Authorize(Policy = "ViewBilling")] // Require ViewBilling policy
        public ActionResult<string> GetBillingInfo()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            return Ok($"Billing Information - Access granted to {userEmail}");
        }

        [HttpPost("subscriptions")]
        [Authorize(Policy = "ManageSubscriptions")] // Require ManageSubscriptions policy
        public ActionResult<string> CreateSubscription()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            return Ok($"Subscription created - Action performed by {userEmail}");
        }

        [HttpPost("payments")]
        [Authorize(Policy = "ProcessPayments")] // Require ProcessPayments policy
        public ActionResult<string> ProcessPayment()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            return Ok($"Payment processed - Action performed by {userEmail}");
        }
    }
}
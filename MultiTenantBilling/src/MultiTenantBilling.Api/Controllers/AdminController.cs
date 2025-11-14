using Microsoft.AspNetCore.Mvc;
using MultiTenantBilling.Api.Attributes;
using System;

namespace MultiTenantBilling.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [RequireTenant]
    public class AdminController : ControllerBase
    {
        [HttpGet("dashboard")]
        [AuthorizeRole("Admin")]
        public ActionResult<string> GetAdminDashboard()
        {
            return Ok("Admin Dashboard - Only accessible by users with Admin role");
        }

        [HttpGet("billing")]
        [AuthorizePermission("view_billing")]
        public ActionResult<string> GetBillingInfo()
        {
            return Ok("Billing Information - Only accessible by users with view_billing permission");
        }

        [HttpPost("subscriptions")]
        [AuthorizePermission("manage_subscriptions")]
        public ActionResult<string> CreateSubscription()
        {
            return Ok("Subscription created - Only accessible by users with manage_subscriptions permission");
        }

        [HttpPost("payments")]
        [AuthorizePermission("process_payments")]
        public ActionResult<string> ProcessPayment()
        {
            return Ok("Payment processed - Only accessible by users with process_payments permission");
        }
    }
}
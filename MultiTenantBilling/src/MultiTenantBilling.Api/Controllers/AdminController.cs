using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiTenantBilling.Api.Attributes;
using MultiTenantBilling.Api.Services;
using MultiTenantBilling.Application.Commands;
using MultiTenantBilling.Application.DTOs;
using MultiTenantBilling.Application.Queries;
using MultiTenantBilling.Application.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MultiTenantBilling.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [RequireTenant]
    [Authorize] // Require authentication for all actions in this controller
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IApiTenantService _tenantService;

        public AdminController(IMediator mediator, IApiTenantService tenantService)
        {
            _mediator = mediator;
            _tenantService = tenantService;
        }

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

        /// <summary>
        /// Creates a new plan for the tenant.
        /// </summary>
        /// <param name="request">The plan details.</param>
        /// <returns>The created plan.</returns>
        /// <response code="200">Returns the created plan.</response>
        [HttpPost("plans")]
        [Authorize(Roles = "Admin")] // Require Admin role
        [ProducesResponseType(typeof(PlanDto), 200)]
        public async Task<ActionResult<PlanDto>> CreatePlan([FromBody] CreatePlanRequest request)
        {
            var tenantId = _tenantService.GetRequiredTenantId();
            var plan = await _mediator.Send(new CreatePlanCommand
            {
                TenantId = tenantId,
                Name = request.Name,
                Description = request.Description,
                MonthlyPrice = request.MonthlyPrice,
                MaxUsers = request.MaxUsers,
                MaxStorageGb = request.MaxStorageGb,
                IsActive = request.IsActive
            });
            return Ok(plan);
        }

        /// <summary>
        /// Gets a plan by ID.
        /// </summary>
        /// <param name="planId">The ID of the plan.</param>
        /// <returns>The plan details.</returns>
        /// <response code="200">Returns the plan.</response>
        [HttpGet("plans/{planId}")]
        [Authorize(Roles = "Admin")] // Require Admin role
        [ProducesResponseType(typeof(PlanDto), 200)]
        public ActionResult<PlanDto> GetPlan(Guid planId)
        {
            var tenantId = _tenantService.GetRequiredTenantId();
            // In a real implementation, you would use a query handler to get the plan
            // For now, we'll return a placeholder
            return Ok(new PlanDto
            {
                Id = planId,
                Name = "Sample Plan",
                Description = "A sample plan for demonstration",
                MonthlyPrice = 29.99m,
                MaxUsers = 10,
                MaxStorageGb = 100
            });
        }

        /// <summary>
        /// Gets all plans for the tenant.
        /// </summary>
        /// <returns>A list of all plans.</returns>
        /// <response code="200">Returns the list of plans.</response>
        [HttpGet("plans")]
        [Authorize(Roles = "Admin")] // Require Admin role
        [ProducesResponseType(typeof(IEnumerable<PlanDto>), 200)]
        public async Task<ActionResult<IEnumerable<PlanDto>>> GetAllPlans()
        {
            var tenantId = _tenantService.GetRequiredTenantId();
            var plans = await _mediator.Send(new GetAllPlansQuery { TenantId = tenantId });
            return Ok(plans);
        }

        /// <summary>
        /// Gets all users for the tenant.
        /// </summary>
        /// <returns>A list of all users.</returns>
        /// <response code="200">Returns the list of users.</response>
        [HttpGet("users")]
        [Authorize(Roles = "Admin")] // Require Admin role
        [ProducesResponseType(typeof(IEnumerable<UserDto>), 200)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers([FromServices] IAuthService authService)
        {
            var users = await authService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Activates a user.
        /// </summary>
        /// <param name="userId">The ID of the user to activate.</param>
        /// <returns>True if successful.</returns>
        /// <response code="200">Returns true if successful.</response>
        [HttpPost("users/{userId}/activate")]
        [Authorize(Roles = "Admin")] // Require Admin role
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult<bool>> ActivateUser(Guid userId, [FromServices] IAuthService authService)
        {
            var result = await authService.ActivateUserAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// Deactivates a user.
        /// </summary>
        /// <param name="userId">The ID of the user to deactivate.</param>
        /// <returns>True if successful.</returns>
        /// <response code="200">Returns true if successful.</response>
        [HttpPost("users/{userId}/deactivate")]
        [Authorize(Roles = "Admin")] // Require Admin role
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult<bool>> DeactivateUser(Guid userId, [FromServices] IAuthService authService)
        {
            var result = await authService.DeactivateUserAsync(userId);
            return Ok(result);
        }
    }

    /// <summary>
    /// DTO for creating a plan.
    /// </summary>
    public class CreatePlanRequest
    {
        /// <summary>
        /// The name of the plan.
        /// </summary>
        public string Name { get; set; } = default!;

        /// <summary>
        /// The description of the plan.
        /// </summary>
        public string Description { get; set; } = default!;

        /// <summary>
        /// The monthly price of the plan.
        /// </summary>
        public decimal MonthlyPrice { get; set; }

        /// <summary>
        /// The maximum number of users allowed.
        /// </summary>
        public int MaxUsers { get; set; }

        /// <summary>
        /// The maximum storage in GB.
        /// </summary>
        public int MaxStorageGb { get; set; }

        /// <summary>
        /// Whether the plan is active.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
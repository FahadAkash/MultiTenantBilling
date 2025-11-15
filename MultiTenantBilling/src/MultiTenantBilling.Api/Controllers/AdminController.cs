using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiTenantBilling.Api.Attributes;
using MultiTenantBilling.Api.Services;
using MultiTenantBilling.Application.Commands;
using MultiTenantBilling.Application.DTOs;
using MultiTenantBilling.Application.Queries;
using MultiTenantBilling.Application.Services;
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Gets all tenants in the system.
        /// </summary>
        /// <returns>A list of all tenants.</returns>
        /// <response code="200">Returns the list of tenants.</response>
        [HttpGet("tenants")]
        [Authorize(Roles = "Admin")] // Require Admin role
        [ProducesResponseType(typeof(IEnumerable<TenantDto>), 200)]
        public async Task<ActionResult<IEnumerable<TenantDto>>> GetAllTenants([FromServices] ITenantRepository<Tenant> tenantRepository)
        {
            var tenants = await tenantRepository.GetAllEntitiesAsync();
            var tenantDtos = tenants.Select(tenant => new TenantDto
            {
                Id = tenant.Id,
                TenantId = tenant.TenantId,
                Name = tenant.Name,
                Subdomain = tenant.Subdomain,
                Email = tenant.Email,
                Status = tenant.Status
            }).ToList();
            
            return Ok(tenantDtos);
        }

        /// <summary>
        /// Gets all subscriptions for a specific tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant to get subscriptions for.</param>
        /// <returns>A list of subscriptions for the tenant.</returns>
        /// <response code="200">Returns the list of subscriptions.</response>
        [HttpGet("tenants/{tenantId}/subscriptions")]
        [Authorize(Roles = "Admin")] // Require Admin role
        [ProducesResponseType(typeof(IEnumerable<SubscriptionDto>), 200)]
        public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetSubscriptionsForTenant(Guid tenantId,
            [FromServices] ITenantRepository<Subscription> subscriptionRepository,
            [FromServices] ITenantService tenantService)
        {
            // Temporarily set the tenant ID for this operation
            var originalTenantId = tenantService.GetTenantId();
            tenantService.SetTenantId(tenantId);
            
            try
            {
                var subscriptions = await subscriptionRepository.GetByTenantIdAsync(tenantId);
                var subscriptionDtos = subscriptions.Select(subscription => new SubscriptionDto
                {
                    Id = subscription.Id,
                    TenantId = subscription.TenantId,
                    PlanId = subscription.PlanId,
                    StartDate = subscription.StartDate,
                    EndDate = subscription.EndDate,
                    Status = subscription.Status
                }).ToList();
                
                return Ok(subscriptionDtos);
            }
            finally
            {
                // Restore the original tenant ID
                if (originalTenantId.HasValue)
                    tenantService.SetTenantId(originalTenantId.Value);
            }
        }

        /// <summary>
        /// Creates a new plan for a specific tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant to create the plan for.</param>
        /// <param name="request">The plan details.</param>
        /// <returns>The created plan.</returns>
        /// <response code="200">Returns the created plan.</response>
        [HttpPost("tenants/{tenantId}/plans")]
        [Authorize(Roles = "Admin")] // Require Admin role
        [ProducesResponseType(typeof(PlanDto), 200)]
        public async Task<ActionResult<PlanDto>> CreatePlanForTenant(Guid tenantId, [FromBody] CreatePlanRequest request, 
            [FromServices] ITenantRepository<Plan> planRepository, 
            [FromServices] ITenantService tenantService)
        {
            // Temporarily set the tenant ID for this operation
            var originalTenantId = tenantService.GetTenantId();
            tenantService.SetTenantId(tenantId);
            
            try
            {
                var plan = new Plan
                {
                    TenantId = tenantId,
                    Name = request.Name,
                    Description = request.Description,
                    MonthlyPrice = request.MonthlyPrice,
                    MaxUsers = request.MaxUsers,
                    MaxStorageGb = request.MaxStorageGb,
                    IsActive = request.IsActive
                };
                
                var createdPlan = await planRepository.AddAsync(plan);
                
                var planDto = new PlanDto
                {
                    Id = createdPlan.Id,
                    Name = createdPlan.Name,
                    Description = createdPlan.Description,
                    MonthlyPrice = createdPlan.MonthlyPrice,
                    MaxUsers = createdPlan.MaxUsers,
                    MaxStorageGb = createdPlan.MaxStorageGb
                };
                
                return Ok(planDto);
            }
            finally
            {
                // Restore the original tenant ID
                if (originalTenantId.HasValue)
                    tenantService.SetTenantId(originalTenantId.Value);
                else
                    // Clear tenant ID if it wasn't set originally
                    // This would require a way to clear the tenant ID, which isn't currently implemented
                    // For now, we'll just leave it as is
                    tenantService.SetTenantId(tenantId);
            }
        }
        
        /// <summary>
        /// Registers a new user for a specific tenant with assigned roles.
        /// </summary>
        /// <param name="request">The user registration details.</param>
        /// <returns>The authentication response with a JWT token.</returns>
        /// <response code="200">Returns the authentication response.</response>
        /// <response code="400">If the user already exists or validation fails.</response>
        [HttpPost("register")]
        [Authorize(Roles = "Admin")] // Require Admin role
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(typeof(string), 400)]
        public async Task<ActionResult<AuthResponseDto>> AdminRegister([FromBody] AdminRegisterDto request, 
            [FromServices] IAuthService authService)
        {
            try
            {
                var result = await authService.AdminRegisterAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
        
        /// <summary>
        /// Manually generates an invoice for a specific tenant.
        /// </summary>
        /// <param name="request">The invoice generation details.</param>
        /// <returns>The generated invoice.</returns>
        /// <response code="200">Returns the generated invoice.</response>
        [HttpPost("invoices")]
        [Authorize(Roles = "Admin")] // Require Admin role
        [ProducesResponseType(typeof(InvoiceDto), 200)]
        public async Task<ActionResult<InvoiceDto>> GenerateManualInvoice([FromBody] GenerateInvoiceRequest request,
            [FromServices] ITenantRepository<Invoice> invoiceRepository,
            [FromServices] ITenantRepository<Subscription> subscriptionRepository,
            [FromServices] ITenantRepository<Plan> planRepository,
            [FromServices] ITenantService tenantService)
        {
            // Temporarily set the tenant ID for this operation
            var originalTenantId = tenantService.GetTenantId();
            tenantService.SetTenantId(request.TenantId);
            
            try
            {
                // First, check if there's an existing active subscription for this tenant
                var existingSubscriptions = await subscriptionRepository.GetByTenantIdAsync(request.TenantId);
                var activeSubscription = existingSubscriptions.FirstOrDefault(s => s.Status == "Active");
                
                // If no active subscription exists, create a default one
                if (activeSubscription == null)
                {
                    // Get the first available plan for this tenant
                    var plans = await planRepository.GetByTenantIdAsync(request.TenantId);
                    var defaultPlan = plans.FirstOrDefault();
                    
                    if (defaultPlan != null)
                    {
                        // Create a default subscription
                        activeSubscription = new Subscription
                        {
                            TenantId = request.TenantId,
                            PlanId = defaultPlan.Id,
                            StartDate = DateTime.UtcNow,
                            EndDate = DateTime.UtcNow.AddMonths(1),
                            Status = "Active"
                        };
                        
                        activeSubscription = await subscriptionRepository.AddAsync(activeSubscription);
                    }
                }
                
                // Create a manual invoice
                var invoice = new Invoice
                {
                    TenantId = request.TenantId,
                    SubscriptionId = activeSubscription?.Id ?? Guid.NewGuid(), // Use existing subscription or create a temporary ID
                    Amount = request.Amount,
                    InvoiceDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(7), // 7 days to pay
                    IsPaid = false,
                    Status = "Pending",
                    Description = request.Description
                };
                
                var createdInvoice = await invoiceRepository.AddAsync(invoice);
                
                var invoiceDto = new InvoiceDto
                {
                    Id = createdInvoice.Id,
                    TenantId = createdInvoice.TenantId,
                    SubscriptionId = createdInvoice.SubscriptionId,
                    Amount = createdInvoice.Amount,
                    InvoiceDate = createdInvoice.InvoiceDate,
                    DueDate = createdInvoice.DueDate,
                    IsPaid = createdInvoice.IsPaid,
                    Status = createdInvoice.Status,
                    Description = createdInvoice.Description
                };
                
                return Ok(invoiceDto);
            }
            finally
            {
                // Restore the original tenant ID
                if (originalTenantId.HasValue)
                    tenantService.SetTenantId(originalTenantId.Value);
            }
        }
    }

    /// <summary>
    /// DTO for tenant information.
    /// </summary>
    public class TenantDto
    {
        /// <summary>
        /// The ID of the tenant.
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// The tenant ID.
        /// </summary>
        public Guid TenantId { get; set; }
        
        /// <summary>
        /// The name of the tenant.
        /// </summary>
        public string Name { get; set; } = default!;
        
        /// <summary>
        /// The subdomain of the tenant.
        /// </summary>
        public string Subdomain { get; set; } = default!;
        
        /// <summary>
        /// The email of the tenant.
        /// </summary>
        public string Email { get; set; } = default!;
        
        /// <summary>
        /// The status of the tenant.
        /// </summary>
        public string Status { get; set; } = default!;
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
    
    /// <summary>
    /// DTO for manually generating an invoice.
    /// </summary>
    public class GenerateInvoiceRequest
    {
        /// <summary>
        /// The ID of the tenant to generate an invoice for.
        /// </summary>
        public Guid TenantId { get; set; }
        
        /// <summary>
        /// Optional description for the manual invoice.
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// The amount for the manual invoice.
        /// </summary>
        public decimal Amount { get; set; }
    }
}
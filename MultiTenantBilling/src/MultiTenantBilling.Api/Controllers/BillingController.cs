using MediatR;
using Microsoft.AspNetCore.Mvc;
using MultiTenantBilling.Api.Attributes;
using MultiTenantBilling.Api.Services;
using MultiTenantBilling.Application.Commands;
using MultiTenantBilling.Application.DTOs;
using MultiTenantBilling.Application.Queries;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiTenantBilling.Api.Controllers
{
    /// <summary>
    /// Controller for billing operations including subscriptions, invoices, and payments.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [RequireTenant]
    public class BillingController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IApiTenantService _tenantService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BillingController"/> class.
        /// </summary>
        /// <param name="mediator">The MediatR mediator for CQRS.</param>
        /// <param name="tenantService">The tenant service.</param>
        public BillingController(IMediator mediator, IApiTenantService tenantService)
        {
            _mediator = mediator;
            _tenantService = tenantService;
        }

        /// <summary>
        /// Creates a new subscription for the tenant.
        /// </summary>
        /// <param name="planId">The ID of the plan to subscribe to.</param>
        /// <returns>The created subscription.</returns>
        /// <response code="200">Returns the created subscription.</response>
        [HttpPost("subscriptions")]
        [ProducesResponseType(typeof(SubscriptionDto), 200)]
        public async Task<ActionResult<SubscriptionDto>> CreateSubscription([FromBody] CreateSubscriptionRequest request)
        {
            var tenantId = _tenantService.GetRequiredTenantId();
            var subscription = await _mediator.Send(new CreateSubscriptionCommand
            {
                TenantId = tenantId,
                PlanId = request.PlanId,
                StartDate = DateTime.UtcNow,
                PaymentMethodId = request.PaymentMethodId
            });
            return Ok(subscription);
        }

        /// <summary>
        /// Records usage for a subscription.
        /// </summary>
        /// <param name="subscriptionId">The ID of the subscription.</param>
        /// <param name="request">The usage record details.</param>
        /// <returns>The recorded usage record.</returns>
        /// <response code="200">Returns the recorded usage record.</response>
        [HttpPost("subscriptions/{subscriptionId}/usage")]
        [ProducesResponseType(typeof(UsageRecordDto), 200)]
        public async Task<ActionResult<UsageRecordDto>> RecordUsage(Guid subscriptionId, [FromBody] UsageRecordRequest request)
        {
            var usageRecord = await _mediator.Send(new RecordUsageCommand
            {
                SubscriptionId = subscriptionId,
                MetricName = request.MetricName,
                Quantity = request.Quantity
            });
            return Ok(usageRecord);
        }

        /// <summary>
        /// Generates an invoice for a subscription.
        /// </summary>
        /// <param name="subscriptionId">The ID of the subscription.</param>
        /// <returns>The generated invoice.</returns>
        /// <response code="200">Returns the generated invoice.</response>
        [HttpPost("subscriptions/{subscriptionId}/invoices")]
        [ProducesResponseType(typeof(InvoiceDto), 200)]
        public async Task<ActionResult<InvoiceDto>> GenerateInvoice(Guid subscriptionId)
        {
            var invoice = await _mediator.Send(new GenerateInvoiceCommand
            {
                SubscriptionId = subscriptionId,
                InvoiceDate = DateTime.UtcNow,
                IncludeOverage = true
            });
            return Ok(invoice);
        }

        /// <summary>
        /// Processes a payment for an invoice.
        /// </summary>
        /// <param name="invoiceId">The ID of the invoice.</param>
        /// <param name="request">The payment details.</param>
        /// <returns>The processed payment.</returns>
        /// <response code="200">Returns the processed payment.</response>
        [HttpPost("invoices/{invoiceId}/payments")]
        [ProducesResponseType(typeof(PaymentDto), 200)]
        public async Task<ActionResult<PaymentDto>> ProcessPayment(Guid invoiceId, [FromBody] PaymentRequest request)
        {
            var payment = await _mediator.Send(new ProcessPaymentCommand
            {
                InvoiceId = invoiceId,
                PaymentMethodId = request.PaymentMethodId,
                IsRetry = false,
                RetryAttempt = 0
            });
            return Ok(payment);
        }

        /// <summary>
        /// Gets all available plans for the tenant.
        /// </summary>
        /// <returns>A list of all available plans.</returns>
        /// <response code="200">Returns the list of plans.</response>
        [HttpGet("plans")]
        [ProducesResponseType(typeof(IEnumerable<PlanDto>), 200)]
        public async Task<ActionResult<IEnumerable<PlanDto>>> GetAllPlans()
        {
            var tenantId = _tenantService.GetRequiredTenantId();
            var plans = await _mediator.Send(new GetAllPlansQuery { TenantId = tenantId });
            return Ok(plans);
        }
    }

    /// <summary>
    /// DTO for creating a subscription.
    /// </summary>
    public class CreateSubscriptionRequest
    {
        /// <summary>
        /// The ID of the plan to subscribe to.
        /// </summary>
        public Guid PlanId { get; set; }

        /// <summary>
        /// Optional payment method ID for automatic billing.
        /// </summary>
        public string? PaymentMethodId { get; set; }
    }

    /// <summary>
    /// DTO for recording usage.
    /// </summary>
    public class UsageRecordRequest
    {
        /// <summary>
        /// The name of the metric being tracked.
        /// </summary>
        public string MetricName { get; set; } = string.Empty;

        /// <summary>
        /// The quantity of usage.
        /// </summary>
        public double Quantity { get; set; }
    }

    /// <summary>
    /// DTO for processing payments.
    /// </summary>
    public class PaymentRequest
    {
        /// <summary>
        /// The payment method ID.
        /// </summary>
        public string PaymentMethodId { get; set; } = string.Empty;
    }
}
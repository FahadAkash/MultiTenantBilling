using Microsoft.AspNetCore.Mvc;
using MultiTenantBilling.Api.Attributes;
using MultiTenantBilling.Api.Services;
using MultiTenantBilling.Application.DTOs;
using MultiTenantBilling.Application.Services;
using System;
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
        private readonly ISubscriptionService _subscriptionService;
        private readonly IInvoiceService _invoiceService;
        private readonly IPaymentService _paymentService;
        private readonly IUsageService _usageService;
        private readonly IApiTenantService _tenantService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BillingController"/> class.
        /// </summary>
        /// <param name="subscriptionService">The subscription service.</param>
        /// <param name="invoiceService">The invoice service.</param>
        /// <param name="paymentService">The payment service.</param>
        /// <param name="usageService">The usage service.</param>
        /// <param name="tenantService">The tenant service.</param>
        public BillingController(
            ISubscriptionService subscriptionService,
            IInvoiceService invoiceService,
            IPaymentService paymentService,
            IUsageService usageService,
            IApiTenantService tenantService)
        {
            _subscriptionService = subscriptionService;
            _invoiceService = invoiceService;
            _paymentService = paymentService;
            _usageService = usageService;
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
        public async Task<ActionResult<SubscriptionDto>> CreateSubscription(Guid planId)
        {
            var tenantId = _tenantService.GetRequiredTenantId();
            var subscription = await _subscriptionService.CreateSubscriptionAsync(tenantId, planId, DateTime.UtcNow);
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
            var usageRecord = await _usageService.RecordUsageAsync(subscriptionId, request.MetricName, request.Quantity);
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
            var invoice = await _invoiceService.GenerateInvoiceAsync(subscriptionId, DateTime.UtcNow);
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
            var payment = await _paymentService.ProcessPaymentAsync(invoiceId, request.PaymentMethodId);
            return Ok(payment);
        }
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
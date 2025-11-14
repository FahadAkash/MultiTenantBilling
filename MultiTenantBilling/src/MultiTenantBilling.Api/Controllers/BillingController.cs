using Microsoft.AspNetCore.Mvc;
using MultiTenantBilling.Api.Attributes;
using MultiTenantBilling.Api.Services;
using MultiTenantBilling.Application.DTOs;
using MultiTenantBilling.Application.Services;
using System;
using System.Threading.Tasks;

namespace MultiTenantBilling.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [RequireTenant]
    public class BillingController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly IInvoiceService _invoiceService;
        private readonly IPaymentService _paymentService;
        private readonly IUsageService _usageService;
        private readonly ITenantService _tenantService;

        public BillingController(
            ISubscriptionService subscriptionService,
            IInvoiceService invoiceService,
            IPaymentService paymentService,
            IUsageService usageService,
            ITenantService tenantService)
        {
            _subscriptionService = subscriptionService;
            _invoiceService = invoiceService;
            _paymentService = paymentService;
            _usageService = usageService;
            _tenantService = tenantService;
        }

        [HttpPost("subscriptions")]
        public async Task<ActionResult<SubscriptionDto>> CreateSubscription(Guid planId)
        {
            var tenantId = _tenantService.GetRequiredTenantId();
            var subscription = await _subscriptionService.CreateSubscriptionAsync(tenantId, planId, DateTime.UtcNow);
            return Ok(subscription);
        }

        [HttpPost("subscriptions/{subscriptionId}/usage")]
        public async Task<ActionResult<UsageRecordDto>> RecordUsage(Guid subscriptionId, [FromBody] UsageRecordRequest request)
        {
            var usageRecord = await _usageService.RecordUsageAsync(subscriptionId, request.MetricName, request.Quantity);
            return Ok(usageRecord);
        }

        [HttpPost("subscriptions/{subscriptionId}/invoices")]
        public async Task<ActionResult<InvoiceDto>> GenerateInvoice(Guid subscriptionId)
        {
            var invoice = await _invoiceService.GenerateInvoiceAsync(subscriptionId, DateTime.UtcNow);
            return Ok(invoice);
        }

        [HttpPost("invoices/{invoiceId}/payments")]
        public async Task<ActionResult<PaymentDto>> ProcessPayment(Guid invoiceId, [FromBody] PaymentRequest request)
        {
            var payment = await _paymentService.ProcessPaymentAsync(invoiceId, request.PaymentMethodId);
            return Ok(payment);
        }
    }

    public class UsageRecordRequest
    {
        public string MetricName { get; set; } = string.Empty;
        public double Quantity { get; set; }
    }

    public class PaymentRequest
    {
        public string PaymentMethodId { get; set; } = string.Empty;
    }
}
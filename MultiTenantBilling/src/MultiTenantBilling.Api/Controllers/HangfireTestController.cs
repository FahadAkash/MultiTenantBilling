using Hangfire;
using Microsoft.AspNetCore.Mvc;
using MultiTenantBilling.Application.BackgroundJobs;

namespace MultiTenantBilling.Api.Controllers
{
    /// <summary>
    /// Controller for testing Hangfire background jobs
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HangfireTestController : ControllerBase
    {
        /// <summary>
        /// Triggers the invoice generation job immediately
        /// </summary>
        /// <returns>Success message</returns>
        [HttpPost("trigger-invoice-generation")]
        public IActionResult TriggerInvoiceGeneration()
        {
            // Queue the job to run immediately
            BackgroundJob.Enqueue<InvoiceGenerationJob>(job => job.ExecuteAsync());
            return Ok("Invoice generation job queued successfully");
        }

        /// <summary>
        /// Triggers the payment retry job immediately
        /// </summary>
        /// <returns>Success message</returns>
        [HttpPost("trigger-payment-retry")]
        public IActionResult TriggerPaymentRetry()
        {
            // Queue the job to run immediately
            BackgroundJob.Enqueue<PaymentRetryJob>(job => job.ExecuteAsync());
            return Ok("Payment retry job queued successfully");
        }

        /// <summary>
        /// Triggers the usage aggregation job immediately
        /// </summary>
        /// <returns>Success message</returns>
        [HttpPost("trigger-usage-aggregation")]
        public IActionResult TriggerUsageAggregation()
        {
            // Queue the job to run immediately
            BackgroundJob.Enqueue<UsageAggregationJob>(job => job.ExecuteAsync());
            return Ok("Usage aggregation job queued successfully");
        }

        /// <summary>
        /// Triggers the dunning process job immediately
        /// </summary>
        /// <returns>Success message</returns>
        [HttpPost("trigger-dunning-process")]
        public IActionResult TriggerDunningProcess()
        {
            // Queue the job to run immediately
            BackgroundJob.Enqueue<DunningProcessJob>(job => job.ExecuteAsync());
            return Ok("Dunning process job queued successfully");
        }
    }
}
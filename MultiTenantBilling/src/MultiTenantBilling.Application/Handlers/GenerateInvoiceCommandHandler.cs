using MediatR;
using Microsoft.Extensions.Logging;
using MultiTenantBilling.Application.Commands;
using MultiTenantBilling.Application.DTOs;
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Domain.Events;
using MultiTenantBilling.Infrastructure.Repositories;
using System;

namespace MultiTenantBilling.Application.Handlers
{
    /// <summary>
    /// Handler for generating invoices
    /// </summary>
    public class GenerateInvoiceCommandHandler : IRequestHandler<GenerateInvoiceCommand, InvoiceDto>
    {
        private readonly ITenantRepository<Subscription> _subscriptionRepository;
        private readonly ITenantRepository<Invoice> _invoiceRepository;
        private readonly ITenantRepository<Plan> _planRepository;
        private readonly ITenantRepository<UsageRecord> _usageRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<GenerateInvoiceCommandHandler> _logger;

        public GenerateInvoiceCommandHandler(
            ITenantRepository<Subscription> subscriptionRepository,
            ITenantRepository<Invoice> invoiceRepository,
            ITenantRepository<Plan> planRepository,
            ITenantRepository<UsageRecord> usageRepository,
            IMediator mediator,
            ILogger<GenerateInvoiceCommandHandler> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _invoiceRepository = invoiceRepository;
            _planRepository = planRepository;
            _usageRepository = usageRepository;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<InvoiceDto> Handle(GenerateInvoiceCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Generating invoice for subscription {SubscriptionId}", request.SubscriptionId);

            var subscription = await _subscriptionRepository.GetByIdAsync(request.SubscriptionId);
            if (subscription == null)
            {
                throw new InvalidOperationException("Subscription not found");
            }

            var plan = await _planRepository.GetByIdAsync(subscription.PlanId);
            if (plan == null)
            {
                throw new InvalidOperationException("Plan not found");
            }

            // Calculate base subscription amount
            decimal baseAmount = plan.MonthlyPrice;

            // Calculate overage if requested
            decimal overageAmount = 0m;
            if (request.IncludeOverage)
            {
                // Get usage records for the billing period
                var usageRecords = await _usageRepository.GetByTenantIdAsync(subscription.TenantId);
                // TODO: Implement actual overage calculation based on plan limits
                overageAmount = 0m; // Placeholder
            }

            // Calculate total (base + overage + taxes)
            decimal subtotal = baseAmount + overageAmount;
            decimal taxAmount = subtotal * 0.15m; // 15% VAT (adjust as needed)
            decimal totalAmount = subtotal + taxAmount;

            // Create invoice
            var invoice = new Invoice
            {
                TenantId = subscription.TenantId,
                SubscriptionId = subscription.Id,
                Amount = totalAmount,
                InvoiceDate = request.InvoiceDate,
                DueDate = request.InvoiceDate.AddDays(7), // 7 days to pay
                IsPaid = false,
                Status = "Pending"
            };

            var createdInvoice = await _invoiceRepository.AddAsync(invoice);

            // Check if this is a renewal (subscription is expiring)
            bool isRenewal = subscription.EndDate.Date <= request.InvoiceDate.Date;
            
            // If this is a renewal, automatically attempt to process payment
            if (isRenewal)
            {
                // TODO: Get actual payment method from customer
                // For now, we'll use a placeholder payment method
                string paymentMethodId = "pm_default";
                
                try
                {
                    // Process payment for the invoice
                    var paymentResult = await _mediator.Send(new ProcessPaymentCommand
                    {
                        InvoiceId = createdInvoice.Id,
                        PaymentMethodId = paymentMethodId,
                        IsRetry = false,
                        RetryAttempt = 0
                    }, cancellationToken);
                    
                    _logger.LogInformation("Payment processed successfully for invoice {InvoiceId}", createdInvoice.Id);
                    
                    // If payment succeeded, renew the subscription
                    if (paymentResult.Status == "Success")
                    {
                        await RenewSubscriptionAsync(subscription, plan, request.InvoiceDate);
                    }
                    else
                    {
                        // If payment failed, mark subscription as expired (grace period)
                        await ExpireSubscriptionAsync(subscription);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing payment for invoice {InvoiceId}", createdInvoice.Id);
                    // Payment failed, mark subscription as expired (grace period)
                    await ExpireSubscriptionAsync(subscription);
                }
            }

            // Raise domain event
            await _mediator.Publish(new InvoiceGeneratedEvent
            {
                TenantId = subscription.TenantId,
                InvoiceId = createdInvoice.Id,
                SubscriptionId = subscription.Id,
                Amount = totalAmount,
                DueDate = createdInvoice.DueDate
            }, cancellationToken);

            return new InvoiceDto
            {
                Id = createdInvoice.Id,
                TenantId = createdInvoice.TenantId,
                SubscriptionId = createdInvoice.SubscriptionId,
                Amount = createdInvoice.Amount,
                InvoiceDate = createdInvoice.InvoiceDate,
                DueDate = createdInvoice.DueDate,
                IsPaid = createdInvoice.IsPaid,
                Status = createdInvoice.Status
            };
        }

        private async Task RenewSubscriptionAsync(Subscription subscription, Plan plan, DateTime renewalDate)
        {
            _logger.LogInformation("Renewing subscription {SubscriptionId}", subscription.Id);

            // Extend the subscription for another billing period
            subscription.StartDate = renewalDate;
            subscription.EndDate = renewalDate.AddMonths(1);
            subscription.Status = "Active"; // Ensure it's active

            // Update the subscription in the database
            await _subscriptionRepository.UpdateAsync(subscription);

            _logger.LogInformation("Subscription {SubscriptionId} renewed successfully. New end date: {EndDate}", 
                subscription.Id, subscription.EndDate);
        }
        
        private async Task ExpireSubscriptionAsync(Subscription subscription)
        {
            _logger.LogInformation("Expiring subscription {SubscriptionId}", subscription.Id);

            // Mark the subscription as expired (grace period before cancellation)
            subscription.Status = "Expired";

            // Update the subscription in the database
            await _subscriptionRepository.UpdateAsync(subscription);

            _logger.LogInformation("Subscription {SubscriptionId} marked as expired", subscription.Id);
        }
    }
}
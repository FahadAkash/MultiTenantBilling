using MediatR;
using Microsoft.Extensions.Logging;
using MultiTenantBilling.Application.Commands;
using MultiTenantBilling.Application.DTOs;
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Domain.Events;
using MultiTenantBilling.Infrastructure.Repositories;

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
    }
}


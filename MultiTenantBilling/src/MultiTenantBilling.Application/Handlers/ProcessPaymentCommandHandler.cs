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
    /// Handler for processing payments
    /// </summary>
    public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, PaymentDto>
    {
        private readonly ITenantRepository<Invoice> _invoiceRepository;
        private readonly ITenantRepository<Payment> _paymentRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<ProcessPaymentCommandHandler> _logger;

        public ProcessPaymentCommandHandler(
            ITenantRepository<Invoice> invoiceRepository,
            ITenantRepository<Payment> paymentRepository,
            IMediator mediator,
            ILogger<ProcessPaymentCommandHandler> logger)
        {
            _invoiceRepository = invoiceRepository;
            _paymentRepository = paymentRepository;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<PaymentDto> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing payment for invoice {InvoiceId}, Retry: {IsRetry}, Attempt: {RetryAttempt}",
                request.InvoiceId, request.IsRetry, request.RetryAttempt);

            var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId);
            if (invoice == null)
            {
                throw new InvalidOperationException("Invoice not found");
            }

            if (invoice.IsPaid)
            {
                throw new InvalidOperationException("Invoice is already paid");
            }

            // TODO: Integrate with actual payment gateway (Stripe, etc.)
            // For now, simulate payment processing
            bool paymentSucceeded = SimulatePaymentProcessing(request.PaymentMethodId);

            var payment = new Payment
            {
                TenantId = invoice.TenantId,
                InvoiceId = invoice.Id,
                Amount = invoice.Amount,
                PaymentDate = DateTime.UtcNow,
                Method = request.PaymentMethodId,
                Status = paymentSucceeded ? "Success" : "Failed",
                TransactionId = Guid.NewGuid().ToString()
            };

            var createdPayment = await _paymentRepository.AddAsync(payment);

            if (paymentSucceeded)
            {
                // Update invoice status
                invoice.IsPaid = true;
                invoice.Status = "Paid";
                await _invoiceRepository.UpdateAsync(invoice);

                // Raise success event
                await _mediator.Publish(new PaymentSucceededEvent
                {
                    TenantId = invoice.TenantId,
                    InvoiceId = invoice.Id,
                    PaymentId = createdPayment.Id,
                    Amount = invoice.Amount,
                    PaymentMethodId = request.PaymentMethodId
                }, cancellationToken);
            }
            else
            {
                // Raise failure event
                await _mediator.Publish(new PaymentFailedEvent
                {
                    TenantId = invoice.TenantId,
                    InvoiceId = invoice.Id,
                    Amount = invoice.Amount,
                    FailureReason = "Payment processing failed",
                    RetryAttempt = request.RetryAttempt
                }, cancellationToken);
            }

            return new PaymentDto
            {
                Id = createdPayment.Id,
                InvoiceId = createdPayment.InvoiceId,
                Amount = createdPayment.Amount,
                PaymentDate = createdPayment.PaymentDate,
                Status = createdPayment.Status,
                TransactionId = createdPayment.TransactionId
            };
        }

        private bool SimulatePaymentProcessing(string paymentMethodId)
        {
            // TODO: Replace with actual Stripe/payment gateway integration
            // For now, simulate 90% success rate
            return new Random().Next(1, 11) <= 9;
        }
    }
}


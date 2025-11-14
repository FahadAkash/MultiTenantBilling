using MediatR;
using Microsoft.Extensions.Logging;
using MultiTenantBilling.Application.Commands;
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Repositories;

namespace MultiTenantBilling.Application.Handlers
{
    /// <summary>
    /// Handler for retrying failed payments
    /// </summary>
    public class RetryFailedPaymentCommandHandler : IRequestHandler<RetryFailedPaymentCommand, bool>
    {
        private readonly ITenantRepository<Invoice> _invoiceRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<RetryFailedPaymentCommandHandler> _logger;

        public RetryFailedPaymentCommandHandler(
            ITenantRepository<Invoice> invoiceRepository,
            IMediator mediator,
            ILogger<RetryFailedPaymentCommandHandler> logger)
        {
            _invoiceRepository = invoiceRepository;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<bool> Handle(RetryFailedPaymentCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrying payment for invoice {InvoiceId}, attempt {RetryAttempt}",
                request.InvoiceId, request.RetryAttempt);

            var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId);
            if (invoice == null)
            {
                throw new InvalidOperationException("Invoice not found");
            }

            if (invoice.IsPaid)
            {
                _logger.LogInformation("Invoice {InvoiceId} is already paid", request.InvoiceId);
                return true;
            }

            // Get payment method from tenant (stored when subscription was created)
            // TODO: Retrieve actual payment method from tenant/subscription
            string paymentMethodId = "pm_default"; // Placeholder

            // Process payment
            try
            {
                var payment = await _mediator.Send(new ProcessPaymentCommand
                {
                    InvoiceId = request.InvoiceId,
                    PaymentMethodId = paymentMethodId,
                    IsRetry = true,
                    RetryAttempt = request.RetryAttempt
                }, cancellationToken);

                return payment.Status == "Success";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying payment for invoice {InvoiceId}", request.InvoiceId);
                return false;
            }
        }
    }
}


using MediatR;
using Microsoft.Extensions.Logging;
using MultiTenantBilling.Application.Queries;
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MultiTenantBilling.Application.Handlers
{
    /// <summary>
    /// Handler for getting failed payments that need retry
    /// </summary>
    public class GetFailedPaymentsQueryHandler : IRequestHandler<GetFailedPaymentsQuery, List<FailedPaymentDto>>
    {
        private readonly ITenantRepository<Payment> _paymentRepository;
        private readonly ILogger<GetFailedPaymentsQueryHandler> _logger;

        public GetFailedPaymentsQueryHandler(
            ITenantRepository<Payment> paymentRepository,
            ILogger<GetFailedPaymentsQueryHandler> logger)
        {
            _paymentRepository = paymentRepository;
            _logger = logger;
        }

        public async Task<List<FailedPaymentDto>> Handle(GetFailedPaymentsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting failed payments for retry");

            // Get all payments across all tenants
            var allPayments = await _paymentRepository.GetAllEntitiesAsync();
            
            // Filter for failed payments that need retry
            var failedPayments = allPayments
                .Where(p => p.Status == "Failed" || p.Status == "Pending")
                .Where(p => p.RetryAttempt < (request.MaxRetryAttempts ?? 3))
                .Where(p => p.PaymentDate > (request.OlderThan ?? DateTime.UtcNow.AddDays(-30)))
                .Select(payment => new FailedPaymentDto
                {
                    InvoiceId = payment.InvoiceId,
                    TenantId = payment.TenantId,
                    Amount = payment.Amount,
                    RetryAttempt = payment.RetryAttempt,
                    LastAttemptDate = payment.PaymentDate,
                    FailureReason = payment.FailureReason ?? "Unknown"
                })
                .ToList();

            return failedPayments;
        }
    }
}
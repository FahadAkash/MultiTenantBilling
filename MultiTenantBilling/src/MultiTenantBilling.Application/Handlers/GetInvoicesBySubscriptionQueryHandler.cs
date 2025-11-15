using MediatR;
using Microsoft.Extensions.Logging;
using MultiTenantBilling.Application.DTOs;
using MultiTenantBilling.Application.Queries;
using MultiTenantBilling.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MultiTenantBilling.Application.Handlers
{
    /// <summary>
    /// Handler for getting invoices for a specific subscription
    /// </summary>
    public class GetInvoicesBySubscriptionQueryHandler : IRequestHandler<GetInvoicesBySubscriptionQuery, IEnumerable<InvoiceDto>>
    {
        private readonly ITenantRepository<Domain.Entities.Invoice> _invoiceRepository;
        private readonly ILogger<GetInvoicesBySubscriptionQueryHandler> _logger;

        public GetInvoicesBySubscriptionQueryHandler(
            ITenantRepository<Domain.Entities.Invoice> invoiceRepository,
            ILogger<GetInvoicesBySubscriptionQueryHandler> logger)
        {
            _invoiceRepository = invoiceRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<InvoiceDto>> Handle(GetInvoicesBySubscriptionQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting invoices for subscription {SubscriptionId}", request.SubscriptionId);

            var invoices = await _invoiceRepository.GetByTenantIdAsync(request.TenantId);
            var subscriptionInvoices = invoices
                .Where(i => i.SubscriptionId == request.SubscriptionId)
                .Select(invoice => new InvoiceDto
                {
                    Id = invoice.Id,
                    TenantId = invoice.TenantId,
                    SubscriptionId = invoice.SubscriptionId,
                    Amount = invoice.Amount,
                    InvoiceDate = invoice.InvoiceDate,
                    DueDate = invoice.DueDate,
                    Status = invoice.Status,
                    IsPaid = invoice.IsPaid
                })
                .ToList();

            return subscriptionInvoices;
        }
    }
}
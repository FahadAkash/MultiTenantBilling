using MultiTenantBilling.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace MultiTenantBilling.Application.Services
{
    public interface IInvoiceService
    {
        Task<InvoiceDto> GenerateInvoiceAsync(Guid subscriptionId, DateTime invoiceDate);
        Task<InvoiceDto> GetInvoiceAsync(Guid invoiceId);
        Task<bool> MarkInvoiceAsPaidAsync(Guid invoiceId);
        Task<decimal> CalculateTotalAsync(Guid invoiceId);
    }
}
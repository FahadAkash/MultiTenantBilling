using MultiTenantBilling.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace MultiTenantBilling.Application.Services
{
    public interface IPaymentService
    {
        Task<PaymentDto> ProcessPaymentAsync(Guid invoiceId, string paymentMethodId);
        Task<PaymentDto> GetPaymentAsync(Guid paymentId);
        Task<bool> RefundPaymentAsync(Guid paymentId);
    }
}
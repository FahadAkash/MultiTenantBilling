using MultiTenantBilling.Application.DTOs;

namespace MultiTenantBilling.Application.Queries
{
    /// <summary>
    /// Query to get payment history for a tenant or invoice
    /// </summary>
    public class GetPaymentHistoryQuery : IQuery<List<PaymentDto>>
    {
        public Guid? TenantId { get; set; }
        public Guid? InvoiceId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}


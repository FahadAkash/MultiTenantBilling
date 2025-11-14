using MultiTenantBilling.Application.DTOs;

namespace MultiTenantBilling.Application.Queries
{
    /// <summary>
    /// Query to get invoice history for a tenant
    /// </summary>
    public class GetInvoiceHistoryQuery : IQuery<List<InvoiceDto>>
    {
        public Guid TenantId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}


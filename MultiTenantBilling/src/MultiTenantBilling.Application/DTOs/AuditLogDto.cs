using System;

namespace MultiTenantBilling.Application.DTOs
{
    public class AuditLogDto
    {
        public Guid Id { get; set; }
        public Guid? TenantId { get; set; }
        public string UserEmail { get; set; } = default!;
        public string Action { get; set; } = default!;
        public string EntityName { get; set; } = default!;
        public string EntityId { get; set; } = default!;
        public string Changes { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
    }
}
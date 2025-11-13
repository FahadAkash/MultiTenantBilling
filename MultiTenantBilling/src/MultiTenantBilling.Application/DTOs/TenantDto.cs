using System;
using System.Collections.Generic;

namespace MultiTenantBilling.Application.DTOs
{
    public class TenantDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Subdomain { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Status { get; set; } = default!;
        public IEnumerable<SubscriptionDto>? Subscriptions { get; set; }
    }
}
using System;

namespace MultiTenantBilling.Application.DTOs
{
    public class PlanDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal MonthlyPrice { get; set; }
        public int MaxUsers { get; set; }
        public int MaxStorageGb { get; set; }
    }
}
using MultiTenantBilling.Application.DTOs;

namespace MultiTenantBilling.Application.Commands
{
    /// <summary>
    /// Command to create a new subscription
    /// </summary>
    public class CreateSubscriptionCommand : ICommand<SubscriptionDto>
    {
        public Guid TenantId { get; set; }
        public Guid PlanId { get; set; }
        public DateTime StartDate { get; set; }
        public string? PaymentMethodId { get; set; }
    }
}


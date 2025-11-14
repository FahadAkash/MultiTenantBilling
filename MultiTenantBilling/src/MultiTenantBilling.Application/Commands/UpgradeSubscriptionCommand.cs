using MultiTenantBilling.Application.DTOs;

namespace MultiTenantBilling.Application.Commands
{
    /// <summary>
    /// Command to upgrade/downgrade a subscription with proration
    /// </summary>
    public class UpgradeSubscriptionCommand : ICommand<SubscriptionDto>
    {
        public Guid SubscriptionId { get; set; }
        public Guid NewPlanId { get; set; }
        public bool ChargeImmediately { get; set; } = true;
    }
}


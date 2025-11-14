namespace MultiTenantBilling.Application.Commands
{
    /// <summary>
    /// Command to suspend a subscription due to payment failure
    /// </summary>
    public class SuspendSubscriptionCommand : ICommand<bool>
    {
        public Guid SubscriptionId { get; set; }
        public string Reason { get; set; } = default!;
    }
}


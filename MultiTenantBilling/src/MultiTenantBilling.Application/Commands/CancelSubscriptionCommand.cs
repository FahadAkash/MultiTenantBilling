namespace MultiTenantBilling.Application.Commands
{
    /// <summary>
    /// Command to cancel a subscription
    /// </summary>
    public class CancelSubscriptionCommand : ICommand<bool>
    {
        public Guid SubscriptionId { get; set; }
        public string? Reason { get; set; }
        public bool IssueRefund { get; set; } = false;
    }
}


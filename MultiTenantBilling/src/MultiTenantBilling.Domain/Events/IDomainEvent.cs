using MediatR;

namespace MultiTenantBilling.Domain.Events
{
    /// <summary>
    /// Marker interface for domain events
    /// </summary>
    public interface IDomainEvent : INotification
    {
        DateTime OccurredOn { get; }
    }
}
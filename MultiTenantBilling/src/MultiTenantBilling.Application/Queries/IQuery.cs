using MediatR;

namespace MultiTenantBilling.Application.Queries
{
    /// <summary>
    /// Marker interface for queries (read operations)
    /// </summary>
    public interface IQuery<TResponse> : IRequest<TResponse>
    {
    }
}


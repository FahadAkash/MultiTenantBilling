using MediatR;

namespace MultiTenantBilling.Application.Commands
{
    /// <summary>
    /// Marker interface for commands (write operations)
    /// </summary>
    public interface ICommand : IRequest
    {
    }

    /// <summary>
    /// Marker interface for commands that return a result
    /// </summary>
    public interface ICommand<TResponse> : IRequest<TResponse>
    {
    }
}


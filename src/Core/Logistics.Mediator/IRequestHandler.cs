namespace Logistics.Mediator;

/// <summary>
/// Handles one request type. Registering two handlers for the same request throws from
/// <c>AddMediator</c>.
/// </summary>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

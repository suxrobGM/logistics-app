namespace Logistics.Mediator;

/// <summary>
/// Handles a single <typeparamref name="TRequest" />. Exactly one handler may be registered per
/// request type; <c>AddMediator</c> throws at registration time if two are found.
/// </summary>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

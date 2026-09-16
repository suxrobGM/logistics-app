namespace Logistics.Mediator;

/// <summary>
/// A request handled by exactly one <see cref="IRequestHandler{TRequest,TResponse}" />.
/// </summary>
public interface IRequest<TResponse>;

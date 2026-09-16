namespace Logistics.Mediator;

/// <summary>
/// A request handled by exactly one <see cref="IRequestHandler{TRequest,TResponse}" />.
/// </summary>
/// <remarks>
/// <typeparamref name="TResponse" /> is covariant so that the compiler can infer it from a
/// concrete request type at a <c>Send</c> call site.
/// </remarks>
public interface IRequest<out TResponse> : IBaseRequest;

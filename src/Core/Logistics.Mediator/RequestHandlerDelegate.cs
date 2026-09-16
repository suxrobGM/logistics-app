namespace Logistics.Mediator;

/// <summary>
/// The next step in a request pipeline: the following behaviour, or the handler itself.
/// </summary>
/// <remarks>The token is required rather than optional so a behaviour cannot silently drop it.</remarks>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>(CancellationToken cancellationToken);

using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Abstractions;

/// <summary>
///     Application-specific MediatR handler that **enforces** a result pattern.
/// </summary>
/// <remarks>
///     This interface extends <see cref="MediatR.IRequestHandler{TRequest, TResponse}" /> and constrains:
///     <list type="bullet">
///         <item>
///             <description><typeparamref name="TRequest" /> must be a MediatR <see cref="IRequest{TResponse}" />.</description>
///         </item>
///         <item>
///             <description>
///                 <typeparamref name="TResponse" /> must implement <see cref="IResult" /> and have a
///                 parameterless constructor.
///             </description>
///         </item>
///     </list>
///     Use it to ensure all handlers in the Application layer return standard <c>Result</c>/<c>Result&lt;T&gt;</c>.
/// </remarks>
/// <typeparam name="TRequest">
///     The request type handled by this handler. Must implement <see cref="IAppRequest{TResponse}" />.
/// </typeparam>
/// <typeparam name="TResponse">
///     The response type produced by the handler. Must implement <see cref="IResult" /> and provide a parameterless
///     constructor.
/// </typeparam>
public interface IAppRequestHandler<in TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IAppRequest<TResponse>
    where TResponse : IResult, new()
{
    /// <summary>
    ///     Handles the request and returns an application-standard result.
    /// </summary>
    /// <param name="req">The request instance.</param>
    /// <param name="ct">A token to observe while awaiting the operation.</param>
    /// <returns>A task that resolves to a <typeparamref name="TResponse" /> implementing <see cref="IResult" />.</returns>
    new Task<TResponse> Handle(TRequest req, CancellationToken ct);
}

using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Abstractions;

/// <summary>
///     Application request that is guaranteed to produce an <see cref="IResult" />.
///     A request can be a command or a query.
/// </summary>
/// <typeparam name="TResponse">
///     The response type returned by the pipeline. Must implement <see cref="IResult" /> and have a parameterless ctor.
/// </typeparam>
public interface IAppRequest<out TResponse> : IRequest<TResponse>
    where TResponse : IResult, new();

/// <summary>
///     Application request that returns a non-generic <see cref="Result" />.
/// </summary>
public interface IAppRequest : IAppRequest<Result>;

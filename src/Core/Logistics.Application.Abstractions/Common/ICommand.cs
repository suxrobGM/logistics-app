using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Abstractions.Common;

/// <summary>
/// Marker for application commands (state-mutating requests). The pipeline wraps every
/// <see cref="ICommand{TResponse}"/> in a Unit-of-Work transaction unless the handler is
/// annotated with <see cref="NoAutoTransactionAttribute"/>.
/// </summary>
public interface ICommand<TResponse> : IRequest<TResponse>
    where TResponse : IResult, new();

/// <summary>
/// Marker for commands that return a non-generic <see cref="Result"/>.
/// </summary>
public interface ICommand : ICommand<Result>;

using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Abstractions.Common;

/// <summary>
/// Marker for application commands (state-mutating requests). Commands flow through the full
/// MediatR pipeline (Logging, UnhandledException, Validation, FeatureCheck) and are responsible
/// for calling <c>SaveChangesAsync</c> on the appropriate <see cref="Logistics.Domain.Persistence.IUnitOfWork{T}"/>.
/// </summary>
public interface ICommand<TResponse> : IRequest<TResponse>
    where TResponse : IResult, new();

/// <summary>
/// Marker for commands that return a non-generic <see cref="Result"/>.
/// </summary>
public interface ICommand : ICommand<Result>;

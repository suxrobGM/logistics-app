using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Abstractions.Common;

/// <summary>
/// A state-mutating request. Commands run through the Validation and FeatureCheck behaviours, and
/// are responsible for calling <c>SaveChangesAsync</c> on the appropriate
/// <see cref="Logistics.Domain.Persistence.IUnitOfWork{T}" />.
/// </summary>
public interface ICommand<TResponse> : IRequest<TResponse>
    where TResponse : Result, new();

/// <summary>A command that returns a non-generic <see cref="Result" />.</summary>
public interface ICommand : ICommand<Result>;

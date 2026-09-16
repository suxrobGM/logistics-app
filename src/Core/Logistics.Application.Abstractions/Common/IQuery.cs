using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Abstractions.Common;

/// <summary>
/// A read request. Handlers must not mutate state: defer audit and bookkeeping writes through
/// <see cref="Logistics.Application.Abstractions.BackgroundJobs.ICommandEnqueuer" />, or promote
/// the operation to an <see cref="ICommand{TResponse}" />.
/// </summary>
public interface IQuery<TResponse> : IRequest<TResponse>
    where TResponse : Result, new();

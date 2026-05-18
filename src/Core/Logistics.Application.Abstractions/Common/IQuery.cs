using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Abstractions.Common;

/// <summary>
/// Marker for application queries (read requests). Queries flow through the full MediatR
/// pipeline (Logging, UnhandledException, Validation, FeatureCheck). Handlers must not
/// mutate state — defer audit/bookkeeping writes via
/// <see cref="Logistics.Application.Abstractions.BackgroundJobs.ICommandEnqueuer"/>, or
/// promote the operation to an <see cref="ICommand{TResponse}"/>.
/// </summary>
public interface IQuery<TResponse> : IRequest<TResponse>
    where TResponse : IResult, new();

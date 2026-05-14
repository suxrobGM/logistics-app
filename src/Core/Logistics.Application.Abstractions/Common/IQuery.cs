using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Abstractions.Common;

/// <summary>
/// Marker for application queries (read requests). Queries flow through the full MediatR
/// pipeline (Logging, UnhandledException, Validation, FeatureCheck). Handlers should not
/// mutate state; the rare CQS-violating queries that do are responsible for their own
/// <c>SaveChangesAsync</c> calls.
/// </summary>
public interface IQuery<TResponse> : IRequest<TResponse>
    where TResponse : IResult, new();

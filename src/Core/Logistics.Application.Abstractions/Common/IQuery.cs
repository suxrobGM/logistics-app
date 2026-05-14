using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Abstractions.Common;

/// <summary>
/// Marker for application queries (read requests). Queries flow through Logging,
/// UnhandledException, Validation, and FeatureCheck behaviours but skip TransactionBehaviour —
/// queries that intentionally mutate state must declare <see cref="NoAutoTransactionAttribute"/>
/// and remain responsible for their own SaveChangesAsync calls.
/// </summary>
public interface IQuery<TResponse> : IRequest<TResponse>
    where TResponse : IResult, new();

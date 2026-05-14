using Logistics.Shared.Models;

namespace Logistics.Application.Abstractions.Common;

/// <summary>
/// Marker for commands that legitimately mutate both the master and tenant databases in a
/// single request (e.g. AcceptInvitation). TransactionBehaviour begins and commits a
/// transaction on each UoW sequentially, but does NOT provide cross-database atomicity —
/// a partial failure can leave one side committed and the other rolled back. Handlers must
/// design for partial-failure recovery (idempotent retry, reconciliation jobs, etc.) or
/// opt out via <see cref="NoAutoTransactionAttribute"/> and manage transactions manually.
/// </summary>
public interface ICrossDatabaseCommand<TResponse> : ICommand<TResponse>
    where TResponse : IResult, new();

/// <inheritdoc cref="ICrossDatabaseCommand{TResponse}"/>
public interface ICrossDatabaseCommand : ICrossDatabaseCommand<Result>;

using Logistics.Shared.Models;

namespace Logistics.Application.Abstractions.Common;

/// <summary>
/// Marker for commands that mutate the master database (tenants, subscriptions, super-admin
/// data) instead of the per-tenant database. The pipeline's TransactionBehaviour selects
/// IMasterUnitOfWork for these commands, so they do not require an active tenant context.
/// </summary>
public interface IMasterCommand<TResponse> : ICommand<TResponse>
    where TResponse : IResult, new();

/// <summary>
/// Marker for master-DB commands that return a non-generic <see cref="Result"/>.
/// </summary>
public interface IMasterCommand : IMasterCommand<Result>;

using System.Reflection;
using Logistics.Application.Abstractions.Common;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Behaviours;

/// <summary>
/// Wraps every <see cref="ICommand{TResponse}"/> in a Unit-of-Work transaction. Commits on
/// success, rolls back on exception or on <see cref="IResult.IsSuccess"/> = false. Selects
/// the master UoW for <see cref="IMasterCommand{TResponse}"/>, both UoWs (no atomicity)
/// for <see cref="ICrossDatabaseCommand{TResponse}"/>, and the tenant UoW otherwise.
/// Handlers annotated with <see cref="NoAutoTransactionAttribute"/> are skipped.
/// </summary>
public sealed class TransactionBehaviour<TRequest, TResponse>(
    IServiceProvider serviceProvider,
    ILogger<TransactionBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
    where TResponse : IResult, new()
{
    // Evaluated once per closed generic instantiation — no per-call reflection.
    private static readonly bool OptOut =
        typeof(TRequest).GetCustomAttribute<NoAutoTransactionAttribute>() is not null;

    private static readonly bool IsMasterCommand =
        typeof(IMasterCommand<TResponse>).IsAssignableFrom(typeof(TRequest));

    private static readonly bool IsCrossDatabaseCommand =
        typeof(ICrossDatabaseCommand<TResponse>).IsAssignableFrom(typeof(TRequest));

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (OptOut)
        {
            return await next(cancellationToken);
        }

        var begins = new List<Func<CancellationToken, Task>>(2);
        var commits = new List<Func<CancellationToken, Task>>(2);
        var rollbacks = new List<Func<CancellationToken, Task>>(2);

        if (IsCrossDatabaseCommand)
        {
            logger.LogWarning(
                "Command {Request} is a cross-database command — no atomicity guarantee across master/tenant DBs.",
                typeof(TRequest).Name);

            var masterUow = serviceProvider.GetRequiredService<IMasterUnitOfWork>();
            var tenantUow = serviceProvider.GetRequiredService<ITenantUnitOfWork>();
            AddUow(masterUow.BeginTransactionAsync, masterUow.CommitTransactionAsync, masterUow.RollbackTransactionAsync);
            AddUow(tenantUow.BeginTransactionAsync, tenantUow.CommitTransactionAsync, tenantUow.RollbackTransactionAsync);
        }
        else if (IsMasterCommand)
        {
            var masterUow = serviceProvider.GetRequiredService<IMasterUnitOfWork>();
            AddUow(masterUow.BeginTransactionAsync, masterUow.CommitTransactionAsync, masterUow.RollbackTransactionAsync);
        }
        else
        {
            var tenantUow = serviceProvider.GetRequiredService<ITenantUnitOfWork>();
            AddUow(tenantUow.BeginTransactionAsync, tenantUow.CommitTransactionAsync, tenantUow.RollbackTransactionAsync);
        }

        foreach (var begin in begins)
        {
            await begin(cancellationToken);
        }

        TResponse result;
        try
        {
            result = await next(cancellationToken);
        }
        catch
        {
            await SafeRollbackAll(rollbacks, cancellationToken);
            throw;
        }

        if (result is IResult r && !r.IsSuccess)
        {
            await SafeRollbackAll(rollbacks, cancellationToken);
            return result;
        }

        foreach (var commit in commits)
        {
            await commit(cancellationToken);
        }

        return result;

        void AddUow(
            Func<CancellationToken, Task> begin,
            Func<CancellationToken, Task> commit,
            Func<CancellationToken, Task> rollback)
        {
            begins.Add(begin);
            commits.Add(commit);
            rollbacks.Add(rollback);
        }
    }

    private async Task SafeRollbackAll(
        List<Func<CancellationToken, Task>> rollbacks,
        CancellationToken cancellationToken)
    {
        foreach (var rollback in rollbacks)
        {
            try
            {
                await rollback(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Rollback failed for {Request}; continuing.", typeof(TRequest).Name);
            }
        }
    }
}

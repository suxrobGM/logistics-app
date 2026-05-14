using Logistics.Application.Abstractions.Common;
using Logistics.Shared.Models;

namespace Logistics.Application.Abstractions.BackgroundJobs;

/// <summary>
/// Schedules a command to run in the background via a durable job system (e.g., Hangfire).
/// The command is dispatched through MediatR in a fresh DI scope, so its handler runs with
/// its own <c>IUnitOfWork</c> instances.
/// <para>
/// Commands enqueued through this interface must be plain serialisable records — no
/// <c>CancellationToken</c> fields, no non-serialisable dependencies. The job system will
/// JSON-serialise the command and revive it on the worker.
/// </para>
/// </summary>
public interface ICommandEnqueuer
{
    /// <summary>
    /// Enqueues a command that returns a non-generic <see cref="Result"/>.
    /// </summary>
    void Enqueue<TCommand>(TCommand command) where TCommand : ICommand;

    /// <summary>
    /// Enqueues a command that returns <typeparamref name="TResponse"/>. The response is
    /// discarded by the background runner; use this overload when you only care that the
    /// command executes, not what it returned.
    /// </summary>
    void Enqueue<TCommand, TResponse>(TCommand command)
        where TCommand : ICommand<TResponse>
        where TResponse : IResult, new();
}

using Logistics.Application.Abstractions.Common;

namespace Logistics.Application.Abstractions.BackgroundJobs;

/// <summary>
/// Schedules a command to run in the background via a durable job system. The command is
/// dispatched in a fresh DI scope, so its handler runs with its own <c>IUnitOfWork</c> instances.
/// </summary>
/// <remarks>
/// Enqueued commands must be plain serialisable records: no <c>CancellationToken</c> fields and no
/// non-serialisable dependencies. The job system JSON-serialises the command and revives it on the
/// worker.
/// </remarks>
public interface ICommandEnqueuer
{
    void Enqueue<TCommand>(TCommand command) where TCommand : ICommand;
}

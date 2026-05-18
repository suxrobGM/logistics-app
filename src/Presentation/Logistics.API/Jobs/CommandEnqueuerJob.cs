using Hangfire;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Abstractions.Common;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.API.Jobs;

/// <summary>
///     Hangfire job that dispatches a command via MediatR inside its own DI scope.
///     Wired up by <see cref="HangfireCommandEnqueuer"/>.
/// </summary>
public class CommandEnqueuerJob(
    IServiceScopeFactory scopeFactory,
    ILogger<CommandEnqueuerJob> logger)
{
    public async Task RunAsync(object command, CancellationToken ct)
    {
        if (command is not IBaseRequest request)
        {
            logger.LogWarning(
                "CommandEnqueuerJob received a payload of type {Type} that is not an IBaseRequest; skipping.",
                command.GetType().FullName);
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        try
        {
            await mediator.Send(request, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Background command {Command} failed.", command.GetType().Name);
            throw;
        }
    }
}

/// <summary>
///     Hangfire-backed implementation of <see cref="ICommandEnqueuer"/>. Serialises the
///     command record and dispatches it via MediatR in a fresh DI scope.
/// </summary>
public sealed class HangfireCommandEnqueuer(IBackgroundJobClient jobClient) : ICommandEnqueuer
{
    public void Enqueue<TCommand>(TCommand command) where TCommand : ICommand
    {
        EnqueueInternal(command);
    }

    public void Enqueue<TCommand, TResponse>(TCommand command)
        where TCommand : ICommand<TResponse>
        where TResponse : Logistics.Shared.Models.IResult, new()
    {
        EnqueueInternal(command);
    }

    private void EnqueueInternal(object command)
    {
        jobClient.Enqueue<CommandEnqueuerJob>(job => job.RunAsync(command, CancellationToken.None));
    }
}

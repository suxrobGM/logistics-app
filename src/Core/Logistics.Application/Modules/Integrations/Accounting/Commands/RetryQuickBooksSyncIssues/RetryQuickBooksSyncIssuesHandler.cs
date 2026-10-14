using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Accounting.Commands;

internal sealed class RetryQuickBooksSyncIssuesHandler(ITenantUnitOfWork tenantUow, ICommandEnqueuer commandEnqueuer)
    : IRequestHandler<RetryQuickBooksSyncIssuesCommand, Result>
{
    public async Task<Result> Handle(RetryQuickBooksSyncIssuesCommand req, CancellationToken ct)
    {
        var failed = await tenantUow.Repository<QboEntityMapping>().GetListAsync(
            m => m.SyncStatus == QboSyncStatus.Failed && (req.Id == null || m.Id == req.Id), ct);

        if (req.Id is not null && failed.Count == 0)
        {
            return Result.Fail("That sync error is already resolved.");
        }

        foreach (var mapping in failed)
        {
            mapping.LastAttemptAt = null;
        }

        await tenantUow.SaveChangesAsync(ct);
        commandEnqueuer.Enqueue(new RunAccountingSyncCommand { TenantId = tenantUow.GetCurrentTenant().Id });
        return Result.Ok();
    }
}

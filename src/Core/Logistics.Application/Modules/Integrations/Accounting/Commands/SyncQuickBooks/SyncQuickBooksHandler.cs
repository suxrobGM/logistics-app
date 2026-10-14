using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Accounting.Commands;

internal sealed class SyncQuickBooksHandler(ITenantUnitOfWork tenantUow, ICommandEnqueuer commandEnqueuer)
    : IRequestHandler<SyncQuickBooksCommand, Result>
{
    public Task<Result> Handle(SyncQuickBooksCommand req, CancellationToken ct)
    {
        commandEnqueuer.Enqueue(new RunAccountingSyncCommand { TenantId = tenantUow.GetCurrentTenant().Id });
        return Task.FromResult(Result.Ok());
    }
}

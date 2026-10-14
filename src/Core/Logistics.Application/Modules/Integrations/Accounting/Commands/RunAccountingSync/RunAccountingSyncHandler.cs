using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Integrations.Accounting.Services;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Accounting.Commands;

internal sealed class RunAccountingSyncHandler(ITenantUnitOfWork tenantUow, IAccountingSyncService syncService)
    : IRequestHandler<RunAccountingSyncCommand, Result>
{
    public async Task<Result> Handle(RunAccountingSyncCommand req, CancellationToken ct)
    {
        await tenantUow.SetCurrentTenantByIdAsync(req.TenantId);
        await syncService.SyncAsync(ct);
        return Result.Ok();
    }
}

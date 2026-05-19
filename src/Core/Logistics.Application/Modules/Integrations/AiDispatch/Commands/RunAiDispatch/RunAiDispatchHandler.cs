using Logistics.Application.Abstractions;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Abstractions.AiDispatch;

namespace Logistics.Application.Modules.Integrations.AiDispatch.Commands;

internal sealed class RunAiDispatchHandler(
    ICurrentUserService currentUser,
    ITenantUnitOfWork tenantUow,
    IBackgroundJobRunner<AiDispatchRequest> backgroundRunner) : IAppRequestHandler<RunAiDispatchCommand, Result<Guid>>
{
    public Task<Result<Guid>> Handle(RunAiDispatchCommand request, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();

        backgroundRunner.Enqueue(new AiDispatchRequest(
            TenantId: tenant.Id,
            Mode: request.Mode,
            TriggeredByUserId: currentUser.GetUserId(),
            Instructions: request.Instructions));

        return Task.FromResult(Result<Guid>.Ok(Guid.Empty));
    }
}

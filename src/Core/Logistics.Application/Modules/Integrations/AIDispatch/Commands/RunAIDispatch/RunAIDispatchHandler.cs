using Logistics.Application.Abstractions;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Abstractions.AIDispatch;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

internal sealed class RunAIDispatchHandler(
    ICurrentUserService currentUser,
    ITenantUnitOfWork tenantUow,
    IBackgroundJobRunner<AIDispatchRequest> backgroundRunner) : IAppRequestHandler<RunAIDispatchCommand, Result<Guid>>
{
    public Task<Result<Guid>> Handle(RunAIDispatchCommand request, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();

        backgroundRunner.Enqueue(new AIDispatchRequest(
            TenantId: tenant.Id,
            Mode: request.Mode,
            TriggeredByUserId: currentUser.GetUserId(),
            Instructions: request.Instructions));

        return Task.FromResult(Result<Guid>.Ok(Guid.Empty));
    }
}

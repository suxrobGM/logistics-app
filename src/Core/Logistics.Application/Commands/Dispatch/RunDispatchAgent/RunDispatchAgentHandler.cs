using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class RunDispatchAgentHandler(
    ICurrentUserService currentUser,
    ITenantUnitOfWork tenantUow,
    IBackgroundJobRunner<DispatchAgentRequest> backgroundRunner) : IAppRequestHandler<RunDispatchAgentCommand, Result<Guid>>
{
    public Task<Result<Guid>> Handle(RunDispatchAgentCommand request, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();

        backgroundRunner.Enqueue(new DispatchAgentRequest(
            TenantId: tenant.Id,
            Mode: request.Mode,
            TriggeredByUserId: currentUser.GetUserId(),
            Instructions: request.Instructions));

        return Task.FromResult(Result<Guid>.Ok(Guid.Empty));
    }
}

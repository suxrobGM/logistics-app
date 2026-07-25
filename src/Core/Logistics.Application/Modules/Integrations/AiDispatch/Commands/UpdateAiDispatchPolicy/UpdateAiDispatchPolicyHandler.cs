using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.AiDispatch.Commands;

internal sealed class UpdateAiDispatchPolicyHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser) : IAppRequestHandler<UpdateAiDispatchPolicyCommand, Result>
{
    public async Task<Result> Handle(UpdateAiDispatchPolicyCommand request, CancellationToken ct)
    {
        var repo = tenantUow.Repository<AiDispatchPolicy>();
        var policy = await repo.Query().FirstOrDefaultAsync(ct);

        // Created on demand so a dispatcher can write directives before the agent has learned anything.
        if (policy is null)
        {
            policy = new AiDispatchPolicy();
            await repo.AddAsync(policy, ct);
        }

        policy.EditManual(request.ManualContent, request.IsEnabled, currentUser.GetUserId());
        await tenantUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}

using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

internal sealed class UpdateAIDispatchPolicyHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser) : IAppRequestHandler<UpdateAIDispatchPolicyCommand, Result>
{
    public async Task<Result> Handle(UpdateAIDispatchPolicyCommand request, CancellationToken ct)
    {
        var repo = tenantUow.Repository<AIDispatchPolicy>();
        var policy = await repo.Query().FirstOrDefaultAsync(ct);

        // Created on demand: directives can be written before the agent has learned anything.
        if (policy is null)
        {
            policy = new AIDispatchPolicy();
            await repo.AddAsync(policy, ct);
        }

        policy.EditManual(request.ManualContent, request.IsEnabled, currentUser.GetUserId());
        await tenantUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}

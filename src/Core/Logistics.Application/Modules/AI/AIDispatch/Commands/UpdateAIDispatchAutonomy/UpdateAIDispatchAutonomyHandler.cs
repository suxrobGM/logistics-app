using Logistics.Application.Abstractions.Context;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.AI.AIDispatch.Commands;

internal sealed class UpdateAIDispatchAutonomyHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser) : IRequestHandler<UpdateAIDispatchAutonomyCommand, Result>
{
    public async Task<Result> Handle(UpdateAIDispatchAutonomyCommand request, CancellationToken ct)
    {
        var repo = tenantUow.Repository<AIDispatchPolicy>();
        var policy = await repo.Query().FirstOrDefaultAsync(ct);
        if (policy is null)
        {
            policy = new AIDispatchPolicy();
            await repo.AddAsync(policy, ct);
        }

        policy.SetAutoApprove(
            request.AutoApproveTypes, request.AutoApproveStreak, request.MinutesSavedPerDecision,
            currentUser.GetUserId());
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}

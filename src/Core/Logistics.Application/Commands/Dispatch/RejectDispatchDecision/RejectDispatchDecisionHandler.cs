using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class RejectDispatchDecisionHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser) : IAppRequestHandler<RejectDispatchDecisionCommand, Result>
{
    public async Task<Result> Handle(RejectDispatchDecisionCommand request, CancellationToken ct)
    {
        var decision = await tenantUow.Repository<DispatchDecision>()
            .GetByIdAsync(request.DecisionId);

        if (decision is null)
            return Result.Fail("Decision not found");

        if (decision.Status != DispatchDecisionStatus.Suggested)
            return Result.Fail("Decision is not in a suggested state");

        var userId = currentUser.GetUserId() ?? Guid.Empty;
        decision.Reject(userId, request.Reason);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}

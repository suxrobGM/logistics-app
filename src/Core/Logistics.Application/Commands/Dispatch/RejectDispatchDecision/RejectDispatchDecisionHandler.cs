using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Configuration;

namespace Logistics.Application.Commands;

internal sealed class RejectDispatchDecisionHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser,
    IConfiguration configuration) : IAppRequestHandler<RejectDispatchDecisionCommand, Result>
{
    public async Task<Result> Handle(RejectDispatchDecisionCommand request, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();
        var bypassGate = configuration.GetValue<bool>("Llm:BypassLlmGate");

        if (!bypassGate && tenant.Settings.LlmEnabled == false)
            return Result.Fail("AI dispatch is disabled for this tenant");

        var decision = await tenantUow.Repository<DispatchDecision>()
            .GetByIdAsync(request.DecisionId, ct);

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

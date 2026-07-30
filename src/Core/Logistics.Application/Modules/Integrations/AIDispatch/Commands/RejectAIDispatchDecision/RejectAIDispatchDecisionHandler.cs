using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.AI;
using Microsoft.Extensions.Options;
using Logistics.Application.Abstractions.CurrentUser;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

internal sealed class RejectAIDispatchDecisionHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser,
    IOptions<LlmOptions> llmOptions) : IAppRequestHandler<RejectAIDispatchDecisionCommand, Result>
{
    public async Task<Result> Handle(RejectAIDispatchDecisionCommand request, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();
        var bypassGate = llmOptions.Value.BypassLlmGate;

        if (!bypassGate && tenant.Settings.LlmEnabled == false)
            return Result.Fail("AI dispatch is disabled for this tenant");

        var decision = await tenantUow.Repository<AgentDecision>()
            .Query()
            .DispatchOnly()
            .FirstOrDefaultAsync(d => d.Id == request.DecisionId, ct);

        if (decision is null)
            return Result.Fail("Decision not found");

        if (decision.Status != AgentDecisionStatus.Suggested)
            return Result.Fail("Decision is not in a suggested state");

        var userId = currentUser.GetUserId() ?? Guid.Empty;
        decision.Reject(userId, request.Reason);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}

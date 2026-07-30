using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.AI;
using Microsoft.Extensions.Options;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Abstractions.AIDispatch;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

internal sealed class ApproveAIDispatchDecisionHandler(
    ITenantUnitOfWork tenantUow,
    IAgentToolExecutor toolExecutor,
    ICurrentUserService currentUser,
    IOptions<LlmOptions> llmOptions) : IAppRequestHandler<ApproveAIDispatchDecisionCommand, Result>
{
    public async Task<Result> Handle(ApproveAIDispatchDecisionCommand request, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();
        var bypassGate = llmOptions.Value.BypassLlmGate;

        if (!bypassGate && tenant.Settings.LlmEnabled == false)
            return Result.Fail("AI dispatch is disabled for this tenant");

        var decision = await tenantUow.Repository<AIDispatchDecision>()
            .Query()
            .DispatchOnly()
            .FirstOrDefaultAsync(d => d.Id == request.DecisionId, ct);

        if (decision is null)
            return Result.Fail("Decision not found");

        if (decision.Status != AIDispatchDecisionStatus.Suggested)
            return Result.Fail("Decision is not in a suggested state");

        var userId = currentUser.GetUserId() ?? Guid.Empty;
        decision.Approve(userId);

        // Execute the tool action
        try
        {
            var result = await toolExecutor.ExecuteToolAsync(
                decision.ToolName!, decision.ToolInput!, ct);
            decision.ToolOutput = result;
            decision.MarkExecuted();
            await tenantUow.SaveChangesAsync(ct);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            decision.MarkFailed(ex.Message);
            await tenantUow.SaveChangesAsync(ct);
            return Result.Fail($"Failed to execute decision: {ex.Message}");
        }
    }
}

using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class ApproveDispatchDecisionHandler(
    ITenantUnitOfWork tenantUow,
    IDispatchToolExecutor toolExecutor,
    ICurrentUserService currentUser) : IAppRequestHandler<ApproveDispatchDecisionCommand, Result>
{
    public async Task<Result> Handle(ApproveDispatchDecisionCommand request, CancellationToken ct)
    {
        var decision = await tenantUow.Repository<DispatchDecision>()
            .GetByIdAsync(request.DecisionId);

        if (decision is null)
            return Result.Fail("Decision not found");

        if (decision.Status != DispatchDecisionStatus.Suggested)
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

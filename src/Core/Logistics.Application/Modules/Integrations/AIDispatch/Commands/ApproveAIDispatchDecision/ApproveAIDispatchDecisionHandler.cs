using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.AI;
using Microsoft.Extensions.Options;
using Logistics.Application.Abstractions.CurrentUser;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

internal sealed class ApproveAIDispatchDecisionHandler(
    ITenantUnitOfWork tenantUow,
    IAgentToolExecutor toolExecutor,
    ICurrentUserService currentUser,
    IAIDispatchBroadcastService broadcastService,
    IOptions<LlmOptions> llmOptions) : IAppRequestHandler<ApproveAIDispatchDecisionCommand, Result>
{
    public async Task<Result> Handle(ApproveAIDispatchDecisionCommand request, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();
        var bypassGate = llmOptions.Value.BypassAIGate;

        if (!bypassGate && tenant.Settings.AIEnabled == false)
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
        decision.Approve(userId);

        // Execute the tool action
        string note;
        Result outcome;
        try
        {
            var result = await toolExecutor.ExecuteToolAsync(
                decision.ToolName!, decision.ToolInput!, ct);
            decision.ToolOutput = result;
            decision.MarkExecuted();
            note = $"Approved and executed: {decision.ToolName} - {Compact(result)}";
            outcome = Result.Ok();
        }
        catch (Exception ex)
        {
            decision.MarkFailed(ex.Message);
            note = $"Approved but failed to execute: {decision.ToolName} - {Compact(ex.Message)}";
            outcome = Result.Fail($"Failed to execute decision: {ex.Message}");
        }

        await AppendConversationNoteAsync(decision, note, ct);
        return outcome;
    }

    /// <summary>
    /// Old sessions may predate conversations - only note the transcript when this decision's
    /// session belongs to one.
    /// </summary>
    private async Task AppendConversationNoteAsync(AgentDecision decision, string note, CancellationToken ct)
    {
        if (decision.Session.ConversationId is not { } conversationId)
        {
            await tenantUow.SaveChangesAsync(ct);
            return;
        }

        var conversation = await tenantUow.Repository<AgentConversation>().GetByIdAsync(conversationId, ct);
        if (conversation is null)
        {
            await tenantUow.SaveChangesAsync(ct);
            return;
        }

        var message = conversation.AddTextMessage(AgentMessageRole.System, note);
        await tenantUow.Repository<AgentMessage>().AddAsync(message, ct);
        await tenantUow.SaveChangesAsync(ct);

        await broadcastService.BroadcastMessageAsync(tenantUow.GetCurrentTenant().Id, message.ToDto());
    }

    private static string Compact(string text) =>
        text.Length > 500 ? text[..500] : text;
}

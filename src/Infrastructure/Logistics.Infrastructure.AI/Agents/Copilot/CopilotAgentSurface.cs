using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions.AICopilot;
using Logistics.Application.Modules.IdentityAccess.Users.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.AI.Agents.Copilot;

/// <summary>
/// Today's copilot behavior as an <see cref="IAgentSurface"/>: a permission-scoped tool catalogue
/// built from the triggering user's own permissions, and broadcasts routed to the conversation
/// owner's private copilot hub group.
/// </summary>
internal sealed class CopilotAgentSurface(
    AICopilotConversationBuilder conversationBuilder,
    IAICopilotBroadcastService broadcastService,
    IUserPermissionService userPermissions) : IAgentSurface
{
    public AgentSessionType SessionType => AgentSessionType.Copilot;

    public async Task<AgentTurnSetup> PrepareAsync(
        AgentSession session,
        AgentConversation conversation,
        AgentTurnRequest request,
        LlmOptions config,
        CancellationToken ct)
    {
        var permissions = await ResolveCallerPermissionsAsync(request, ct);

        var llmConversation = await conversationBuilder.BuildAsync(session, conversation, permissions, config, ct);

        var toolContext = new ToolCallContext(
            CallerPermissions: permissions,
            DecisionBroadcastOverride: dto =>
                broadcastService.BroadcastDecisionAsync(request.TenantId, conversation.CreatedById, dto));

        return new AgentTurnSetup(llmConversation, toolContext);
    }

    public Task BroadcastMessageAsync(AgentTurnRequest request, AgentConversation conversation, AgentMessageDto message) =>
        broadcastService.BroadcastMessageAsync(request.TenantId, conversation.CreatedById, message);

    public Task BroadcastTurnUpdateAsync(AgentTurnRequest request, AgentConversation conversation, AgentSession session) =>
        broadcastService.BroadcastTurnUpdateAsync(request.TenantId, conversation.CreatedById, new AgentTurnUpdateDto
        {
            ConversationId = conversation.Id,
            SessionId = session.Id,
            Status = session.Status,
            TotalTokensUsed = session.TotalTokensUsed,
            DecisionCount = session.DecisionCount,
            ErrorMessage = session.ErrorMessage
        });

    private async Task<IReadOnlySet<string>> ResolveCallerPermissionsAsync(
        AgentTurnRequest request, CancellationToken ct)
    {
        if (request.TriggeredByUserId is not { } userId)
            return new HashSet<string>();

        return await userPermissions.GetPermissionsAsync(userId, request.TenantId, ct);
    }
}

using Logistics.Application.Abstractions.Common;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Agents.Services;

/// <summary>
/// The conversation write path shared by the dispatch and copilot surfaces. Each surface keeps only
/// its own command, permission policy and feature gate; what the command then does lives here.
/// </summary>
internal interface IAgentConversationCommands : IApplicationService
{
    Task<Result<AgentConversationDto>> CreateAsync(
        AgentConversationKind kind, Guid? userId, CancellationToken ct);

    Task<Result> RenameAsync(
        AgentConversationScope scope, Guid conversationId, string title, CancellationToken ct);

    Task<Result> DeleteAsync(AgentConversationScope scope, Guid conversationId, CancellationToken ct);

    Task<Result> CancelTurnAsync(
        AgentConversationScope scope, Guid conversationId, CancellationToken ct);

    /// <summary>
    /// Appends the user message, opens the turn and hands it to the surface's background runner via
    /// <paramref name="enqueueTurn"/> (tenant id, conversation id, caller id).
    /// </summary>
    Task<Result<SendAgentMessageResultDto>> SendMessageAsync(
        AgentConversationScope scope,
        Guid conversationId,
        string text,
        Guid? userId,
        Action<Guid, Guid, Guid> enqueueTurn,
        CancellationToken ct);
}

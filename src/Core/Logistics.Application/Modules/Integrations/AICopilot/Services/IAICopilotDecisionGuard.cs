using Logistics.Application.Abstractions.Common;
using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AICopilot.Services;

internal sealed record CopilotDecisionContext(
    AgentDecision Decision,
    AgentConversation Conversation);

/// <summary>
/// Shared by copilot decision approval and rejection: the decision must exist, belong to a copilot
/// turn, still be Suggested, and its conversation must be owned by the caller.
/// </summary>
internal interface IAICopilotDecisionGuard : IApplicationService
{
    Task<Result<CopilotDecisionContext>> LoadAsync(Guid decisionId, Guid? userId, CancellationToken ct);
}

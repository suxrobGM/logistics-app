using Logistics.Application.Abstractions.AICopilot;

namespace Logistics.Infrastructure.AI.Agents.Copilot;

/// <summary>
/// Thin adapter from the copilot's port to the shared turn lifecycle: maps the copilot's own
/// request shape onto <see cref="AgentTurnRequest"/> and runs it through <see cref="AgentTurnService"/>
/// with the <see cref="CopilotAgentSurface"/>.
/// </summary>
internal sealed class AICopilotService(
    AgentTurnService turnService,
    CopilotAgentSurface surface) : IAICopilotService
{
    public Task RunTurnAsync(AICopilotTurnRequest request, CancellationToken ct = default) =>
        turnService.RunTurnAsync(
            new AgentTurnRequest(request.TenantId, request.ConversationId, request.UserId), surface, ct);
}

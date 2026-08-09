using Logistics.Application.Abstractions.Common;
using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Agents.Services;

/// <summary>
/// Records what a dispatcher did with a suggested decision in the conversation transcript, and
/// pushes the note through the surface's own broadcast.
/// </summary>
internal interface IAgentDecisionNotes : IApplicationService
{
    string RejectionNote(AgentDecision decision, string? reason);

    /// <summary>The conversation a decision's outcome belongs in.</summary>
    Task<AgentConversation> LoadConversationAsync(AgentDecision decision, CancellationToken ct);

    /// <summary>Appends <paramref name="note"/> to the transcript and saves.</summary>
    Task AppendAsync(
        AgentConversation conversation,
        string note,
        Func<AgentMessageDto, Task> broadcastMessageAsync,
        CancellationToken ct);
}

using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Agents;

/// <summary>
/// Records what a dispatcher did with a suggested decision in the conversation transcript, and
/// pushes the note through the surface's own broadcast.
/// </summary>
internal static class AgentDecisionNotes
{
    public static string RejectionNote(AgentDecision decision, string? reason) =>
        string.IsNullOrWhiteSpace(reason)
            ? $"Rejected: {decision.ToolName}"
            : $"Rejected: {decision.ToolName} - {reason}";

    /// <summary>The conversation a decision's outcome belongs in.</summary>
    public static async Task<AgentConversation> LoadConversationAsync(
        ITenantUnitOfWork tenantUow, AgentDecision decision, CancellationToken ct) =>
        // Required FK cascading from the conversation, so a live decision always has its row.
        (await tenantUow.Repository<AgentConversation>()
            .GetByIdAsync(decision.Session.ConversationId, ct))!;

    /// <summary>Appends <paramref name="note"/> to the transcript and saves.</summary>
    public static async Task AppendAsync(
        ITenantUnitOfWork tenantUow,
        AgentConversation conversation,
        string note,
        Func<AgentMessageDto, Task> broadcastMessageAsync,
        CancellationToken ct)
    {
        var message = conversation.AddTextMessage(AgentMessageRole.System, note);
        await tenantUow.Repository<AgentMessage>().AddAsync(message, ct);
        await tenantUow.SaveChangesAsync(ct);

        await broadcastMessageAsync(message.ToDto());
    }
}

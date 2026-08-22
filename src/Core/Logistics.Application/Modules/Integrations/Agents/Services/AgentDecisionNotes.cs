using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.Agents.Services;

internal sealed class AgentDecisionNotes(
    ITenantUnitOfWork tenantUow,
    IAgentSenderDirectory senderDirectory) : IAgentDecisionNotes
{
    public string RejectionNote(AgentDecision decision, string? reason) =>
        string.IsNullOrWhiteSpace(reason)
            ? $"Rejected: {decision.ToolName}"
            : $"Rejected: {decision.ToolName} - {reason}";

    public Task<AgentConversation> LoadConversationAsync(AgentDecision decision, CancellationToken ct) =>
        // Projected off the session query, not decision.Session, which lazy-loads the whole row
        // for one FK. Required FK, so a live decision always has its conversation.
        tenantUow.Repository<AgentSession>().Query()
            .Where(s => s.Id == decision.SessionId)
            .Select(s => s.Conversation)
            .FirstAsync(ct);

    public async Task AppendAsync(
        AgentConversation conversation,
        string note,
        Guid? actedByUserId,
        Func<AgentMessageDto, Task> broadcastMessageAsync,
        CancellationToken ct)
    {
        var message = conversation.AddTextMessage(AgentMessageRole.System, note, actedByUserId);
        await tenantUow.Repository<AgentMessage>().AddAsync(message, ct);
        await tenantUow.SaveChangesAsync(ct);

        var actorName = await senderDirectory.GetNameAsync(actedByUserId, ct);
        await broadcastMessageAsync(message.ToDto(actorName));
    }
}

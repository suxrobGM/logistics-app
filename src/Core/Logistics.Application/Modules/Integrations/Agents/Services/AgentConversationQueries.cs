using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.Agents.Services;

internal sealed class AgentConversationQueries(
    ITenantUnitOfWork tenantUow,
    IAgentConversationAccess access) : IAgentConversationQueries
{
    public async Task<PagedResult<AgentConversationDto>> ListAsync(
        AgentConversationScope scope, int page, int pageSize, CancellationToken ct)
    {
        if (scope.RequireOwner && scope.CallerId is null)
            return PagedResult<AgentConversationDto>.Fail("User is not authenticated");

        var query = access.Restrict(tenantUow.Repository<AgentConversation>().Query(), scope);

        var totalItems = await query.CountAsync(ct);

        var conversations = await query
            .OrderByDescending(c => c.LastMessageAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = conversations.Select(c => c.ToDto()).ToList();
        return PagedResult<AgentConversationDto>.Ok(dtos, totalItems, pageSize);
    }

    public async Task<Result<AgentConversationDto>> GetByIdAsync(
        AgentConversationScope scope, Guid conversationId, bool includeSessions, CancellationToken ct)
    {
        var conversation = await access.LoadAsync(conversationId, scope, ct);
        if (conversation is null)
            return Result<AgentConversationDto>.Fail("Conversation not found");

        var decisions = await tenantUow.Repository<AgentDecision>().Query()
            .Where(d => d.Session.ConversationId == conversation.Id)
            .OrderBy(d => d.CreatedAt)
            .ToListAsync(ct);

        // Null DisplayText marks a tool-result row: provider replay data, not chat content. Queried
        // rather than filtered off the navigation, which would materialize every excluded row's
        // ContentJson - the fleet snapshots and HOS payloads that dwarf the transcript itself.
        var messages = await tenantUow.Repository<AgentMessage>().Query()
            .Where(m => m.ConversationId == conversation.Id && m.DisplayText != null)
            .OrderBy(m => m.Sequence)
            .ToListAsync(ct);

        var dto = conversation.ToDto();
        dto.Messages = messages.Select(m => m.ToDto()).ToList();
        dto.Decisions = decisions.Select(d => d.ToDto()).ToList();

        if (includeSessions)
        {
            var sessions = await tenantUow.Repository<AgentSession>().Query()
                .Where(s => s.ConversationId == conversation.Id)
                .OrderBy(s => s.StartedAt)
                .ToListAsync(ct);

            dto.Sessions = sessions.Select(s => s.ToDto()).ToList();
        }

        return Result<AgentConversationDto>.Ok(dto);
    }
}

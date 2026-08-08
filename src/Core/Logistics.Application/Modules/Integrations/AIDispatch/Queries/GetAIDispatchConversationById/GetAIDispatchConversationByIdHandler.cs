using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Queries;

internal sealed class GetAIDispatchConversationByIdHandler(
    ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetAIDispatchConversationByIdQuery, Result<AgentConversationDto>>
{
    public async Task<Result<AgentConversationDto>> Handle(
        GetAIDispatchConversationByIdQuery request, CancellationToken ct)
    {
        var conversation = await tenantUow.Repository<AgentConversation>()
            .GetByIdAsync(request.Id, ct);

        if (conversation is null || conversation.Kind != AgentConversationKind.Dispatch)
            return Result<AgentConversationDto>.Fail("Conversation not found");

        var sessions = await tenantUow.Repository<AgentSession>().Query()
            .Where(s => s.ConversationId == conversation.Id)
            .OrderBy(s => s.StartedAt)
            .ToListAsync(ct);

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
        dto.Sessions = sessions.Select(s => s.ToDto()).ToList();

        return Result<AgentConversationDto>.Ok(dto);
    }
}

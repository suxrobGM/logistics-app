using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.AICopilot.Queries;

internal sealed class GetAICopilotConversationByIdHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser)
    : IAppRequestHandler<GetAICopilotConversationByIdQuery, Result<AgentConversationDto>>
{
    public async Task<Result<AgentConversationDto>> Handle(
        GetAICopilotConversationByIdQuery request, CancellationToken ct)
    {
        var userId = currentUser.GetUserId();
        var conversation = await tenantUow.Repository<AgentConversation>()
            .GetByIdAsync(request.Id, ct);

        if (conversation is null
            || conversation.CreatedById != userId
            || conversation.Kind != AgentConversationKind.Copilot)
        {
            return Result<AgentConversationDto>.Fail("Conversation not found");
        }

        var decisions = await tenantUow.Repository<AgentDecision>().Query()
            .Where(d => d.Session.ConversationId == conversation.Id)
            .OrderBy(d => d.CreatedAt)
            .ToListAsync(ct);

        // Null DisplayText marks a tool-result row: provider replay data, not chat content. Queried
        // rather than filtered off the navigation, which would materialize every excluded row's
        // ContentJson.
        var messages = await tenantUow.Repository<AgentMessage>().Query()
            .Where(m => m.ConversationId == conversation.Id && m.DisplayText != null)
            .OrderBy(m => m.Sequence)
            .ToListAsync(ct);

        var dto = conversation.ToDto();
        dto.Messages = messages.Select(m => m.ToDto()).ToList();
        dto.Decisions = decisions.Select(d => d.ToDto()).ToList();

        return Result<AgentConversationDto>.Ok(dto);
    }
}

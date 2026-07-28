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
    : IAppRequestHandler<GetAICopilotConversationByIdQuery, Result<AICopilotConversationDto>>
{
    public async Task<Result<AICopilotConversationDto>> Handle(
        GetAICopilotConversationByIdQuery request, CancellationToken ct)
    {
        var userId = currentUser.GetUserId();
        var conversation = await tenantUow.Repository<AICopilotConversation>()
            .GetByIdAsync(request.Id, ct);

        if (conversation is null || conversation.CreatedById != userId)
            return Result<AICopilotConversationDto>.Fail("Conversation not found");

        var decisions = await tenantUow.Repository<AIDispatchDecision>().Query()
            .Where(d => d.Session.ConversationId == conversation.Id)
            .OrderBy(d => d.CreatedAt)
            .ToListAsync(ct);

        var dto = conversation.ToDto();
        // Tool-result rows (null DisplayText) are provider replay data, not chat content.
        dto.Messages = conversation.Messages
            .Where(m => m.DisplayText != null)
            .OrderBy(m => m.Sequence)
            .Select(m => m.ToDto())
            .ToList();
        dto.Decisions = decisions.Select(d => d.ToDto()).ToList();

        return Result<AICopilotConversationDto>.Ok(dto);
    }
}

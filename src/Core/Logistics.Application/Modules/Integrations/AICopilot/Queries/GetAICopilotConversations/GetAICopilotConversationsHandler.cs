using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.AICopilot.Queries;

internal sealed class GetAICopilotConversationsHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser)
    : IAppRequestHandler<GetAICopilotConversationsQuery, PagedResult<AICopilotConversationDto>>
{
    public async Task<PagedResult<AICopilotConversationDto>> Handle(
        GetAICopilotConversationsQuery request, CancellationToken ct)
    {
        var userId = currentUser.GetUserId();
        if (userId is null)
            return PagedResult<AICopilotConversationDto>.Fail("User is not authenticated");

        var query = tenantUow.Repository<AICopilotConversation>().Query()
            .Where(c => c.CreatedById == userId.Value);

        var totalItems = await query.CountAsync(ct);

        var conversations = await query
            .OrderByDescending(c => c.LastMessageAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var dtos = conversations.Select(c => c.ToDto()).ToList();
        return PagedResult<AICopilotConversationDto>.Ok(dtos, totalItems, request.PageSize);
    }
}

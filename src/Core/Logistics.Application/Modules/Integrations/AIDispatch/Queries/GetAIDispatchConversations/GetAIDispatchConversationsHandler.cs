using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Queries;

/// <summary>Tenant-shared: every conversation with Kind == Dispatch, regardless of who created it.</summary>
internal sealed class GetAIDispatchConversationsHandler(
    ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetAIDispatchConversationsQuery, PagedResult<AgentConversationDto>>
{
    public async Task<PagedResult<AgentConversationDto>> Handle(
        GetAIDispatchConversationsQuery request, CancellationToken ct)
    {
        var query = tenantUow.Repository<AgentConversation>().Query()
            .Where(c => c.Kind == AgentConversationKind.Dispatch);

        var totalItems = await query.CountAsync(ct);

        var conversations = await query
            .OrderByDescending(c => c.LastMessageAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var dtos = conversations.Select(c => c.ToDto()).ToList();
        return PagedResult<AgentConversationDto>.Ok(dtos, totalItems, request.PageSize);
    }
}

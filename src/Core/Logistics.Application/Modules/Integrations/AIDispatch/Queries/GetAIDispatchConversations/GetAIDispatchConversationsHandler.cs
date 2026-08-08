using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Integrations.Agents;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Queries;

/// <summary>Tenant-shared: every conversation with Kind == Dispatch, regardless of who created it.</summary>
internal sealed class GetAIDispatchConversationsHandler(
    ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetAIDispatchConversationsQuery, PagedResult<AgentConversationDto>>
{
    public Task<PagedResult<AgentConversationDto>> Handle(
        GetAIDispatchConversationsQuery request, CancellationToken ct) =>
        AgentConversationQueries.ListAsync(
            tenantUow, AgentConversationScope.Dispatch, request.Page, request.PageSize, ct);
}

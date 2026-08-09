using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Integrations.Agents;
using Logistics.Application.Modules.Integrations.Agents.Services;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Queries;

/// <summary>Tenant-shared: every conversation with Kind == Dispatch, regardless of who created it.</summary>
internal sealed class GetAIDispatchConversationsHandler(
    IAgentConversationQueries queries)
    : IAppRequestHandler<GetAIDispatchConversationsQuery, PagedResult<AgentConversationDto>>
{
    public Task<PagedResult<AgentConversationDto>> Handle(
        GetAIDispatchConversationsQuery request, CancellationToken ct) =>
        queries.ListAsync(AgentConversationScope.Dispatch, request.Page, request.PageSize, ct);
}

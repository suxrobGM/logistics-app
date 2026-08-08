using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Integrations.Agents;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Queries;

internal sealed class GetAIDispatchConversationByIdHandler(
    ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetAIDispatchConversationByIdQuery, Result<AgentConversationDto>>
{
    public Task<Result<AgentConversationDto>> Handle(
        GetAIDispatchConversationByIdQuery request, CancellationToken ct) =>
        AgentConversationQueries.GetByIdAsync(
            tenantUow, AgentConversationScope.Dispatch, request.Id, includeSessions: true, ct);
}

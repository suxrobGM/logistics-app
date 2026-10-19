using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Integrations.Agents.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Queries;

internal sealed class GetPendingDecisionsHandler(
    ITenantUnitOfWork tenantUow,
    IAgentDecisionDtoMapper decisionDtoMapper) : IRequestHandler<GetPendingDecisionsQuery, Result<List<AgentDecisionDto>>>
{
    public async Task<Result<List<AgentDecisionDto>>> Handle(
        GetPendingDecisionsQuery request, CancellationToken ct)
    {
        // Copilot suggestions surface in the chat drawer, not on the dispatch board.
        var decisions = await tenantUow.Repository<AgentDecision>()
            .Query()
            .DispatchOnly()
            .Where(d => d.Status == AgentDecisionStatus.Suggested)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);

        var dtos = await decisionDtoMapper.MapAsync(decisions, ct);
        return Result<List<AgentDecisionDto>>.Ok(dtos);
    }
}

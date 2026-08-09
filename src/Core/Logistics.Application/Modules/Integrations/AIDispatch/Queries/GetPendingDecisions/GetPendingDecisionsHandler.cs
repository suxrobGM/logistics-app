using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Queries;

internal sealed class GetPendingDecisionsHandler(
    ITenantUnitOfWork tenantUow) : IAppRequestHandler<GetPendingDecisionsQuery, Result<List<AgentDecisionDto>>>
{
    public async Task<Result<List<AgentDecisionDto>>> Handle(
        GetPendingDecisionsQuery request, CancellationToken ct)
    {
        // Copilot suggestions surface in the chat drawer, not on the dispatch board.
        var decisions = await tenantUow.Repository<AgentDecision>()
            .Query()
            .DispatchOnly()
            .Where(d => d.Status == AgentDecisionStatus.Suggested)
            .ToListAsync(ct);

        // Batch-load load names and truck numbers to avoid N+1
        var loadIds = decisions.Where(d => d.LoadId is not null).Select(d => d.LoadId!.Value).Distinct().ToList();
        var truckIds = decisions.Where(d => d.TruckId is not null).Select(d => d.TruckId!.Value).Distinct().ToList();

        var loadNames = loadIds.Count > 0
            ? (await tenantUow.Repository<Load>().GetListAsync(l => loadIds.Contains(l.Id), ct))
                .ToDictionary(l => l.Id, l => l.Name)
            : [];

        var truckNumbers = truckIds.Count > 0
            ? (await tenantUow.Repository<Truck>().GetListAsync(t => truckIds.Contains(t.Id), ct))
                .ToDictionary(t => t.Id, t => t.Number)
            : [];

        var dtos = decisions
            .OrderByDescending(d => d.CreatedAt)
            .Select(d =>
            {
                var dto = d.ToDto();
                dto.LoadName = d.LoadId is not null && loadNames.TryGetValue(d.LoadId.Value, out var ln) ? ln : null;
                dto.TruckNumber = d.TruckId is not null && truckNumbers.TryGetValue(d.TruckId.Value, out var tn) ? tn : null;
                return dto;
            })
            .ToList();

        return Result<List<AgentDecisionDto>>.Ok(dtos);
    }
}

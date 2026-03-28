using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetPendingDecisionsHandler(
    ITenantUnitOfWork tenantUow) : IAppRequestHandler<GetPendingDecisionsQuery, Result<List<DispatchDecisionDto>>>
{
    public async Task<Result<List<DispatchDecisionDto>>> Handle(
        GetPendingDecisionsQuery request, CancellationToken ct)
    {
        var decisions = await tenantUow.Repository<DispatchDecision>()
            .GetListAsync(d => d.Status == DispatchDecisionStatus.Suggested, ct);

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
            .Select(d => new DispatchDecisionDto
            {
                Id = d.Id,
                SessionId = d.SessionId,
                Type = d.Type,
                Status = d.Status,
                Reasoning = d.Reasoning,
                ToolName = d.ToolName,
                ToolInput = d.ToolInput,
                LoadId = d.LoadId,
                LoadName = d.LoadId is not null && loadNames.TryGetValue(d.LoadId.Value, out var ln) ? ln : null,
                TruckId = d.TruckId,
                TruckNumber = d.TruckId is not null && truckNumbers.TryGetValue(d.TruckId.Value, out var tn) ? tn : null,
                TripId = d.TripId,
                CreatedAt = d.CreatedAt
            })
            .ToList();

        return Result<List<DispatchDecisionDto>>.Ok(dtos);
    }
}

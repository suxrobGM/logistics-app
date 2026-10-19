using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.Agents.Services;

public sealed class AgentDecisionDtoMapper(ITenantUnitOfWork tenantUow) : IAgentDecisionDtoMapper
{
    public async Task<List<AgentDecisionDto>> MapAsync(IReadOnlyCollection<AgentDecision> decisions, CancellationToken ct)
    {
        var loadIds = decisions.Select(d => d.LoadId).OfType<Guid>().Distinct().ToList();
        var truckIds = decisions.Select(d => d.TruckId).OfType<Guid>().Distinct().ToList();

        var loadNames = loadIds.Count > 0
            ? await tenantUow.Repository<Load>().Query()
                .Where(l => loadIds.Contains(l.Id))
                .Select(l => new { l.Id, l.Name })
                .ToDictionaryAsync(l => l.Id, l => l.Name, ct)
            : [];

        var truckNumbers = truckIds.Count > 0
            ? await tenantUow.Repository<Truck>().Query()
                .Where(t => truckIds.Contains(t.Id))
                .Select(t => new { t.Id, t.Number })
                .ToDictionaryAsync(t => t.Id, t => t.Number, ct)
            : [];

        return decisions
            .Select(d => d.ToDto(
                d.LoadId is { } loadId ? loadNames.GetValueOrDefault(loadId) : null,
                d.TruckId is { } truckId ? truckNumbers.GetValueOrDefault(truckId) : null))
            .ToList();
    }

    public async Task<AgentDecisionDto> MapAsync(AgentDecision decision, CancellationToken ct) =>
        (await MapAsync([decision], ct))[0];
}

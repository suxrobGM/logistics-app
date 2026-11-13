using Logistics.Application.Common.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.Agents.Services;

public sealed class AgentDecisionDtoMapper(ITenantUnitOfWork tenantUow, IDisplayNameLookup names) : IAgentDecisionDtoMapper
{
    public async Task<List<AgentDecisionDto>> MapAsync(IReadOnlyCollection<AgentDecision> decisions, CancellationToken ct)
    {
        var loadIds = decisions.Select(d => d.LoadId).OfType<Guid>().Distinct().ToList();

        var loadNames = loadIds.Count > 0
            ? await tenantUow.Repository<Load>().Query()
                .Where(l => loadIds.Contains(l.Id))
                .Select(l => new { l.Id, l.Name })
                .ToDictionaryAsync(l => l.Id, l => l.Name, ct)
            : [];

        var truckNumbers = await names.GetTruckNumbersAsync(decisions.Select(d => d.TruckId), ct);

        return decisions
            .Select(d => d.ToDto(
                d.LoadId is { } loadId ? loadNames.GetValueOrDefault(loadId) : null,
                truckNumbers.Find(d.TruckId)))
            .ToList();
    }

    public async Task<AgentDecisionDto> MapAsync(AgentDecision decision, CancellationToken ct) =>
        (await MapAsync([decision], ct))[0];
}

using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.Agents.Services;

/// <summary>
/// The one way to turn decisions into DTOs. Decision cards label their load and truck from
/// <see cref="AgentDecisionDto.LoadName"/> and <see cref="AgentDecisionDto.TruckNumber"/> and fall
/// back to the raw ids in the tool input, so a bare <c>ToDto()</c> shows GUIDs.
/// </summary>
public static class AgentDecisionDtos
{
    public static async Task<List<AgentDecisionDto>> ToLabeledDtosAsync(
        this ITenantUnitOfWork tenantUow, IReadOnlyCollection<AgentDecision> decisions, CancellationToken ct)
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

    public static async Task<AgentDecisionDto> ToLabeledDtoAsync(
        this ITenantUnitOfWork tenantUow, AgentDecision decision, CancellationToken ct) =>
        (await tenantUow.ToLabeledDtosAsync([decision], ct))[0];
}

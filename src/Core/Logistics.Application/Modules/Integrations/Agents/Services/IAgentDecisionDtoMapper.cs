using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Agents.Services;

/// <summary>
/// The one way to turn decisions into DTOs. Decision cards label their load and truck from
/// <see cref="AgentDecisionDto.LoadName"/> and <see cref="AgentDecisionDto.TruckNumber"/> and fall
/// back to the raw ids in the tool input, so a bare <c>ToDto()</c> shows GUIDs.
/// </summary>
public interface IAgentDecisionDtoMapper : IApplicationService
{
    Task<List<AgentDecisionDto>> MapAsync(IReadOnlyCollection<AgentDecision> decisions, CancellationToken ct);

    Task<AgentDecisionDto> MapAsync(AgentDecision decision, CancellationToken ct);
}

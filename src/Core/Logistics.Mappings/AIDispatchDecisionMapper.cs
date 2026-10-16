using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class AIDispatchDecisionMapper
{
    public static AgentDecisionDto ToDto(this AgentDecision entity)
    {
        var dto = entity.ToDtoCore();
        dto.ToolResult = AgentToolResultJson.Deserialize(entity.ToolOutput);
        return dto;
    }

    public static AgentDecisionDto ToDto(this AgentDecision entity, string? loadName, string? truckNumber)
    {
        var dto = entity.ToDto();
        dto.LoadName = loadName;
        dto.TruckNumber = truckNumber;
        return dto;
    }

    [MapperIgnoreSource(nameof(AgentDecision.DomainEvents))]
    [MapperIgnoreSource(nameof(AgentDecision.Session))]
    [MapperIgnoreSource(nameof(AgentDecision.ToolOutput))]
    [MapperIgnoreTarget(nameof(AgentDecisionDto.LoadName))]
    [MapperIgnoreTarget(nameof(AgentDecisionDto.TruckNumber))]
    [MapperIgnoreTarget(nameof(AgentDecisionDto.ToolResult))]
    private static partial AgentDecisionDto ToDtoCore(this AgentDecision entity);
}

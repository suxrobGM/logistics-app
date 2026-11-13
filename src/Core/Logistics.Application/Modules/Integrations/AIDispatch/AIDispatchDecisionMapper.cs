using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.Integrations.AIDispatch;

[Mapper]
public static partial class AIDispatchDecisionMapper
{
    extension(AgentDecision entity)
    {
        public AgentDecisionDto ToDto()
        {
            var dto = entity.ToDtoCore();
            dto.ToolResult = AgentToolResultJson.Deserialize(entity.ToolOutput);
            return dto;
        }

        public AgentDecisionDto ToDto(string? loadName, string? truckNumber)
        {
            var dto = entity.ToDto();
            dto.LoadName = loadName;
            dto.TruckNumber = truckNumber;
            return dto;
        }
    }

    [MapperIgnoreSource(nameof(AgentDecision.Session))]
    [MapperIgnoreTarget(nameof(AgentDecisionDto.LoadName))]
    [MapperIgnoreTarget(nameof(AgentDecisionDto.TruckNumber))]
    [MapperIgnoreTarget(nameof(AgentDecisionDto.ToolResult))]
    private static partial AgentDecisionDto ToDtoCore(this AgentDecision entity);
}

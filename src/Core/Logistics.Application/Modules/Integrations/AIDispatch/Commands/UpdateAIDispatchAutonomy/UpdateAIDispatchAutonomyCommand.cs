using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

[RequiresFeature(TenantFeature.AgenticDispatch)]
public class UpdateAIDispatchAutonomyCommand : ICommand
{
    public List<AgentDecisionType> AutoApproveTypes { get; set; } = [];
    public int AutoApproveStreak { get; set; }
    public int MinutesSavedPerDecision { get; set; }
}

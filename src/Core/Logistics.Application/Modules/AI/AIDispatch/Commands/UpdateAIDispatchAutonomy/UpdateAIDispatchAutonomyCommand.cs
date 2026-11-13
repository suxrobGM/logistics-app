using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.AI.AIDispatch.Commands;

[RequiresFeature(TenantFeature.AIDispatch)]
public class UpdateAIDispatchAutonomyCommand : ICommand
{
    public List<AgentDecisionType> AutoApproveTypes { get; set; } = [];
    public int AutoApproveStreak { get; set; }
    public int MinutesSavedPerDecision { get; set; }
}

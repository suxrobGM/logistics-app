using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.AI.AICopilot.Commands;

[RequiresFeature(TenantFeature.AICopilot)]
public class ApproveAICopilotDecisionCommand : ICommand
{
    public Guid DecisionId { get; set; }
}

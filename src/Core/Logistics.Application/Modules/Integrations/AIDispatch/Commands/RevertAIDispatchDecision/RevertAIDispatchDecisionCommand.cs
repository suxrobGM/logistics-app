using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

/// <summary>Undoes a decision the agent executed on its own, inside the revert window.</summary>
[RequiresFeature(TenantFeature.AgenticDispatch)]
public class RevertAIDispatchDecisionCommand : ICommand
{
    public Guid DecisionId { get; set; }
}

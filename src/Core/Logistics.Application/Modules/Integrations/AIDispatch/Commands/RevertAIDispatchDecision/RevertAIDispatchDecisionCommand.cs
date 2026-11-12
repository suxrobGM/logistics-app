using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

[RequiresFeature(TenantFeature.AgenticDispatch)]
public class RevertAIDispatchDecisionCommand : ICommand
{
    public Guid DecisionId { get; set; }
}

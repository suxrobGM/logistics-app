using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.AI.AIDispatch.Commands;

[RequiresFeature(TenantFeature.AIDispatch)]
public class RevertAIDispatchDecisionCommand : ICommand
{
    public Guid DecisionId { get; set; }
}

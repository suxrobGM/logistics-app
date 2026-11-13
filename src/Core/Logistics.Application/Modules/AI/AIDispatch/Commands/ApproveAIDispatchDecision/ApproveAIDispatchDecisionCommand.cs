using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.AI.AIDispatch.Commands;

[RequiresFeature(TenantFeature.AgenticDispatch)]
public class ApproveAIDispatchDecisionCommand : ICommand
{
    public Guid DecisionId { get; set; }
}

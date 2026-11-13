using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.AI.AIDispatch.Commands;

[RequiresFeature(TenantFeature.AIDispatch)]
public class RejectAIDispatchDecisionCommand : ICommand
{
    public Guid DecisionId { get; set; }
    public string? Reason { get; set; }
}

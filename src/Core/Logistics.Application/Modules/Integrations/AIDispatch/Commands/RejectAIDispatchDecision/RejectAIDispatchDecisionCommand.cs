using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

[RequiresFeature(TenantFeature.AgenticDispatch)]
public class RejectAIDispatchDecisionCommand : ICommand
{
    public Guid DecisionId { get; set; }
    public string? Reason { get; set; }
}

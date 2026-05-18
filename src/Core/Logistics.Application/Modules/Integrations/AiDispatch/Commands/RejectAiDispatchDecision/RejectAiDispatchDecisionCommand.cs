using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.AiDispatch.Commands;

[RequiresFeature(TenantFeature.AgenticDispatch)]
public class RejectAiDispatchDecisionCommand : ICommand
{
    public Guid DecisionId { get; set; }
    public string? Reason { get; set; }
}

using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.AICopilot.Commands;

[RequiresFeature(TenantFeature.AICopilot)]
public class RejectAICopilotDecisionCommand : ICommand
{
    public Guid DecisionId { get; set; }
    public string? Reason { get; set; }
}

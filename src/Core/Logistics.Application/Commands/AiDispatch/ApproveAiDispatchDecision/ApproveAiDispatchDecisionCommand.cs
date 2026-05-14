using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

[RequiresFeature(TenantFeature.AgenticDispatch)]
public class ApproveAiDispatchDecisionCommand : ICommand
{
    public Guid DecisionId { get; set; }
}

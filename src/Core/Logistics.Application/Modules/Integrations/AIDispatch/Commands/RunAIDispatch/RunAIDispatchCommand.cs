using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

[RequiresFeature(TenantFeature.AgenticDispatch)]
public class RunAIDispatchCommand : ICommand<Result<Guid>>
{
    public AIDispatchMode Mode { get; set; } = AIDispatchMode.HumanInTheLoop;
    public string? Instructions { get; set; }
}

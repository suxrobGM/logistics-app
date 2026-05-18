using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AiDispatch.Commands;

[RequiresFeature(TenantFeature.AgenticDispatch)]
public class RunAiDispatchCommand : ICommand<Result<Guid>>
{
    public AiDispatchMode Mode { get; set; } = AiDispatchMode.HumanInTheLoop;
    public string? Instructions { get; set; }
}

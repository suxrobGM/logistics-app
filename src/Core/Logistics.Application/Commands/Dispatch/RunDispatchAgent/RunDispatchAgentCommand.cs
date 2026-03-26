using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

[RequiresFeature(TenantFeature.AgenticDispatch)]
public class RunDispatchAgentCommand : IAppRequest<Result<Guid>>
{
    public DispatchAgentMode Mode { get; set; } = DispatchAgentMode.HumanInTheLoop;
}

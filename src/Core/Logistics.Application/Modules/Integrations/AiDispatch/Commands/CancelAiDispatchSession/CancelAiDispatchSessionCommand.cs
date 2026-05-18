using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.AiDispatch.Commands;

[RequiresFeature(TenantFeature.AgenticDispatch)]
public class CancelAiDispatchSessionCommand : ICommand
{
    public Guid SessionId { get; set; }
}

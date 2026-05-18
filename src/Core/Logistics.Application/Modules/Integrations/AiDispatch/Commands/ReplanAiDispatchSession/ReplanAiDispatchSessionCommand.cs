using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AiDispatch.Commands;

[RequiresFeature(TenantFeature.AgenticDispatch)]
public class ReplanAiDispatchSessionCommand : ICommand<Result<Guid>>
{
    public Guid OriginalSessionId { get; set; }
    public string? AdditionalInstructions { get; set; }
}

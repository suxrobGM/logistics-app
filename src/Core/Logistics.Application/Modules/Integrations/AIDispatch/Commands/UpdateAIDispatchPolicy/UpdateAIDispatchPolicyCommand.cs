using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

[RequiresFeature(TenantFeature.AgenticDispatch)]
public class UpdateAIDispatchPolicyCommand : ICommand
{
    /// <summary>Dispatcher directives. Null or empty clears them; the learned section is untouched.</summary>
    public string? ManualContent { get; set; }

    /// <summary>False pauses both prompt injection and nightly learning.</summary>
    public bool IsEnabled { get; set; } = true;
}

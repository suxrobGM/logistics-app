using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.AICopilot.Commands;

[RequiresFeature(TenantFeature.AICopilot)]
public class RenameAICopilotConversationCommand : ICommand
{
    public Guid ConversationId { get; set; }
    public string Title { get; set; } = "";
}

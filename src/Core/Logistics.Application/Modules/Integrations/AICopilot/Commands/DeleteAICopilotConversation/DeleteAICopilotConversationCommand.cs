using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.AICopilot.Commands;

[RequiresFeature(TenantFeature.AICopilot)]
public class DeleteAICopilotConversationCommand : ICommand
{
    public Guid ConversationId { get; set; }
}

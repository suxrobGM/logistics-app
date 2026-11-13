using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

[RequiresFeature(TenantFeature.AgenticDispatch)]
public class RenameAIDispatchConversationCommand : ICommand
{
    public Guid ConversationId { get; set; }
    public string Title { get; set; } = "";
}

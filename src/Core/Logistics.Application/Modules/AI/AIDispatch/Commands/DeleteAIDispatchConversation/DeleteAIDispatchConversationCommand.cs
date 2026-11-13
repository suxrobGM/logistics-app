using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.AI.AIDispatch.Commands;

[RequiresFeature(TenantFeature.AIDispatch)]
public class DeleteAIDispatchConversationCommand : ICommand
{
    public Guid ConversationId { get; set; }
}

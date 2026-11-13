using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.AI.AIDispatch.Commands;

[RequiresFeature(TenantFeature.AgenticDispatch)]
public class CancelAIDispatchTurnCommand : ICommand
{
    public Guid ConversationId { get; set; }
}

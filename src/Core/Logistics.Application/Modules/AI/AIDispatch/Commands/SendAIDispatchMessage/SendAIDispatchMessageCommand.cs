using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.AIDispatch.Commands;

[RequiresFeature(TenantFeature.AgenticDispatch)]
public class SendAIDispatchMessageCommand : ICommand<Result<SendAgentMessageResultDto>>
{
    public Guid ConversationId { get; set; }
    public string Text { get; set; } = "";
}

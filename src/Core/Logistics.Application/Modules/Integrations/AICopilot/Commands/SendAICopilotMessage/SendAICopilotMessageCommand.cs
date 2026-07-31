using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AICopilot.Commands;

[RequiresFeature(TenantFeature.AICopilot)]
public class SendAICopilotMessageCommand : ICommand<Result<SendAICopilotMessageResultDto>>
{
    public Guid ConversationId { get; set; }
    public string Text { get; set; } = "";
}

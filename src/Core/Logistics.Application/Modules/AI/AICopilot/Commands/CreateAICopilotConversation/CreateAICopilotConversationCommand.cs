using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.AICopilot.Commands;

[RequiresFeature(TenantFeature.AICopilot)]
public class CreateAICopilotConversationCommand : ICommand<Result<AgentConversationDto>>;

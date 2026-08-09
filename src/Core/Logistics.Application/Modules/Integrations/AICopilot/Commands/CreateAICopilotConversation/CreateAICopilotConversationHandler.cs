using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Integrations.Agents.Services;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AICopilot.Commands;

internal sealed class CreateAICopilotConversationHandler(
    IAgentConversationCommands commands,
    ICurrentUserService currentUser)
    : IAppRequestHandler<CreateAICopilotConversationCommand, Result<AgentConversationDto>>
{
    public Task<Result<AgentConversationDto>> Handle(
        CreateAICopilotConversationCommand request, CancellationToken ct) =>
        commands.CreateAsync(AgentConversationKind.Copilot, currentUser.GetUserId(), ct);
}

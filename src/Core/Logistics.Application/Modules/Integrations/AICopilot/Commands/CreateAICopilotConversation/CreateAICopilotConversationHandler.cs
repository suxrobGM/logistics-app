using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AICopilot.Commands;

internal sealed class CreateAICopilotConversationHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser) : IAppRequestHandler<CreateAICopilotConversationCommand, Result<AgentConversationDto>>
{
    public async Task<Result<AgentConversationDto>> Handle(
        CreateAICopilotConversationCommand request, CancellationToken ct)
    {
        var userId = currentUser.GetUserId();
        if (userId is null)
            return Result<AgentConversationDto>.Fail("User is not authenticated");

        var conversation = new AgentConversation { CreatedById = userId.Value };
        await tenantUow.Repository<AgentConversation>().AddAsync(conversation, ct);
        await tenantUow.SaveChangesAsync(ct);

        return Result<AgentConversationDto>.Ok(conversation.ToDto());
    }
}

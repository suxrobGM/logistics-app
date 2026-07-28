using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AICopilot.Commands;

internal sealed class CreateAICopilotConversationHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser) : IAppRequestHandler<CreateAICopilotConversationCommand, Result<AICopilotConversationDto>>
{
    public async Task<Result<AICopilotConversationDto>> Handle(
        CreateAICopilotConversationCommand request, CancellationToken ct)
    {
        var userId = currentUser.GetUserId();
        if (userId is null)
            return Result<AICopilotConversationDto>.Fail("User is not authenticated");

        var conversation = new AICopilotConversation { CreatedById = userId.Value };
        await tenantUow.Repository<AICopilotConversation>().AddAsync(conversation, ct);
        await tenantUow.SaveChangesAsync(ct);

        return Result<AICopilotConversationDto>.Ok(conversation.ToDto());
    }
}

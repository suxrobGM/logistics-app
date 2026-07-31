using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AICopilot.Commands;

internal sealed class RenameAICopilotConversationHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser) : IAppRequestHandler<RenameAICopilotConversationCommand, Result>
{
    public async Task<Result> Handle(RenameAICopilotConversationCommand request, CancellationToken ct)
    {
        var userId = currentUser.GetUserId();
        var conversation = await tenantUow.Repository<AICopilotConversation>()
            .GetByIdAsync(request.ConversationId, ct);

        if (conversation is null || conversation.CreatedById != userId)
            return Result.Fail("Conversation not found");

        conversation.Title = request.Title.Trim();
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}

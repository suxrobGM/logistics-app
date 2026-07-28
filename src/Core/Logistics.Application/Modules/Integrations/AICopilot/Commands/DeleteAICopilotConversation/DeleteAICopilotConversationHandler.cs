using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AICopilot.Commands;

internal sealed class DeleteAICopilotConversationHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser) : IAppRequestHandler<DeleteAICopilotConversationCommand, Result>
{
    public async Task<Result> Handle(DeleteAICopilotConversationCommand request, CancellationToken ct)
    {
        var userId = currentUser.GetUserId();
        var repo = tenantUow.Repository<AICopilotConversation>();
        var conversation = await repo.GetByIdAsync(request.ConversationId, ct);

        if (conversation is null || conversation.CreatedById != userId)
            return Result.Fail("Conversation not found");

        if (conversation.Status == AICopilotConversationStatus.Running)
            return Result.Fail("Cannot delete a conversation while a turn is running");

        // Cascades to messages, turn sessions, and their decisions.
        repo.Delete(conversation);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}

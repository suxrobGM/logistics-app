using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

internal sealed class DeleteAIDispatchConversationHandler(
    ITenantUnitOfWork tenantUow) : IAppRequestHandler<DeleteAIDispatchConversationCommand, Result>
{
    public async Task<Result> Handle(DeleteAIDispatchConversationCommand request, CancellationToken ct)
    {
        var repo = tenantUow.Repository<AgentConversation>();
        var conversation = await repo.GetByIdAsync(request.ConversationId, ct);

        if (conversation is null || conversation.Kind != AgentConversationKind.Dispatch)
            return Result.Fail("Conversation not found");

        if (conversation.Status == AgentConversationStatus.Running)
            return Result.Fail("Cannot delete a conversation while a turn is running");

        // Cascades to messages, turn sessions, and their decisions.
        repo.Delete(conversation);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}

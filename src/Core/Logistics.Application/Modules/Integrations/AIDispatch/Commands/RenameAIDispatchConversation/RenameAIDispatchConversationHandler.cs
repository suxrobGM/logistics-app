using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

internal sealed class RenameAIDispatchConversationHandler(
    ITenantUnitOfWork tenantUow) : IAppRequestHandler<RenameAIDispatchConversationCommand, Result>
{
    public async Task<Result> Handle(RenameAIDispatchConversationCommand request, CancellationToken ct)
    {
        var conversation = await tenantUow.Repository<AgentConversation>()
            .GetByIdAsync(request.ConversationId, ct);

        if (conversation is null || conversation.Kind != AgentConversationKind.Dispatch)
            return Result.Fail("Conversation not found");

        conversation.Title = request.Title.Trim();
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}

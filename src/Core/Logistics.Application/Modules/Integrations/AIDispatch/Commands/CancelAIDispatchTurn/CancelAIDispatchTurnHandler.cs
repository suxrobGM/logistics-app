using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

internal sealed class CancelAIDispatchTurnHandler(
    ITenantUnitOfWork tenantUow,
    IAIDispatchService dispatchService) : IAppRequestHandler<CancelAIDispatchTurnCommand, Result>
{
    public async Task<Result> Handle(CancelAIDispatchTurnCommand request, CancellationToken ct)
    {
        var conversation = await tenantUow.Repository<AgentConversation>()
            .GetByIdAsync(request.ConversationId, ct);

        if (conversation is null || conversation.Kind != AgentConversationKind.Dispatch)
            return Result.Fail("Conversation not found");

        var runningSessionId = await tenantUow.Repository<AgentSession>().Query()
            .Where(s => s.ConversationId == conversation.Id && s.Status == AgentSessionStatus.Running)
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync(ct);

        // Cancellation is cooperative - the turn's own finally block calls EndTurn. Only a turn
        // with no live session needs unsticking here.
        if (runningSessionId is { } sessionId)
        {
            await dispatchService.CancelAsync(sessionId, ct);
            return Result.Ok();
        }

        conversation.EndTurn();
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}

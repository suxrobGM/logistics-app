using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.AICopilot.Commands;

internal sealed class CancelAICopilotTurnHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser,
    IAIDispatchService dispatchService) : IAppRequestHandler<CancelAICopilotTurnCommand, Result>
{
    public async Task<Result> Handle(CancelAICopilotTurnCommand request, CancellationToken ct)
    {
        var userId = currentUser.GetUserId();
        var conversation = await tenantUow.Repository<AICopilotConversation>()
            .GetByIdAsync(request.ConversationId, ct);

        if (conversation is null || conversation.CreatedById != userId)
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

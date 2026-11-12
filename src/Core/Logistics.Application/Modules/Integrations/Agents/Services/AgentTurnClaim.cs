using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.Agents.Services;

internal sealed class AgentTurnClaim(ITenantUnitOfWork tenantUow) : IAgentTurnClaim
{
    public async Task<bool> TryClaimAsync(AgentConversation conversation, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var staleBefore = now - AgentConversation.StaleTurnWindow;

        var claimed = await tenantUow.Repository<AgentConversation>().Query()
            .Where(c => c.Id == conversation.Id)
            .Where(c => c.Status != AgentConversationStatus.Running
                        || c.TurnStartedAt == null
                        || c.TurnStartedAt < staleBefore)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.Status, AgentConversationStatus.Running)
                .SetProperty(c => c.TurnStartedAt, now), ct);

        return claimed == 1;
    }
}

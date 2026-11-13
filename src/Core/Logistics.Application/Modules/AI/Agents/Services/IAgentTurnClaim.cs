using Logistics.Application.Abstractions.Common;
using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.AI.Agents.Services;

internal interface IAgentTurnClaim : IApplicationService
{
    /// <summary>
    /// Marks the conversation Running unless a turn younger than
    /// <see cref="AgentConversation.StaleTurnWindow"/> already holds it. One conditional UPDATE, so two
    /// callers racing on the same conversation cannot both win.
    /// </summary>
    Task<bool> TryClaimAsync(AgentConversation conversation, CancellationToken ct);
}

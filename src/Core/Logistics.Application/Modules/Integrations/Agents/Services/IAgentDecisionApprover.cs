using Logistics.Application.Abstractions.Common;
using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Agents.Services;

internal interface IAgentDecisionApprover : IApplicationService
{
    /// <summary>
    /// Approves <paramref name="decision" /> for <paramref name="userId" />, runs its tool and appends the outcome to
    /// the decision's conversation. Fails without touching the decision when the approver lacks the tool's own
    /// permission - a surface's Manage permission is not enough to, say, send an invoice. A tool failure marks the
    /// decision Failed and is returned; the note is written either way.
    /// </summary>
    Task<Result> ApproveAsync(
        AgentDecision decision,
        Guid userId,
        Guid tenantId,
        Func<AgentMessageDto, Task> broadcastMessageAsync,
        CancellationToken ct);
}

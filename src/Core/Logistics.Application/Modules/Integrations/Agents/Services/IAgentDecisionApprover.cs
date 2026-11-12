using Logistics.Application.Abstractions.Common;
using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Agents.Services;

internal interface IAgentDecisionApprover : IApplicationService
{
    /// <summary>
    /// Requires the tool's own permission, not just the surface's Manage. A tool failure marks the decision Failed;
    /// the outcome note is written either way.
    /// </summary>
    Task<Result> ApproveAsync(
        AgentDecision decision,
        Guid userId,
        Guid tenantId,
        Func<AgentMessageDto, Task> broadcastMessageAsync,
        CancellationToken ct);
}
